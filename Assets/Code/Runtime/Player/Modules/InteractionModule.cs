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
            var position = _transform.position + _transform.forward + Vector3.up;
            Collider[] hitColliders = new Collider[5];
            int numColliders = Physics.OverlapSphereNonAlloc(position, 1f, hitColliders);
            
            for (int i = 0; i < numColliders; i++)
            {
                if(hitColliders[i].TryGetComponent(out IInteract interact))
                    interact.Interact(sender);
            }
        }
    
    }
}