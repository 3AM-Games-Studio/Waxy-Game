using UnityEngine;

namespace Player.Modules
{
    public class InteractionModule
    {
        private Transform _transform;

        public void Initialize(PlayerController controller)
        {
            _transform = controller.transform;
        }

        public void ExecuteInteraction(GameObject sender)
        {
            var colliders = Physics.OverlapSphere(_transform.position + _transform.forward + Vector3.up, 1f);
            
            foreach (var col in colliders)
            {
                if(col.TryGetComponent(out IInteract interact))
                    interact.Interact(sender);
            }
        }
    
    }
}