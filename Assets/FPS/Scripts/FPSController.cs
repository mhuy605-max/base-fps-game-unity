using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FPSGame
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5f;
        public float sprintSpeed = 8f;
        public float jumpHeight = 1.2f;
        public float gravity = -20f;

        [Header("Look")]
        public Transform cameraRoot;
        public float mouseSensitivity = 2f;
        public float maxPitch = 85f;

        [Header("Ground Check")]
        public float groundedOffset = 0.1f;
        public LayerMask groundLayers = ~0;

        CharacterController _controller;
        float _pitch;
        float _verticalVelocity;
        bool _grounded;
        bool _isSprinting;
        bool _jumpRequested;
        Vector2 _movementInput;
        Vector2 _lookInput;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (cameraRoot == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    cameraRoot = cam.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            GroundedCheck();
            Look();
            Move();
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMovement(InputAction.CallbackContext context)
        {
            _movementInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                _jumpRequested = true;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
                _isSprinting = true;
            else if (context.canceled)
                _isSprinting = false;
        }
#endif

        void GroundedCheck()
        {
            Vector3 origin = transform.position + Vector3.up * groundedOffset;
            _grounded = Physics.CheckSphere(origin, _controller.radius * 0.9f, groundLayers, QueryTriggerInteraction.Ignore);
        }

        void Look()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 delta = _lookInput;
            float yaw = delta.x * mouseSensitivity * 0.1f;
            float pitch = -delta.y * mouseSensitivity * 0.1f;
#else
            float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
            float pitch = -Input.GetAxis("Mouse Y") * mouseSensitivity;
#endif

            transform.Rotate(Vector3.up, yaw);

            _pitch = Mathf.Clamp(_pitch + pitch, -maxPitch, maxPitch);
            if (cameraRoot != null)
                cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        void Move()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 movement = _movementInput;
            bool jump = _jumpRequested;
            _jumpRequested = false;
#else
            Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            bool jump = Input.GetButtonDown("Jump");
#endif

            Vector3 move = transform.right * movement.x + transform.forward * movement.y;
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            float speed = _isSprinting ? sprintSpeed : walkSpeed;
            _controller.Move(move * speed * Time.deltaTime);

            if (_grounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (_grounded && jump)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            _verticalVelocity += gravity * Time.deltaTime;
            _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        }
    }
}
