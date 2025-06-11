using System;
using UnityEngine;

public class MOCK_Checkpoint : MonoBehaviour
{
   [SerializeField] private MOCK_YKill _kill;
   private void OnTriggerEnter(Collider other)
   {
      if(other.gameObject.layer == 3) _kill.LastCheckpoint(transform.position);
   }
}
