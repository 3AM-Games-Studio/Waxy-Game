using UnityEngine;
using UnityUtils;

namespace AdvancedController {
    public class TurnTowardController : MonoBehaviour {
        [SerializeField] private Player.PlayerController controller;
        [Header("Rotation Settings")]
        public float turnSpeed = 8f; // A smooth blend speed; higher = snappier

        [Header("Movement Penalty")]
        [Range(0f, 1f)] public float maxRotationPenalty = 0.5f;
        [Range(0f, 180f)] public float rotationToleranceAngle = 15f;
        public float rotationSpeedMultiplier = 1f;
        public float onAirRotationSpeedMultiplier = 0.75f;

        private Transform tr;

        void Start() {
            tr = transform;
        }

        void LateUpdate() {
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
    }
}
