using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public class TriggerExitEvent : TriggerEvent
    {
        [Header("Events")]
        public UnityEvent<GameObject> onExit;
        private void Awake()
        {
#if UNITY_EDITOR
            if(!GetComponents<Collider>().Any(col => col.isTrigger)) 
                Debug.LogError(gameObject.name + " MissingCollider");
            #endif
            enabled= false;
        }
        private void OnTriggerExit(Collider other)
        {
            if (enableTriggers && IsInMask(other.gameObject.layer))
                onExit.Invoke(other.gameObject);
        }
    }
}