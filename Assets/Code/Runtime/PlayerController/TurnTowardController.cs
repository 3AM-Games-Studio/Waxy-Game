using UnityEngine;
using UnityUtils;

namespace AdvancedController {
    public class TurnTowardController : MonoBehaviour {
        [SerializeField] PlayerController controller;
        public float turnSpeed = 50f;

        [Header("Movement Penalty")]
        [Range(0f, 1f)] public float maxRotationPenalty = 0.5f;
        [Range(0f, 180f)] public float rotationToleranceAngle = 15f;
        public float rotationSpeedMultiplier = 1f;
        public float onAirRotationSpeedMultiplier = 0.75f;
        Transform tr;
        float currentYRotation;
        const float fallOffAngle = 90f;

        void Start() {
            tr = transform;
            currentYRotation = tr.localEulerAngles.y;
        }

        void LateUpdate() {
            Vector3 inputDir = controller.GetMovementVelocity();
            Vector3 movementDir = Vector3.ProjectOnPlane(inputDir, tr.parent.up);

            if (movementDir.sqrMagnitude < 0.0001f ) {
                controller.SetRotationSpeedMultiplier(1);
                return;
            }

            // Calcular diferencia de rotación
            float angleDifference = VectorMath.GetAngle(tr.forward, movementDir.normalized, tr.parent.up);

            // Aplicar rotación suave
            float targetYRotation = Quaternion.LookRotation(movementDir, tr.parent.up).eulerAngles.y;
            float finalTurnSpeed = turnSpeed * rotationSpeedMultiplier;
            if (!controller.IsGrounded())
                finalTurnSpeed *= onAirRotationSpeedMultiplier;

            float step = finalTurnSpeed * Time.deltaTime;
            currentYRotation = Mathf.MoveTowardsAngle(currentYRotation, targetYRotation, step);
            tr.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

           
                // Penalización por ángulo
                float angleAbs = Mathf.Abs(angleDifference);
                float finalMultiplier;

                if (angleAbs <= rotationToleranceAngle) {
                    finalMultiplier = 1f;
                } else {
                    float t = Mathf.InverseLerp(rotationToleranceAngle, 180f, angleAbs);
                    float penalty = Mathf.Lerp(1f, 1f - maxRotationPenalty, t);
                    finalMultiplier = penalty ;
                }
                
                controller.SetRotationSpeedMultiplier(finalMultiplier);
            
        }
    }
}
