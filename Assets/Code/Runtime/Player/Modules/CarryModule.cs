using System;
using UnityEngine;

namespace Player.Modules
{
    public class CarryModule
    {
        public void SetGrabInput(bool isButtonPressed)
        {
            throw new NotImplementedException();
        }
    }
    
    public class PushModule
    {
        public void SetGrabInput(bool isButtonPressed)
        {
            throw new NotImplementedException();
        }
    }
    
    public class InteractionModule
    {
        public void SetInteractInput(bool isButtonPressed)
        {
            throw new NotImplementedException();
        }
    }

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