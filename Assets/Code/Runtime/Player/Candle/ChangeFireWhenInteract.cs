using UnityEngine;

namespace Player.Candle
{
    [RequireComponent(typeof(Collider))]
    public class ChangeFireWhenInteract : MonoBehaviour , IInteract
    {
        [SerializeField] private bool turn;
        [SerializeField]private bool _canInteract = true;
        public bool CanInteract { get => _canInteract; set => _canInteract = value; }

        public void Interact(GameObject sender)
        {
            if (!_canInteract) return;
            if(turn)CandleController.Instance.TurnFireOn();
            else CandleController.Instance.TurnFireOff();
        }
    }
}