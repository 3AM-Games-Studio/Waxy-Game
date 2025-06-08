
using AdvancedController;
using ImprovedTimers;
using UnityEngine;
using UnityUtils;

namespace Player.Modules
{
    public class MovementModule 
    {
        private PlayerController _controller;
        private InputReader _input;
        
        private Rigidbody _rb;              // asignado en Start o Initialize
        private Transform _cameraTransform; // set desde PlayerController o asignado en Inspector
        
        private Vector3 _savedVelocity;     // opcional para debug o exposición
        private bool jumpKeyIsPressed,jumpKeyWasPressed,jumpKeyWasLetGo,jumpInputIsLocked;
        private CountdownTimer jumpTimer;
        
        private CountdownTimer groundIgnoreTimer;
        private float groundIgnoreDuration = .15f;
        private Vector3 momentum;
        private Transform transform;
        private CeilingDetector ceilingDetector;
        private PlayerMover _mover;
        public void Initialize(
            PlayerController ct,
            InputReader inputs,
            Transform camera)
        {
            _controller = ct;
            transform = _controller.transform;
            _input = inputs;
            _rb = _controller.GetComponent<Rigidbody>();
            _cameraTransform = camera;
            jumpTimer = new CountdownTimer(_controller.jumpDuration);
            ceilingDetector = _controller.GetComponent<CeilingDetector>();
            _mover = _controller.GetComponent<PlayerMover>();
            groundIgnoreTimer = new CountdownTimer(groundIgnoreDuration);
        }

        public void LateTick()
        {
            ceilingDetector?.Reset();
        }
        public bool WantsToJump() =>
            jumpKeyWasPressed && !jumpInputIsLocked && IsGrounded();

        public bool HitCeiling()
        {
            bool result = ceilingDetector != null && ceilingDetector.HitCeiling();
            return result;
        }

        public bool IsRising() =>
            VectorMath.GetDotProduct(momentum, transform.up) > 0f;

        public bool IsFalling() =>
            VectorMath.GetDotProduct(momentum, transform.up) < 0f;

        public bool ShouldStartFalling()
        {
            return jumpTimer.IsFinished || jumpKeyWasLetGo;
        }
        public Vector3 GetMovementVelocity() => _savedVelocity;
        public bool IsGrounded()
        {
            if (!groundIgnoreTimer.IsFinished)
                return false;

            return _mover.IsGrounded();
        }
        public bool IsGroundTooSteep()
        {
            if (!_mover.IsGrounded())return false;

            float angle = Vector3.Angle(GetGroundNormal(), transform.up);
            return angle > _controller.slopeLimit;
        }
        public Vector3 GetGroundNormal() => _mover.GetGroundNormal();
        public void ApplyGroundedMovement()
        {
            _mover.CheckForGround();

            Vector2 inputDir = _input.Direction;

            // Dirección relativa a la cámara
            Vector3 moveDirection = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized * inputDir.x +
                                    Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized * inputDir.y;

            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            Vector3 targetVelocity = moveDirection * (_controller.CurrentSpeed * _rotationSpeedMultiplier);

            // --- Suavizar horizontal (como fricción) ---
            Vector3 horizontalMomentum = VectorMath.RemoveDotVector(momentum, transform.up);
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, targetVelocity, _controller.groundFriction * Time.fixedDeltaTime);

            // --- Eliminar momentum vertical si estamos en el suelo ---
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
            if (VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f)
                verticalMomentum = Vector3.zero;

            // --- Combinar ---
            momentum = horizontalMomentum + verticalMomentum;
            ResetJumpKeys();
            _mover.SetVelocity(momentum);
            _savedVelocity = momentum;
        }


        public void OnJumpStart()
        {
            momentum = VectorMath.RemoveDotVector(momentum, transform.up);
            momentum += transform.up * _controller.jumpSpeed;

    
            jumpTimer.Start();
            groundIgnoreTimer.Start(); // ← esto es crucial
    
            jumpInputIsLocked = true;
            _controller.Events?.OnJump?.Invoke(momentum);
        }

        public void ApplyAirMovement()
        {
            // --- Check suelo ---
            _mover.CheckForGround();

            // --- Descomponer el momentum actual ---
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
            Vector3 horizontalMomentum = VectorMath.RemoveDotVector(momentum, transform.up);

            // --- Aplicar gravedad ---
            verticalMomentum -= transform.up * (_controller.gravity * Time.fixedDeltaTime);

            // --- Calcular input horizontal ---
            Vector2 inputDir = _input.Direction;
            Vector3 inputDirection = Vector3.zero;

            if (inputDir.sqrMagnitude > 0.01f)
            {
                inputDirection = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized * inputDir.x +
                                 Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized * inputDir.y;

                if (inputDirection.sqrMagnitude > 1f)
                    inputDirection.Normalize();
            }

            Vector3 targetVelocity = inputDirection * (_controller.CurrentSpeed * _rotationSpeedMultiplier);

            // --- Aplicar control en el aire ---
            if (horizontalMomentum.magnitude > _controller.CurrentSpeed)
            {
                // Si vamos más rápido de lo deseado, reducimos el exceso
                if (Vector3.Dot(targetVelocity, horizontalMomentum.normalized) > 0f)
                    targetVelocity = VectorMath.RemoveDotVector(targetVelocity, horizontalMomentum.normalized);

                horizontalMomentum += targetVelocity * (_controller.airControlRate * 0.25f * Time.fixedDeltaTime);
            }
            else
            {
                horizontalMomentum += targetVelocity * (_controller.airControlRate * Time.fixedDeltaTime);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, _controller.CurrentSpeed);
            }

            // --- Aplicar fricción aérea (suavemente) ---
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, _controller.airFriction * Time.fixedDeltaTime);

            // --- Combinar ---
            momentum = verticalMomentum + horizontalMomentum;

            // --- Aplicar movimiento al cuerpo ---
            _mover.SetVelocity(momentum);
            ResetJumpKeys();
            // --- Guardar ---
            _savedVelocity = momentum;
        }

        public void OnFallStart()
        {
            // Eliminar cualquier impulso hacia arriba
            Vector3 upMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
            if (VectorMath.GetDotProduct(upMomentum, transform.up) > 0f)
            {
                momentum = VectorMath.RemoveDotVector(momentum, transform.up);
            }

            // Aplicar un pequeño impulso hacia abajo para forzar el inicio de la caída
            momentum -= transform.up * (_controller.gravity * Time.fixedDeltaTime);

            // Aplicar inmediatamente el nuevo momentum
            _mover.SetVelocity(momentum);
            _savedVelocity = momentum;
        }
        public void SetJumpInput(bool isButtonPressed)
        {
    
            if (!jumpKeyIsPressed && isButtonPressed)
                jumpKeyWasPressed = true;

            if (jumpKeyIsPressed && !isButtonPressed)
            {
                jumpKeyWasLetGo = true;
                jumpInputIsLocked = false;
            }

            jumpKeyIsPressed = isButtonPressed;
        }
        public void OnGroundContactRegained()
        {
            _mover.CheckForGround();
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);

            // Si venías cayendo, eliminamos ese momentum vertical
            if (VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f)
                momentum = VectorMath.RemoveDotVector(momentum, transform.up);

            // Si no hay input horizontal, eliminamos también el horizontal
            if (_input.Direction.sqrMagnitude < 0.01f)
                momentum = Vector3.zero;
        }
        
        public void ApplyIdleFriction()
        {
            _mover.CheckForGround();

            // Separar momentum
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
            Vector3 horizontalMomentum = VectorMath.RemoveDotVector(momentum, transform.up);

            // Aplicar fricción de frenado (Adam-style)
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, _controller.groundFriction * Time.fixedDeltaTime);

            // Eliminar rebote hacia arriba si está en el piso
            if (VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f)
                verticalMomentum = Vector3.zero;

            // Combinar
            momentum = verticalMomentum + horizontalMomentum;

            _mover.SetVelocity(momentum);
            _savedVelocity = momentum;
        }
        
        public void ApplySlideMovement()
        {
            _mover.CheckForGround();

            // 1. Dirección principal de deslizamiento
            Vector3 groundNormal = GetGroundNormal();
            Vector3 slideDirection = Vector3.ProjectOnPlane(-transform.up, groundNormal).normalized;

            // 2. Aplicar fuerza de deslizamiento
            Vector3 slideVelocity = slideDirection * (_controller.slideGravity * Time.fixedDeltaTime);

            // 3. Input del jugador (como en grounded)
            Vector2 inputDir = _input.Direction;
            Vector3 inputDirection = Vector3.zero;

            if (inputDir.sqrMagnitude > 0.01f)
            {
                inputDirection = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized * inputDir.x +
                                 Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized * inputDir.y;

                if (inputDirection.sqrMagnitude > 1f)
                    inputDirection.Normalize();
            }

            // 4. Input más limitado para que no domine el deslizamiento
            Vector3 playerInfluence = inputDirection * (_controller.CurrentSpeed * 0.25f); // 25% del control normal
            Vector3 horizontalMomentum = VectorMath.RemoveDotVector(momentum, transform.up);
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, playerInfluence, _controller.groundFriction * Time.fixedDeltaTime);

            // 5. Aplicar ambas fuerzas horizontales
            horizontalMomentum += slideVelocity;

            // 6. Vertical (por si hubiera impulso hacia abajo acumulado)
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
            if (VectorMath.GetDotProduct(verticalMomentum, transform.up) > 0f)
            {
                verticalMomentum = Vector3.zero; // no queremos impulso hacia arriba al deslizar
            }

            // 7. Combinar y aplicar
            momentum = horizontalMomentum + verticalMomentum;
            _mover.SetVelocity(momentum);
            _savedVelocity = momentum;
        }
        private void ResetJumpKeys()
        {
            jumpKeyWasPressed = false;
            jumpKeyWasLetGo = false;
        }
        private float _rotationSpeedMultiplier = 1f;
        public void SetRotationSpeedMultiplier(float rotMultiplier)
        {
            _rotationSpeedMultiplier = Mathf.Clamp01(rotMultiplier);
        }

        public void ApplyPushMovement()
        {
            _mover.CheckForGround();

            Vector3 move = _controller.PushModule.GetPlayerMoveDirection();
            Vector3 targetVelocity = move * (_controller.walkSpeed * _rotationSpeedMultiplier);

            Vector3 horizontalMomentum = VectorMath.RemoveDotVector(momentum, transform.up);
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, targetVelocity, _controller.groundFriction * Time.fixedDeltaTime);

            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
            if (VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f)
                verticalMomentum = Vector3.zero;

            momentum = horizontalMomentum + verticalMomentum;

            _mover.SetVelocity(momentum);
            _savedVelocity = momentum;
        }
    }
    
}