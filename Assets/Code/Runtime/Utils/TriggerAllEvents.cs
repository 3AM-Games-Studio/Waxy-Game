using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public class TriggerAllEvents : TriggerEvent
    {
        
        [Header("Disablers")]
        [SerializeField] private bool disableEnter;
        [SerializeField] private bool disableStay = true;
        [SerializeField] private bool disableExit;
        
        [Header("Events")]
        public UnityEvent<GameObject> onEnter;
        public UnityEvent<GameObject> onStay;
        public UnityEvent<GameObject> onExit;
        private void OnTriggerEnter(Collider other)
        {
            if (enableTriggers && !disableEnter && IsInMask(other.gameObject.layer))
                onEnter.Invoke(other.gameObject);
        }

        private void OnTriggerStay(Collider other)
        {
            if (enableTriggers && !disableStay && IsInMask(other.gameObject.layer))
                onStay.Invoke(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (enableTriggers && !disableExit && IsInMask(other.gameObject.layer))
                onExit.Invoke(other.gameObject);
        }
    }
}