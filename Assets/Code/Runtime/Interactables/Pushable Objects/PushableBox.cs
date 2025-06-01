using System.Collections.Generic;
using System;
using Core;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
    public class PushableBox : MonoBehaviour , IUpdateReceiver
    {
        [Header("Mass Settings")]
        public float defaultMass = 100f;
        public float pushMass = 5f;
        public float stressThreshold;
        public Transform _meshPivot; // The mesh pivot, child of player
        public Vector3 grabOffset;
        private Quaternion rotationOffset;
        private Rigidbody rb;
        private bool isAttached;
        public List<RigidbodyConstraints> constraints;
        public Action DetachOnStress;
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = defaultMass;
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        private void OnDestroy()
        {
            UpdateManager.UnregisterFromFixedUpdate(this);
        }

        public void Attach(Transform controller)
        {
            _meshPivot = controller;

            // Position in player’s local space
            Vector3 pivotPos = _meshPivot.position;
            pivotPos.y = transform.position.y; // Match height
            grabOffset = Quaternion.Inverse(_meshPivot.rotation) * (transform.position - _meshPivot.position);
            rotationOffset = Quaternion.Inverse(_meshPivot.rotation) * transform.rotation;

            rb.mass = pushMass;
            isAttached = true;
            foreach (RigidbodyConstraints constraint in constraints)
            {
                rb.constraints = rb.constraints | constraint;
            }
            
            UpdateManager.RegisterToFixedUpdate(this);
        }

        public void Detach()
        {
            _meshPivot = null;
            rb.mass = defaultMass;
            isAttached = false;
            rb.constraints = RigidbodyConstraints.None;
            UpdateManager.UnregisterFromFixedUpdate(this);
        }


        private void Move()
        {
       
            // 1. Calculate the target position from the player’s rotated offset
            Vector3 targetWorldPos = _meshPivot.position + _meshPivot.rotation * grabOffset;

            // 2. Compute the velocity needed to reach that position
            Vector3 desiredVelocity = (targetWorldPos - rb.position) / Time.fixedDeltaTime;

            // 3. Preserve gravity (don’t affect vertical movement)
            desiredVelocity.y = 0;

            // 4. Apply the horizontal movement as a velocity change
            Vector3 velocityChange = desiredVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            
            Vector3 directionToBox = (targetWorldPos - _meshPivot.position).normalized;

            if (directionToBox.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = _meshPivot.rotation * rotationOffset;
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.1f)); // suavizado opcional
                rb.MoveRotation(rb.rotation); // mantener rotación actual, sin override
                // rb.MoveRotation(smoothedRotation);
            }
            
            if (!(Vector3.Distance(rb.position, targetWorldPos) > stressThreshold)) return;
            DetachOnStress?.Invoke();
            DetachOnStress = delegate { };
        }

        public void OnUpdate() { }
        public void OnFixedUpdate()
        {
            Move();
        }
        public void OnLateUpdate() { }
    }

