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

#if ENABLE_INPUT_SYSTEM
        Keyboard _keyboard;
        Mouse _mouse;
#endif

        void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (cameraRoot == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    cameraRoot = cam.transform;
            }

#if ENABLE_INPUT_SYSTEM
            _keyboard = Keyboard.current;
            _mouse = Mouse.current;
#endif

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            GroundedCheck();
            Look();
            Move();
        }

        void GroundedCheck()
        {
            Vector3 origin = transform.position + Vector3.up * groundedOffset;
            _grounded = Physics.CheckSphere(origin, _controller.radius * 0.9f, groundLayers, QueryTriggerInteraction.Ignore);
        }

        void Look()
        {
#if ENABLE_INPUT_SYSTEM
            if (_mouse == null)
                return;

            Vector2 delta = _mouse.delta.ReadValue();
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
            if (_keyboard == null)
                return;

            float h = (_keyboard.dKey.isPressed ? 1f : 0f) - (_keyboard.aKey.isPressed ? 1f : 0f);
            float v = (_keyboard.wKey.isPressed ? 1f : 0f) - (_keyboard.sKey.isPressed ? 1f : 0f);
            bool sprint = _keyboard.leftShiftKey.isPressed;
            bool jump = _keyboard.spaceKey.wasPressedThisFrame;
#else
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            bool sprint = Input.GetKey(KeyCode.LeftShift);
            bool jump = Input.GetButtonDown("Jump");
#endif

            Vector3 move = transform.right * h + transform.forward * v;
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            float speed = sprint ? sprintSpeed : walkSpeed;
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
