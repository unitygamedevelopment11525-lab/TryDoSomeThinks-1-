using UnityEngine;

namespace Project.Scripts.Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.0f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Weight Penalty Settings")]
        [Tooltip("Maximum penalty multiplier (e.g. 0.5 means speed can drop to 50% of original speed at full inventory weight)")]
        [Range(0f, 1f)] [SerializeField] private float maxWeightSpeedPenalty = 0.5f;

        [Header("Camera Settings")]
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private float lookSensitivityX = 0.1f;
        [SerializeField] private float lookSensitivityY = 0.1f;
        [SerializeField] private float upperLookLimit = 90.0f;
        [SerializeField] private float lowerLookLimit = -90.0f;

        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchCameraYOffset = 0.5f;
        [SerializeField] private float standingCameraYOffset = 0.8f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        private CharacterController _characterController;
        private InputSystem_Actions _inputActions;
        private Vector3 _velocity;
        private float _verticalRotation;
        private bool _isGrounded;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        private void Start()
        {
            // Lock cursor for first-person gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleRotation();
            HandleMovement();
        }

        private void HandleRotation()
        {
            if (cameraHolder == null) return;

            Vector2 lookInput = _inputActions.Player.Look.ReadValue<Vector2>();

            float mouseX = lookInput.x * lookSensitivityX;
            float mouseY = lookInput.y * lookSensitivityY;

            // Horizontal rotation (turns the entire body)
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation (turns the camera holder only, clamped to look straight up/down)
            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, lowerLookLimit, upperLookLimit);
            cameraHolder.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        }

        private void HandleMovement()
        {
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Slight downward force to keep grounded
            }

            // 1. Read movement input
            Vector2 moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

            // 2. Determine target speed state (Walk / Sprint / Crouch)
            bool isSprinting = _inputActions.Player.Sprint.IsPressed();
            bool isCrouching = _inputActions.Player.Crouch.IsPressed();

            float targetSpeed = walkSpeed;

            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (isSprinting && moveInput.y > 0) // Only sprint when moving forward
            {
                targetSpeed = sprintSpeed;
            }

            // 3. Apply Weight Penalty dynamically from Inventory
            float speedMultiplier = GetWeightSpeedMultiplier();
            float finalSpeed = targetSpeed * speedMultiplier;

            // 4. Move the character controller
            _characterController.Move(moveDirection * (finalSpeed * Time.deltaTime));

            // 5. Handle crouching size / camera height transitions
            HandleCrouchTransition(isCrouching);

            // 6. Handle Jumping
            if (_inputActions.Player.Jump.triggered && _isGrounded)
            {
                // Formula: v = sqrt(h * -2 * g)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // 7. Apply gravity & vertical movement
            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private float GetWeightSpeedMultiplier()
        {
            if (Inventory.Inventory.Instance == null) return 1f;

            float currentWeight = Inventory.Inventory.Instance.GetCurrentWeight();
            float maxWeight = Inventory.Inventory.Instance.maxWeight;

            if (maxWeight <= 0f) return 1f;

            // Weight ratio from 0 to 1
            float weightRatio = Mathf.Clamp01(currentWeight / maxWeight);

            // Interpolate speed multiplier based on the penalty setting
            // E.g., if maxWeightSpeedPenalty = 0.5, speed multiplier goes from 1.0 down to 0.5 at full weight
            return Mathf.Lerp(1f, 1f - maxWeightSpeedPenalty, weightRatio);
        }

        private void HandleCrouchTransition(bool isCrouching)
        {
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            float newHeight = Mathf.Lerp(_characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            _characterController.height = newHeight;
            _characterController.center = new Vector3(0f, newHeight / 2f, 0f);

            if (cameraHolder != null)
            {
                float targetCameraY = isCrouching ? crouchCameraYOffset : standingCameraYOffset;
                Vector3 localPos = cameraHolder.localPosition;
                localPos.y = Mathf.Lerp(localPos.y, targetCameraY, crouchTransitionSpeed * Time.deltaTime);
                cameraHolder.localPosition = localPos;
            }
        }
    }
}
