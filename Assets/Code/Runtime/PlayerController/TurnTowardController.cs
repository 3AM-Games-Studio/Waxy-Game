using System;
using Core;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityUtils;

namespace AdvancedController {
    public class TurnTowardController : MonoBehaviour , IUpdateReceiver
    {
        [SerializeField] private Player.PlayerController controller;
        [Header("Rotation Settings")]
        public float turnSpeed = 8f; // A smooth blend speed; higher = snappier

        [Header("Movement Penalty")]
        [Range(0f, 1f)] public float maxRotationPenalty = 0.5f;
        [Range(0f, 180f)] public float rotationToleranceAngle = 15f;
        public float rotationSpeedMultiplier = 1f;
        public float onAirRotationSpeedMultiplier = 0.75f;

        private Transform tr;

        private void Awake()
        {
            UpdateManager.RegisterToLateUpdate(this);
        }

        private void OnEnable()
        {
            UpdateManager.RegisterToLateUpdate(this);
        }

        private void OnDisable()
        {
            UpdateManager.UnregisterFromLateUpdate(this);
        }

        private void OnDestroy()
        {
            UpdateManager.UnregisterFromLateUpdate(this);
        }

        void Start() {
            tr = transform;
            _currentRotation = NormalRotation;
            UpdateManager.RegisterToLateUpdate(this);
        }
        
        public void SetPushingMode(bool enable)
        {
            if (enable) _currentRotation = PushRotation;
            else _currentRotation = NormalRotation;
        }

        private Action _currentRotation;
        public void OnUpdate(){ }
        public void OnFixedUpdate() { }
        
        public void OnLateUpdate()
        {
            _currentRotation.Invoke();
        }

        private void NormalRotation()
        {
            Vector3 inputVelocity = controller.MovementModule.GetMovementVelocity();
            Vector3 movementDir = Vector3.ProjectOnPlane(inputVelocity, tr.parent.up);

            if (movementDir.sqrMagnitude < 0.0001f) {
                controller.MovementModule.SetRotationSpeedMultiplier(1f);
                return;
            }

            // Calculate rotation toward movement direction
            Quaternion currentRot = tr.rotation;
            Quaternion targetRot = Quaternion.LookRotation(movementDir, tr.parent.up);

            float lerpSpeed = turnSpeed * rotationSpeedMultiplier;
            if (!controller.MovementModule.IsGrounded())
                lerpSpeed *= onAirRotationSpeedMultiplier;

            tr.rotation = Quaternion.Lerp(currentRot, targetRot, lerpSpeed * Time.deltaTime);

            // Calculate rotation penalty
            float angleDifference = VectorMath.GetAngle(tr.forward, movementDir.normalized, tr.parent.up);
            float angleAbs = Mathf.Abs(angleDifference);

            float finalMultiplier = 1f;
            if (angleAbs > rotationToleranceAngle) {
                float t = Mathf.InverseLerp(rotationToleranceAngle, 180f, angleAbs);
                float penalty = Mathf.Lerp(1f, 1f - maxRotationPenalty, t);
                finalMultiplier = penalty;
            }

            controller.MovementModule.SetRotationSpeedMultiplier(finalMultiplier);
        }

        private void PushRotation()
        {
            if (!controller.PushModule.ShouldRotateMesh)
            {
                controller.MovementModule.SetRotationSpeedMultiplier(1f);
                return;
            }

            Vector3 lookDir = controller.PushModule.DesiredMeshForward;
            if (lookDir.sqrMagnitude < 0.001f)
                return;

            Quaternion currentRot = tr.rotation;
            Quaternion targetRot = Quaternion.LookRotation(lookDir, tr.parent.up);

            float lerpSpeed = turnSpeed * rotationSpeedMultiplier;
            if (!controller.MovementModule.IsGrounded())
                lerpSpeed *= onAirRotationSpeedMultiplier;

            tr.rotation = Quaternion.Lerp(currentRot, targetRot, lerpSpeed * Time.deltaTime);
        }
    }
}
