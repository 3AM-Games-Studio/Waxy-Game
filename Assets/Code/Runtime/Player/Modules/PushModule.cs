using System;
using System.Threading.Tasks;
using AdvancedController;
using UnityEngine;

namespace Player.Modules
{
    public class PushModule
    {
        private PlayerController _controller;
        private Transform _camera;

        private PushableBox _candidateBox;
        private Collider _candidateCollider;
        private PushableBox _currentBox;

        public bool IsPushing => _isPushing;
        private bool _isPushing;
        private InputReader _input;
        private Transform _playerTransform;
        private float _pushCooldown = 0f;
        private const float MinPushTime = 0.1f;
        private Vector3 _playerMoveDirection;
        public bool ShouldRotateMesh { get; private set; }
        public Vector3 DesiredMeshForward { get; private set; }
        public Vector3 GetPlayerMoveDirection() => _playerMoveDirection;

        public void Initialize(PlayerController controller, InputReader input, Transform camera)
        {
            _controller = controller;
            _input = input;
            _camera = camera;
            _playerTransform = controller.transform;
        }

        private bool CanStartPushing()
        {
            if (_candidateBox == null || !_controller.MovementModule.IsGrounded()) return false;

            Vector3 toBox = _candidateBox.transform.position - _playerTransform.position;
            toBox.y = 0f;

            Vector3 playerForward = _controller.MeshForward;
            playerForward.y = 0f;

            float angle = Vector3.Angle(playerForward.normalized, toBox.normalized);
            return angle < 15f;
        }

        public void FixedTick()
        {
            // Si no está empujando, salir normalmente
            if (!_isPushing || !_currentBox)
            {
                StopPush();
                return;
            }

// Solo si está empujando, esperá que pase el cooldown antes de evaluar lógica de salida
            if (_pushCooldown > 0f)
            {
                _pushCooldown -= Time.fixedDeltaTime;
                return;
            }

            Vector2 inputDir = _input.Direction;
            Vector3 forward = Vector3.ProjectOnPlane(_controller.MeshPivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_controller.MeshPivot.right, Vector3.up).normalized;
            Vector3 desiredMove = (forward * inputDir.y + right * inputDir.x).normalized;

            _playerMoveDirection = desiredMove;

            // Si no hay input, no penalizamos ni rotamos
            if (desiredMove.sqrMagnitude < 0.01f)
            {
                ShouldRotateMesh = false;
                DesiredMeshForward = _controller.MeshPivot.forward;
                _controller.MovementModule.SetRotationSpeedMultiplier(1f);
                return;
            }

            Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
            float angle = Vector3.Angle(desiredMove, cameraForward); // sin signo

            // Penalización por desalineación
            const float penaltyStartAngle = 5f;
            const float penaltyMax = 0.1f;
            float t = Mathf.InverseLerp(penaltyStartAngle, 180f, angle);
            float penalty = Mathf.Lerp(1f, penaltyMax, t);
            // _controller.MovementModule.SetRotationSpeedMultiplier(penalty);

            // Si hay mucho ángulo y se está moviendo hacia adelante, forzar rotación del mesh
            bool wantsToRotate = angle > 15f && Mathf.Abs(inputDir.y) > 0.1f;
            ShouldRotateMesh = wantsToRotate;
            DesiredMeshForward = wantsToRotate ? cameraForward : _controller.MeshPivot.forward;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PushableBox box)) return;
            _candidateBox = box;
            _candidateCollider = other;
        }

        public void OnTriggerExit(Collider other)
        {
            if (other != _candidateCollider) return;
            _candidateBox = null;
            _candidateCollider = null;
        }

        public void SetGrabInput(bool isButtonPressed)
        {
            if (isButtonPressed && !_isPushing && CanStartPushing())
            {
                StartPush();
            }
            else if (!isButtonPressed && _isPushing)
            {
                StopPush();
            }
        }

        public void StopPush()
        {
            if (_currentBox != null)
                _currentBox.Detach();

            _candidateBox = _currentBox;
            _currentBox = null;
            _isPushing = false;
        }

        private async void StartPush()
        {
            const float minDistance = 1.25f;
            const float raycastHeightOffset = 0.75f;
            _pushCooldown = MinPushTime;
            // 1. Rotar MeshPivot hacia el centro de la caja
            Vector3 toBox = _candidateBox.transform.position - _controller.MeshPivot.position;
            toBox.y = 0f;
            if (toBox.sqrMagnitude < 0.001f)
                toBox = -_candidateBox.transform.forward;

            toBox.Normalize();
            _controller.MeshPivot.rotation = Quaternion.LookRotation(toBox, Vector3.up);

            // 2. Raycast desde el cuerpo hacia adelante
            Vector3 rayOrigin = _playerTransform.position + Vector3.up * raycastHeightOffset;
            Vector3 rayDirection = _controller.MeshPivot.forward;
            float detectedDistance = float.MaxValue;
            Vector3 pushNormal = -rayDirection; // fallback

            RaycastHit hit;
            int mask = ~(1 << 3); // ignorar layer 3 (player)

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, 5f, mask, QueryTriggerInteraction.Ignore))
            {
#if UNITY_EDITOR
                Debug.Log($"Ray hit: {hit.collider.name} (layer {hit.collider.gameObject.layer})");
#endif
                bool hitValid = hit.collider.transform.root == _candidateBox.transform.root;
                Debug.DrawRay(rayOrigin, rayDirection * hit.distance, hitValid ? Color.green : Color.yellow, 1f);

                if (hitValid)
                {
                    detectedDistance = hit.distance;
                    pushNormal = hit.normal;
                }
            }
            else
            {
                Debug.DrawRay(rayOrigin, rayDirection * 5f, Color.red, 1f);
            }

            // 3. Si está demasiado cerca, mover en la dirección opuesta a la superficie
            if (detectedDistance < minDistance)
            {
                float backAmount = minDistance - detectedDistance ;
                Vector3 moveBack = pushNormal * backAmount;

                Vector3 newPos = _playerTransform.position + moveBack;
                newPos.y = _playerTransform.position.y;

                _controller.MovementModule.TeleportTo(newPos);
            }
            // 4. Conectar empuje
            _isPushing = true;
            _currentBox = _candidateBox;
            _candidateBox = null;

            await Task.Yield();
            _currentBox.Attach(_controller.MeshPivot);
            _currentBox.DetachOnStress = StopPush;
        }
        
    }
}
