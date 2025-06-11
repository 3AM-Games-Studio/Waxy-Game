using UnityEngine;

public interface IInteract
{
    bool CanInteract {get;set;}
    void Interact(GameObject sender);
}