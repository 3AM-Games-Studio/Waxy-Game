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

        public bool CanStartPushing() => _candidateBox && _controller.MovementModule.IsGrounded();
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
            const float penaltyMax = 0.1f; // ← el 10% de la velocidad normal

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
            if (!isButtonPressed && _isPushing)
            {
                StopPush();
            }
        }

        public void StopPush()
        {
            if(_currentBox)
                _currentBox.Detach();
            _candidateBox = _currentBox;
            _currentBox = null;
            _isPushing = false;
        }

        private void StartPush()
        {
            _isPushing = true;
            _currentBox = _candidateBox;
            _candidateBox = null;
            _currentBox.Attach(_controller.MeshPivot);
            _currentBox.DetachOnStress = StopPush;
        }
    }
}
