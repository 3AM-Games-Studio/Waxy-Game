using UnityEngine;

namespace AdvancedController {
    [RequireComponent(typeof(PlayerController))]
    public class AnimationController : MonoBehaviour {
        PlayerController controller;
        Animator animator;

        readonly int speedHash = Animator.StringToHash("Speed");
        readonly int isJumpingHash = Animator.StringToHash("Jump");
        readonly int isGoundedHash = Animator.StringToHash("Grounded");

        void Start() {
            controller = GetComponent<PlayerController>();
            animator = GetComponentInChildren<Animator>();
            //
            // controller.OnJump += HandleJump;
            // controller.OnGround += HandleGround;
        }

        void Update() {
            animator.SetFloat(speedHash, controller.GetMovementVelocity().magnitude);
        }
        public void HandleJump(bool isJumping) => animator.SetBool(isJumpingHash, isJumping);
        public void HandleGround(bool isGrounded) => animator.SetBool(isGoundedHash, isGrounded);
        void HandleJump(Vector3 momentum) => animator.SetBool(isJumpingHash, true);
        void HandleLand(Vector3 momentum) => animator.SetBool(isJumpingHash, false);
    }
}