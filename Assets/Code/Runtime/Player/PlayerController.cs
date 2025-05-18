using System;
using Core;
using UnityEngine;
using UnityUtils.StateMachine;
using Player.Modules;
using Player.States;

namespace Player
{
    public class PlayerController : StatefulEntity , IUpdateReceiver
    {
        [SerializeField,Header("Inputs")] InputReader input;
    
        #region Variables
        
        [Header("Movement Settings")]
        [Tooltip("Velocidad base del personaje en el suelo.")]
        public float movementSpeed = 7f;
        
        [Tooltip("Qué tan rápido se puede modificar la dirección en el aire.")]
        public float airControlRate = 2f;
        
        [Tooltip("Fuerza inicial aplicada hacia arriba al iniciar un salto.")]
        public float jumpSpeed = 10f;
        
        [Tooltip("Duración máxima del salto si se mantiene la tecla presionada.")]
        public float jumpDuration = 0.2f;
        
        [Tooltip("Fricción horizontal aplicada mientras está en el aire.")]
        public float airFriction = 0.5f;
        
        [Tooltip("Fricción horizontal aplicada mientras está en el suelo.")]
        public float groundFriction = 100f;
        
        [Tooltip("Fuerza de gravedad que tira hacia abajo al personaje.")]
        public float gravity = 30f;
        
        [Tooltip("Fuerza adicional aplicada al deslizar por pendientes empinadas.")]
        public float slideGravity = 5f;
        
        [Tooltip("Ángulo máximo que se considera suelo. Más allá de este valor, el personaje se desliza.")]
        public float slopeLimit = 30f;

        #endregion
        
        #region Modules

        public MovementModule movementModule;
        private CarryModule _carry;
        private PushModule _push;
        private InteractionModule _interaction;
        public PlayerEvents Events;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            SetUpModules();
            SetupStateMachine();
        }

        private void Start() => input.EnablePlayerActions();

        private void OnEnable()
        {
            SubscribeInputs();
            SubscribeUpdates();
        }

        private void OnDisable()
        {
            UnsubscribeInputs();
            UnsubscribeUpdates();
        }
        private void OnDestroy()
        {
            Events.Dispose();
            
            UnsubscribeInputs();
            UnsubscribeUpdates();
        }

        #endregion
        
        #region Update Methods
              
        private void SubscribeUpdates()
        {
            UpdateManager.RegisterToUpdate(this);
            UpdateManager.RegisterToFixedUpdate(this);
        }
        
        private void UnsubscribeUpdates()
        {
            UpdateManager.UnregisterFromUpdate(this);
            UpdateManager.UnregisterFromFixedUpdate(this);
        }
        public void OnUpdate()
        {
            stateMachine.Update();
        }

        public void OnFixedUpdate()
        {
            stateMachine.FixedUpdate();
        }
        public void OnLateUpdate() { }
        
        #endregion

        #region State Machine Setup

        private void SetupStateMachine() {
            stateMachine = new StateMachine();

            #region States
            var idle = new IdleState(this);
            var move = new MoveState(this);
            var running = new RunningState(this);
            var jumping = new JumpingState(this);
            var falling = new FallingState(this);
            var sliding = new SlidingState(this);
            var rising = new RisingState(this);
            var grounded = new GroundedState(this);
            var carrying = new CarryingState(this);
            var pushing = new PushingState(this);
            var interacting = new InteractingState(this);
            var dead = new DeadState(this);
            #endregion

            #region Transitions

// Idle
          
// Move
          
// Running

// Jumping

// Falling

// Sliding

// Rising

// Grounded

// Carrying

// Pushing

// Interacting

// Dead
            #endregion
            
            //TODO delete after changing transitions
            #region OldTransitions
/*
            At(grounded, rising, () => IsRising());
            At(grounded, sliding, () => mover.IsGrounded() && IsGroundTooSteep());
            At(grounded, falling, () => !mover.IsGrounded());
            At(grounded, jumping, () => (jumpKeyIsPressed || jumpKeyWasPressed) && !jumpInputIsLocked);
            
            At(falling, rising, () => IsRising());
            At(falling, grounded, () => mover.IsGrounded() && !IsGroundTooSteep());
            At(falling, sliding, () => mover.IsGrounded() && IsGroundTooSteep());
            
            At(sliding, rising, () => IsRising());
            At(sliding, falling, () => !mover.IsGrounded());
            At(sliding, grounded, () => mover.IsGrounded() && !IsGroundTooSteep());
            
            At(rising, grounded, () => mover.IsGrounded() && !IsGroundTooSteep());
            At(rising, sliding, () => mover.IsGrounded() && IsGroundTooSteep());
            At(rising, falling, () => IsFalling());
            At(rising, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());
            
            At(jumping, rising, () => jumpTimer.IsFinished || jumpKeyWasLetGo);
            At(jumping, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());
*/
            #endregion
            
            stateMachine.SetState(falling);
        }
        public IState CurrentState() => stateMachine.CurrentState;
        #endregion

        #region Input Methods
        private void SubscribeInputs()
        {
            input.Jump     += HandleJumpKeyInput;
            input.Run      += HandleRunInput;
            input.Interact += HandleInteractInput;
            input.Grab     += HandleGrabInput;

        }

        private void UnsubscribeInputs()
        {
            input.Jump     -= HandleJumpKeyInput;
            input.Run      -= HandleRunInput;
            input.Interact -= HandleInteractInput;
            input.Grab     -= HandleGrabInput;
        }

        void HandleJumpKeyInput(bool isButtonPressed)
        {
            // movementModule.SetJumpInput(isButtonPressed);
        }
        
        private void HandleGrabInput(bool isButtonPressed)
        {
            // _carry.SetGrabInput(isButtonPressed);
            // _push.SetGrabInput(isButtonPressed);
        }

        private void HandleInteractInput(bool isButtonPressed)
        {
            // _interaction.SetInteractInput(isButtonPressed);
        }

        private void HandleRunInput(bool isButtonPressed)
        {
            // movementModule.SetRunInput(isButtonPressed);
        }
        #endregion

        #region Modules Methods
        private void SetUpModules()
        {
            movementModule = GetComponent<MovementModule>();
            _carry = new CarryModule();
            _push = new PushModule();
            _interaction = new InteractionModule();
            Events = new PlayerEvents();
        }
        #endregion

        public void OnJumpStart()
        {
            
        }
    }
    
}
