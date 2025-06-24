using System.Linq;
using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(Collider))]
    public abstract class TriggerEvent : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected LayerMask mask = ~0;

        [SerializeField] protected bool enableTriggers = true;
        
        public void EnableTriggers(bool enable) => enableTriggers = enable;
        private void Awake()
        {
#if UNITY_EDITOR
            if(!GetComponents<Collider>().Any(col => col.isTrigger)) 
                Debug.LogError(gameObject.name + " MissingCollider");
            #endif
            enabled= false;
        }
        protected bool IsInMask(int layer) => (mask.value & (1 << layer)) != 0;
    }
}
