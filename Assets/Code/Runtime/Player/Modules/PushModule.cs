using System;
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

            Vector3 playerForward = _controller.MeshFoward;
            playerForward.y = 0f;

            float angle = Vector3.Angle(playerForward.normalized, toBox.normalized);
            return angle < 15f;
        }

        public void FixedTick()
        {

            if (!_isPushing || !_currentBox)
            {
                StopPush();
                return;
            }

            Vector3 playerForward = Vector3.ProjectOnPlane(_playerTransform.forward, Vector3.up).normalized;
            Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;

            float angle = Vector3.SignedAngle(playerForward, cameraForward, Vector3.up);
            float absAngle = Mathf.Abs(angle);

            Vector2 inputDir = _input.Direction;

            ShouldRotateMesh = false;
            DesiredMeshForward = playerForward;

            bool wantsToRotate = absAngle > 15f && Mathf.Abs(inputDir.y) > 0.1f;
            ShouldRotateMesh = wantsToRotate;
            DesiredMeshForward = wantsToRotate ? cameraForward : playerForward;

            const float penaltyStartAngle = 5f;
            const float penaltyMax = 0.1f;
            float t = Mathf.InverseLerp(penaltyStartAngle, 180f, absAngle);
            float penalty = Mathf.Lerp(1f, penaltyMax, t);

            _controller.MovementModule.SetRotationSpeedMultiplier(penalty);

            Vector3 forward = Vector3.ProjectOnPlane(_controller.MeshPivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_controller.MeshPivot.right, Vector3.up).normalized;
            _playerMoveDirection = (forward * _input.Direction.y + right * _input.Direction.x).normalized;
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

        private void StartPush()
        {
            const float minDistance = 0.75f;
            const float raycastHeightOffset = 0.75f;

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
                Debug.Log($"Ray hit: {hit.collider.name} (layer {hit.collider.gameObject.layer})");

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
                float backAmount = minDistance - detectedDistance;
                Vector3 moveBack = pushNormal * backAmount;

                Vector3 newPos = _playerTransform.position + moveBack;
                newPos.y = _playerTransform.position.y;

                _controller.MovementModule.TeleportTo(newPos);
            }

            // 4. Conectar empuje
            _isPushing = true;
            _currentBox = _candidateBox;
            _candidateBox = null;

            _currentBox.Attach(_controller.MeshPivot);
            _currentBox.DetachOnStress = StopPush;
        }
        
    }
}
