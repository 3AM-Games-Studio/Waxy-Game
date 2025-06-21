using System;
using AdvancedController;
using Core;
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
    public class PlayerController : StatefulEntity , IUpdateReceiver
    {
        public string State;
        [SerializeField,Header("Inputs")] InputReader input;
        [SerializeField] private Transform camera;
        #region Variables
        
        [Header("Movement Settings")]
        [Tooltip("Velocidad base del personaje en el suelo.")]
        public float walkSpeed;
        [Tooltip("Velocidad base del personaje en el suelo.")]
        public float runSpeed;
        
        
        [Tooltip("Qué tan rápido se puede modificar la dirección en el aire.")]
        public float airControlRate = 2f;
        
        [Tooltip("Fuerza inicial aplicada hacia arriba al iniciar un salto.")]
        public float jumpSpeed = 10f;
        
        [Tooltip("Duración máxima del salto si se mantiene la tecla presionada.")]
        public float jumpDuration = 0.2f;
        
        [Tooltip("Proporción mínima del salto que se debe ejecutar antes de permitir caída.")]
        [Range(0f, 1f)]
        public float minJumpRatio = 0.75f;
        [Tooltip("Fricción horizontal aplicada mientras está en el aire.")]
        public float airFriction = 0.5f;
        
        [Tooltip("Fricción horizontal aplicada mientras está en el suelo.")]
        public float groundFriction = 100f;
        
        [Tooltip("Fuerza de gravedad que tira hacia abajo al personaje.")]
        public float gravity = 30f;
        
        [Tooltip("Fuerza adicional aplicada al deslizar por pendientes empinadas.")]
        public float slideGravity = 5f;
        [Tooltip("Multiplicador de gravedad cuando se corta el salto antes de tiempo.")]
        public float jumpCutGravityMultiplier = 2f;

        
        [Tooltip("Ángulo máximo que se considera suelo. Más allá de este valor, el personaje se desliza.")]
        public float slopeLimit = 30f;
        
        public float CurrentSpeed { get; private set; }
        public Transform MeshPivot;
        public Vector3 MeshFoward => MeshPivot.forward;
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
            SubscribeUpdates();
        }

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
              
        private void SubscribeUpdates()
        {
            UpdateManager.RegisterToUpdate(this);
            UpdateManager.RegisterToFixedUpdate(this);
            UpdateManager.RegisterToLateUpdate(this);
        }
        
        private void UnsubscribeUpdates()
        {
            UpdateManager.UnregisterFromUpdate(this);
            UpdateManager.UnregisterFromFixedUpdate(this);
            UpdateManager.UnregisterFromLateUpdate(this);
        }
        public void Update()
        {
            stateMachine.Update();
        }
        
        public void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }
        
        public void LateUpdate()
        {
            MovementModule.LateTick();
        }
        public void OnUpdate()
        {
            // stateMachine.Update();
        }

        public void OnFixedUpdate()
        {
            // stateMachine.FixedUpdate();
        }

        public void OnLateUpdate()
        {
            // MovementModule.LateTick();
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
            At<Func<bool>>(grounded, idle, () => !input.HasMovementInput());
            At<Func<bool>>(grounded, move, () => input.HasMovementInput());

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
            At<Func<bool>>(move, running, () => isRunKeyPressed && isCandleLit);
            At<Func<bool>>(move, idle, () => !input.HasMovementInput());

            // ─────────────────────────────
            // Running
            At<Func<bool>>(running, sliding, () => MovementModule.IsGrounded() && MovementModule.IsGroundTooSteep());
            At<Func<bool>>(running, jumping, () => MovementModule.WantsToJump());
            At<Func<bool>>(running, falling, () => !MovementModule.IsGrounded());
            At<Func<bool>>(running, move, () => input.HasMovementInput() && (!isRunKeyPressed || !isCandleLit));
            At<Func<bool>>(running, idle, () => !input.HasMovementInput() && (!isRunKeyPressed || !isCandleLit));

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
        [SerializeField] private bool isCandleLit = true; // Esto luego se conecta con CandleController
        private bool isRunKeyPressed;
        public float interactionDuration;

        private void HandleRunInput(bool isButtonPressed)
        {
            isRunKeyPressed = isCandleLit && isButtonPressed;
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

        public float upDis;
        public float fowardDis;
        public LayerMask WallMask;
        private void OnDrawGizmos()
        {
            return;
            if (MeshPivot == null) return;

            // const float rayHeightOffset = 0.9f;
            // const float rayLength = 5f;

            // 1. Origen del raycast: posición del cuerpo + altura
            Vector3 origin = transform.position + Vector3.up * upDis;
            Vector3 direction = MeshPivot.forward;

            // 2. Dibujar línea de referencia
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + direction * fowardDis);

            // 3. Raycast real con máscara
            if (Physics.Raycast(origin, direction, out RaycastHit hit, fowardDis, WallMask, QueryTriggerInteraction.Ignore))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawSphere(hit.point, 0.05f);

                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
            }
        }

        #endregion
    }
    
}
