using UnityEngine;

namespace AdvancedController {
    public class CeilingDetector : MonoBehaviour {
        public float ceilingAngleLimit = 10f;
        public bool isInDebugMode;
        float debugDrawDuration = 2.0f;
        bool ceilingWasHit;
        
        void OnCollisionEnter(Collision collision) => CheckForContact(collision);
        void OnCollisionStay(Collision collision) => CheckForContact(collision);

        void CheckForContact(Collision collision)
        {
            ContactPoint[] contacts = new ContactPoint[10];
            var contactsAmount = collision.GetContacts(contacts);
            if (contactsAmount == 0) return;
            
            float angle = Vector3.Angle(-transform.up, contacts[0].normal);

            if (angle < ceilingAngleLimit) {
                ceilingWasHit = true;
            }

            if (isInDebugMode) {
                Debug.DrawRay(contacts[0].point, contacts[0].normal, Color.red, debugDrawDuration);
            }
        }
        
        public bool HitCeiling() => ceilingWasHit;
        public void Reset() => ceilingWasHit = false;
    }
}
