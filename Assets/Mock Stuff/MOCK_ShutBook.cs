using System;
using Player.Candle;
using UnityEngine;
using UnityEngine.Events;

public class MOCK_ShutBook : MonoBehaviour
{
   public bool canInteract { get; set; } = true;
   public UnityEvent OnTouchFlame;
   public UnityEvent OnTouchPlayer;
   private bool isFireOn = false;

   private void Start()
   {
      CandleController.OnFlameTurnOn += OnCandleControllerOnOnFlameTurnOn;
   }

   private void OnDisable()
   {
      
      CandleController.OnFlameTurnOn -= OnCandleControllerOnOnFlameTurnOn;
   }

   private void OnDestroy()
   {
      
      CandleController.OnFlameTurnOn -= OnCandleControllerOnOnFlameTurnOn;
   }

   private void OnCandleControllerOnOnFlameTurnOn(bool on)
   {
      isFireOn = on;
   }

   private void OnTriggerEnter(Collider other)
   {
      if(!canInteract) return;
      if(other.gameObject.layer == 7) OnTouchFlame.Invoke();
      else if(isFireOn && other.gameObject.layer == 3){ OnTouchPlayer.Invoke();}
   }
}
