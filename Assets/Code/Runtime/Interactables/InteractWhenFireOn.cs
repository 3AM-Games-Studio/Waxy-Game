using System;
using Player.Candle;
using UnityEngine;
using UnityEngine.Events;

public class InteractWhenFireOn : MonoBehaviour, IInteract
{
    public UnityEvent OnInteract;
    private bool _isFireOn;
    [SerializeField]private bool _canInteract = true;
    public bool CanInteract { get => _canInteract; set => _canInteract = value; }

    private void OnEnable()
    {
        CandleController.OnFlameTurnOn += TurnOn;
    }

    private void OnDisable()
    {
        CandleController.OnFlameTurnOn -= TurnOn;
    }

    private void TurnOn(bool on)
    {
        _isFireOn = on;
    }

    public void Interact(GameObject sender)
    {
        if(_canInteract && _isFireOn) OnInteract.Invoke();
    }
}