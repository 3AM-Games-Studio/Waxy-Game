using System;
using UnityEngine;
using UnityEngine.Events;

public class MOCK_Flame : MonoBehaviour
{
   public UnityEvent OnTriggerFlame;
   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.layer != 7) return;
      if (other.TryGetComponent(out IFlameable flamable))
      {
         flamable.Interact(1,FlameType.Normal);
         OnTriggerFlame.Invoke();
      }
   }
}
