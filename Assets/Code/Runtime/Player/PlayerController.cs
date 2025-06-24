using System;
using AdvancedController;
using Core;
using Player.Candle;
using UnityEngine;
using UnityUtils.StateMachine;
using Player.Modules;
using Player.States;
using FallingState = Player.States.FallingState;
using GroundedState = Player.States.GroundedState;
using JumpingState = Player.States.JumpingState;
using RisingState = Player.States.RisingState;
using SlidingState = Player.States.SlidingState;

namespace Player
{
    public class PlayerController : StatefulEntity 
    {
        public string State;
        [SerializeField,Header("Inputs")] InputReader input;
        [SerializeField] private new Transform camera;
        #region Variables
        
        [Header("Movement Settings")]
        [Tooltip("Velocidad base del personaje en el suelo.")]
        public float walkSpeed = 7f;
        [Tooltip("Velocidad base del personaje en el suelo.")]
        public float runSpeed = 15f;
        
        [Tooltip("Qué tan rápido se puede modificar la dirección en el aire.")]
        public float airControlRate = 3f;
        
        [Tooltip("Fuerza inicial aplicada hacia arriba al iniciar un salto.")]
        public float jumpSpeed = 16.5f;
        
        [Tooltip("Duración máxima del salto si se mantiene la tecla presionada.")]
        public float jumpDuration = 0.75f;
        
        [Tooltip("Proporción mínima del salto que se debe ejecutar antes de permitir caída.")]
        [Range(0f, 1f)]
        public float minJumpRatio = 0.75f;
        [Tooltip("Fricción horizontal aplicada mientras está en el aire.")]
        public float airFriction = 2f;
        
        [Tooltip("Fricción horizontal aplicada mientras está en el suelo.")]
        public float groundFriction = 100f;
        
        [SerializeField] [Range(0f, 1f)]public float landingFriction = 0.9f;
        [Tooltip("Fuerza de gravedad que tira hacia abajo al personaje.")]
        public float gravity = 50f;
        
        [Tooltip("Fuerza adicional aplicada al deslizar por pendientes empinadas.")]
        public float slideGravity = 5f;
       
        [Tooltip("Ángulo máximo que se considera suelo. Más allá de este valor, el personaje se desliza.")]
        public float slopeLimit = 30f;
        
        public float CurrentSpeed { get; private set; }
        public Transform MeshPivot;
        public Vector3 MeshForward => MeshPivot.forward;
        #endregion
        
        #region Modules

        public MovementModule MovementModule;
        private CarryModule _carry;
        public PushModule PushModule;
        public InteractionModule InteractionModule;
        public PlayerEvents Events;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            CurrentSpeed = walkSpeed;
            if (!MeshPivot)
            {
                var turn = GetComponentInChildren<TurnTowardController>().transform;
                if(turn)MeshPivot = turn.transform;
                else Debug.LogError("No Turn Toward controller attached assign MeshPivot to player controller");
            }
            SetUpModules();
            SetupStateMachine();
            
        }

        private void Start()
        {
            input.EnablePlayerActions();
            SubscribeInputs();
            CandleController.OnFlameTurnOn += ChangeCandle;
        }
        private void OnEnable()
        {
            SubscribeInputs();
            CandleController.OnFlameTurnOn += ChangeCandle;
        }

        private void OnDisable()
        {
            UnsubscribeInputs();
            CandleController.OnFlameTurnOn -= ChangeCandle;
        }
        private void OnDestroy()
        {
            Events.Dispose();
            CandleController.OnFlameTurnOn -= ChangeCandle;
            UnsubscribeInputs();
        }
        private void OnTriggerEnter(Collider other)
        {
            PushModule.OnTriggerEnter(other);
            // _carry.OnTriggerEnter(other); // si hace falta
            // _interaction.OnTriggerEnter(other); // si hace falta
        }

        private void OnTriggerExit(Collider other)
        {
            PushModule.OnTriggerExit(other);
        }
        #endregion
        
        #region Update Methods
              
       
        public void Update()
        {
            stateMachine.Update();
        }
        
        public void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }
        
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

            // ─────────────────────────────
            // Sliding (más específico primero)
            At<Func<bool>>(sliding, falling, () => !MovementModule.IsGrounded());
            At<Func<bool>>(sliding, jumping, () => MovementModule.WantsToJump());
            At<Func<bool>>(sliding, move, () => MovementModule.IsGrounded() && !MovementModule.IsGroundTooSteep() && input.HasMovementInput());
            At<Func<bool>>(sliding, idle, () => MovementModule.IsGrounded() && !MovementModule.IsGroundTooSteep() && !input.HasMovementInput());

            // ─────────────────────────────
            // Falling
            At<Func<bool>>(falling, sliding, () => MovementModule.IsGrounded() && MovementModule.IsGroundTooSteep());
            At<Func<bool>>(falling, grounded, () => MovementModule.IsGrounded());

            // ─────────────────────────────
            // Rising
            At<Func<bool>>(rising, grounded, () => MovementModule.IsGrounded());
            At<Func<bool>>(rising, sliding, () => MovementModule.IsGrounded() && MovementModule.IsGroundTooSteep());
            At<Func<bool>>(rising, falling, () => MovementModule.ShouldStartFalling() || MovementModule.HitCeiling()); 
            
            // ─────────────────────────────
            // Jumping
            At<Func<bool>>(jumping, rising, () => MovementModule.IsRising());
            At<Func<bool>>(jumping, falling, () => MovementModule.HitCeiling());

            // ─────────────────────────────
            // Grounded
            At<Func<bool>>(grounded, sliding, () => MovementModule.IsGroundTooSteep());
            At<Func<bool>>(grounded, pushing, () => PushModule.IsPushing);
            At<Func<bool>>(grounded, jumping, () => MovementModule.WantsToJump());
            At<Func<bool>>(grounded, idle, () => grounded.IsReadyToExit && !input.HasMovementInput());
            At<Func<bool>>(grounded, move, () => grounded.IsReadyToExit && input.HasMovementInput());

            // ─────────────────────────────
            // Idle
            At<Func<bool>>(idle, sliding, () => MovementModule.IsGrounded() && MovementModule.IsGroundTooSteep());
            At<Func<bool>>(idle, jumping, () => MovementModule.WantsToJump());
            At<Func<bool>>(idle, falling, () => !MovementModule.IsGrounded());
            At<Func<bool>>(idle, pushing, () => PushModule.IsPushing);
            At<Func<bool>>(idle, interacting, WantsToInteract);
            At<Func<bool>>(idle, move, () => input.HasMovementInput());

            // ─────────────────────────────
            // Move
            At<Func<bool>>(move, sliding, () => MovementModule.IsGrounded() && MovementModule.IsGroundTooSteep());
            At<Func<bool>>(move, jumping, () => MovementModule.WantsToJump());
            At<Func<bool>>(move, falling, () => !MovementModule.IsGrounded());
            At<Func<bool>>(move, pushing, () => PushModule.IsPushing);
            At<Func<bool>>(move, interacting, WantsToInteract);
            At<Func<bool>>(move, running, () => isRunKeyPressed && _candleOn);
            At<Func<bool>>(move, idle, () => !input.HasMovementInput());

            // ─────────────────────────────
            // Running
            At<Func<bool>>(running, sliding, () => MovementModule.IsGrounded() && MovementModule.IsGroundTooSteep());
            At<Func<bool>>(running, jumping, () => MovementModule.WantsToJump());
            At<Func<bool>>(running, falling, () => !MovementModule.IsGrounded());
            At<Func<bool>>(running, move, () => input.HasMovementInput() && (!isRunKeyPressed || !_candleOn));
            At<Func<bool>>(running, idle, () => !input.HasMovementInput() && (!isRunKeyPressed || !_candleOn));

            // ─────────────────────────────
            // Pushing
            At<Func<bool>>(pushing, grounded, () => !PushModule.IsPushing);
            At<Func<bool>>(pushing, falling, () => !PushModule.IsPushing);
            At<Func<bool>>(pushing, idle, () => !PushModule.IsPushing);
            At<Func<bool>>(pushing, move, () => !PushModule.IsPushing);

            // ─────────────────────────────
            // Interacting (al finalizar el timer)
            At<Func<bool>>(interacting, idle, () => interacting.IsFinished() && !input.HasMovementInput());
            At<Func<bool>>(interacting, move, () => interacting.IsFinished() && input.HasMovementInput());

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
            MovementModule.SetJumpInput(isButtonPressed);
        }
        
        private void HandleGrabInput(bool isButtonPressed)
        {
            // _carry.SetGrabInput(isButtonPressed);
            PushModule.SetGrabInput(isButtonPressed);
        }

        private bool wantsToInteract;
        private void HandleInteractInput(bool isButtonPressed)
        {
            if (isButtonPressed)
                wantsToInteract = true;
        }
        public bool WantsToInteract()
        {
            bool temp = wantsToInteract;
            wantsToInteract = false;
            return temp;
        }
        private bool _candleOn = true; // Esto luego se conecta con CandleController
        private bool isRunKeyPressed;
        public float interactionDuration;

        private void ChangeCandle(bool candleOn)
        {
            _candleOn = candleOn;
        }
        private void HandleRunInput(bool isButtonPressed)
        {
            isRunKeyPressed = _candleOn && isButtonPressed;
            CurrentSpeed = isRunKeyPressed ? runSpeed : walkSpeed;
        }
        #endregion

        #region Modules Methods
        private void SetUpModules()
        {
            MovementModule = new MovementModule();
            _carry = new CarryModule();
            PushModule = new PushModule();
            InteractionModule = new InteractionModule();
            Events = new PlayerEvents();
            
            MovementModule.Initialize(
                this,input,camera);
            PushModule.Initialize(
                this,input,camera);
            InteractionModule.Initialize(
                this);
        }
        #endregion

        #region Utilities

        public void TeleportTo(Vector3 lastCheckpoint)
        {
            MovementModule.TeleportTo(lastCheckpoint);
        }
        #endregion
    }
    
}
