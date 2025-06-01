using AdvancedController;
using UnityEngine;
using UnityUtils.StateMachine;

namespace AdvancedController {
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
    public class GroundedState : BasePlayerState {
        public GroundedState(PlayerController controller) : base(controller) { }

        public override void OnEnter() {
            controller.OnGroundContactRegained();
            animationController.HandleGround(true);
        }
    }

    public class FallingState : BasePlayerState {

        public FallingState(PlayerController controller) : base(controller) { }

        public override void OnEnter() {
            controller.OnFallStart();
            // animationController.HandleGround(false);
        }
    }

    public class SlidingState : BasePlayerState {

        public SlidingState(PlayerController controller) : base(controller) { }

        public override void OnEnter() {
            controller.OnGroundContactLost();
            animationController.HandleGround(false);
        }
    }

    public class RisingState : BasePlayerState {

        public RisingState(PlayerController controller) : base(controller) { }
        
        public override void OnEnter() {
            controller.OnGroundContactLost();
            animationController.HandleGround(false);
        }
    }

    public class JumpingState : BasePlayerState {

        public JumpingState(PlayerController controller): base(controller) { }

        public override void OnEnter() {
            controller.OnGroundContactLost();
            controller.OnJumpStart();
            animationController.HandleJump(true);
            animationController.HandleGround(false);
        }

        public override void OnExit()
        {
            animationController.HandleJump(false);
        }
    }
}