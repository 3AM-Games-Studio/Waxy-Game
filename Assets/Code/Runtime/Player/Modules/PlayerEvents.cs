using System;
using UnityEngine;

namespace Player.Modules
{
    public class PlayerEvents
    {
        public  Action<Vector3> OnJump = delegate { };
        public  Action<Vector3> OnLand = delegate { };
        public  Action<bool> OnGround = delegate { };
        
        public void Dispose()
        {
            OnJump = delegate { };
            OnLand = delegate { };
            OnGround = delegate { }; 
        }
    }
}