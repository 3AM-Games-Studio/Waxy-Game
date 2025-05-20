using AdvancedController;
using UnityEngine;
using UnityUtils.StateMachine;

namespace Player.States
{
    public abstract class BasePlayerState : IState
    {
        protected readonly PlayerController controller;
        protected AnimationController animationController;
        public BasePlayerState(PlayerController controller)
        {
            this.controller = controller;
            animationController = controller.GetComponent<AnimationController>();
        }

        public virtual void Update() { }
        public virtual void FixedUpdate() { }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
    }
    //Player States Idle, Move, Running, Jumping, Falling, Sliding, Grounded, Carrying, Pushing, Interacting, Dead
    public class IdleState : BasePlayerState
    {
        public IdleState(PlayerController controller) : base(controller) { }

        public override void FixedUpdate()
        {
            controller.MovementModule.ApplyIdleFriction();
        }
    }

    
    public class MoveState : BasePlayerState
    {
        public MoveState(PlayerController controller) : base(controller)
        {
        }

        public override void FixedUpdate()
        {
            controller.MovementModule.ApplyGroundedMovement();
        }
    }
    public class RunningState : BasePlayerState
    {
        public RunningState(PlayerController controller) : base(controller)
        {
        }

        public override void FixedUpdate()
        {
            controller.MovementModule.ApplyGroundedMovement();
        }
    }
    public class JumpingState : BasePlayerState
    {
        public JumpingState(PlayerController controller) : base(controller)
        {
        }
        
        public override void OnEnter() {
            controller.MovementModule.OnJumpStart();
        }
        
        public override void FixedUpdate() => controller.MovementModule.ApplyAirMovement();
        public override void OnExit()
        {
            // animationController.HandleJump(false);
        }
    }
    public class FallingState : BasePlayerState
    {
        public FallingState(PlayerController controller) : base(controller)
        {
        }
        public override void OnEnter() => controller.MovementModule.OnFallStart();
        
        public override void FixedUpdate() => controller.MovementModule.ApplyAirMovement();
    }
    public class RisingState : BasePlayerState {

        public RisingState(PlayerController controller) : base(controller) { }

        public override void OnEnter()
        {
        }

        public override void FixedUpdate() => controller.MovementModule.ApplyAirMovement();
    }

    public class SlidingState : BasePlayerState
    {
        public SlidingState(PlayerController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Sliding State");
        }
        public override void FixedUpdate()
        {
            controller.MovementModule.ApplySlideMovement();
        }
    }
    public class GroundedState : BasePlayerState
    {
        public GroundedState(PlayerController controller) : base(controller)
        {
        }
        public override void OnEnter()
        {
            base.OnEnter(); 
            // controller.MovementModule.OnGroundContactRegained();
        }
    }
    public class CarryingState : BasePlayerState
    {
        public CarryingState(PlayerController controller) : base(controller)
        {
        }
    }
    public class PushingState : BasePlayerState
    {
        public PushingState(PlayerController controller) : base(controller)
        {
        }
    }
    
    public class InteractingState : BasePlayerState
    {
        public InteractingState(PlayerController controller) : base(controller)
        {
        }
    }
    public class DeadState : BasePlayerState
    {
        public DeadState(PlayerController controller) : base(controller)
        {
        }
    }
}