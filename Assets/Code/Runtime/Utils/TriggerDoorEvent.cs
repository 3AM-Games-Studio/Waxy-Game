using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public class TriggerDoorEvent : TriggerEvent
    {
        [Header("Events")]
        public UnityEvent<GameObject> onEnter;
        public UnityEvent<GameObject> onExit;
        private void Awake()
        {
            if(!GetComponents<Collider>().Any(col => col.isTrigger)) 
                Debug.LogError(gameObject.name + " MissingCollider");
            enabled= false;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (enableTriggers && IsInMask(other.gameObject.layer))
                onEnter.Invoke(other.gameObject);
        } 
        private void OnTriggerExit(Collider other)
        {
            if (enableTriggers && IsInMask(other.gameObject.layer))
                onExit.Invoke(other.gameObject);
        }
    }
}