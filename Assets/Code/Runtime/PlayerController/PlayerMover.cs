using UnityEngine;

namespace AdvancedController {
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMover : MonoBehaviour {
        #region Fields
        [Header("Collider Settings:")]
        [Range(0f, 1f)] [SerializeField] float stepHeightRatio = 0.1f;
        [SerializeField] float colliderHeight = 2f;
        [SerializeField] float colliderThickness = 1f;
        [SerializeField] Vector3 colliderOffset = Vector3.zero;
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundDetectionMask = ~0;
        Rigidbody rb;
        Transform tr;
        CapsuleCollider col;
        // RaycastSensor sensor;
        
        bool isGrounded;
        float baseSensorRange;
        Vector3 currentGroundAdjustmentVelocity; // Velocity to adjust player position to maintain ground contact
        // int currentLayer;
        
        [Header("Sensor Settings:")]
        [SerializeField] bool isInDebugMode;
        bool isUsingExtendedSensorRange = true; // Use extended range for smoother ground transitions
        
        [Header("Multi Raycast Settings")]
        [SerializeField] private float sensorOffsetDistance = 0.3f;
        private RaycastSensor[] sensors = new RaycastSensor[5];
        private readonly Vector3[] _sensorOffsets = {
            Vector3.zero,
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
        #endregion

        void Awake() {
            Setup();
            RecalculateColliderDimensions();
        }

        void OnValidate() {
            if (gameObject.activeInHierarchy) {
                RecalculateColliderDimensions();
            }
        }
        
#if UNITY_EDITOR
        void LateUpdate() {
            if (isInDebugMode) {
                // sensor.DrawDebug();
                foreach (var sensor in sensors)
                {
                    sensor.DrawDebug();
                }
            }
            
         
        }
#endif
        private Vector3 groundNormal = Vector3.up;
        public void CheckForGround() {
            // if (currentLayer != gameObject.layer)
            //     RecalculateSensorLayerMask();

            currentGroundAdjustmentVelocity = Vector3.zero;
            isGrounded = false;

            float closestDistance = float.MaxValue;
            Vector3 bestNormal = Vector3.up;

            foreach (var s in sensors) {
                s.Cast();
                if (s.HasDetectedHit()) {
                    float dist = s.GetDistance();
                    if (dist < closestDistance) {
                        closestDistance = dist;
                        bestNormal = s.GetNormal();
                        isGrounded = true;
                    }
                }
            }

            if (!isGrounded) return;
            groundNormal = bestNormal;
            float upperLimit = colliderHeight * tr.localScale.x * (1f - stepHeightRatio) * 0.5f;
            float middle = upperLimit + colliderHeight * tr.localScale.x * stepHeightRatio;
            float distanceToGo = middle - closestDistance;
            currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
        }
        
        public bool IsGrounded() => isGrounded;
        public Vector3 GetGroundNormal() => groundNormal;
        
        // NOTE: Older versions of Unity use rb.velocity instead
        public void SetVelocity(Vector3 velocity) => rb.linearVelocity = velocity + currentGroundAdjustmentVelocity;
        public void SetExtendSensorRange(bool isExtended) => isUsingExtendedSensorRange = isExtended;

        void Setup() {
            tr = transform;
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();
            
            rb.freezeRotation = true;
            rb.useGravity = false;
        }

        void RecalculateColliderDimensions() {
            if (col == null) {
                Setup();
            }
            
            col.height = colliderHeight * (1f - stepHeightRatio);
            col.radius = colliderThickness / 2f;
            col.center = colliderOffset * colliderHeight + new Vector3(0f, stepHeightRatio * col.height / 2f, 0f);

            if (col.height / 2f < col.radius) {
                col.radius = col.height / 2f;
            }
            
            RecalibrateSensor();
        }
        void RecalibrateSensor() {
            // RecalculateSensorLayerMask();

            baseSensorRange = (colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio) * 1.01f * tr.localScale.x;

            for (int i = 0; i < sensors.Length; i++) {
                if (sensors[i] == null)
                    sensors[i] = new RaycastSensor(tr);

                Vector3 worldOffset = tr.TransformDirection(_sensorOffsets[i]) * sensorOffsetDistance;
                sensors[i].SetCastOrigin(col.bounds.center + worldOffset);
                sensors[i].SetCastDirection(RaycastSensor.CastDirection.Down);
                sensors[i].castLength = baseSensorRange;
                sensors[i].layermask = groundDetectionMask; 
               
            }
        }
   
        public Vector3 GetVelocity() => rb.linearVelocity;
    }
}