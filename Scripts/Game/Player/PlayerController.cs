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
        [Tooltip("Шари (Layers), які вважаються стелею/перешкодою над головою (ОБОВ'ЯЗКОВО вимкни шар гравця!)")]
        [SerializeField] private LayerMask obstacleLayers = ~0; 

        private CharacterController _characterController;
        private InputSystem_Actions _inputActions;
        private Vector3 _velocity;
        private float _verticalRotation;
        private bool _isGrounded;
        
        private bool _isCrouchingState;

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

            transform.Rotate(Vector3.up * mouseX);

            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, lowerLookLimit, upperLookLimit);
            cameraHolder.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        }

        private void HandleMovement()
        {
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            Vector2 moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

            bool isSprinting = _inputActions.Player.Sprint.IsPressed();
            bool crouchKeyPressed = _inputActions.Player.Crouch.IsPressed();

            // Логіка перевірки стелі
            if (crouchKeyPressed)
            {
                _isCrouchingState = true;
            }
            else if (_isCrouchingState && IsObstacleAbove())
            {
                _isCrouchingState = true;
            }
            else
            {
                _isCrouchingState = false;
            }

            float targetSpeed = walkSpeed;

            if (_isCrouchingState)
            {
                targetSpeed = crouchSpeed;
            }
            else if (isSprinting && moveInput.y > 0)
            {
                targetSpeed = sprintSpeed;
            }

            float speedMultiplier = GetWeightSpeedMultiplier();
            float finalSpeed = targetSpeed * speedMultiplier;

            _characterController.Move(moveDirection * (finalSpeed * Time.deltaTime));

            HandleCrouchTransition(_isCrouchingState);

            if (_inputActions.Player.Jump.triggered && _isGrounded && !_isCrouchingState)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// Перевіряє, чи заважає щось персонажу встати на повний зріст.
        /// </summary>
        private bool IsObstacleAbove()
        {
            float radius = _characterController.radius * 0.9f;
            
            // Стартуємо з поточної вершини присідаючого коллайдера (трохи нижче від його реального топу, щоб не було багів)
            // Використовуємо поточну висоту characterController.height
            Vector3 startCenter = transform.position + Vector3.up * (_characterController.height - radius);
            
            // Кінцева точка верхньої сфери повинна бути на висоті standingHeight - radius
            Vector3 endCenter = transform.position + Vector3.up * (standingHeight - radius);
            
            // Відстань, яку сфері потрібно пройти вгору від поточної голови до бажаної голови
            float castDistance = endCenter.y - startCenter.y;

            // Якщо ми вже встали або майже встали, castDistance може бути <= 0
            if (castDistance <= 0.01f) return false;

            // Пускаємо сферу від поточної голови вгору
            return Physics.SphereCast(startCenter, radius, Vector3.up, out RaycastHit hit, castDistance, obstacleLayers);
        }

        private float GetWeightSpeedMultiplier()
        {
            if (Inventory.Inventory.Instance == null) return 1f;

            float currentWeight = Inventory.Inventory.Instance.GetCurrentWeight();
            float maxWeight = Inventory.Inventory.Instance.maxWeight;

            if (maxWeight <= 0f) return 1f;

            float weightRatio = Mathf.Clamp01(currentWeight / maxWeight);
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
        
        private void OnDrawGizmosSelected()
        {
            if (_characterController == null) _characterController = GetComponent<CharacterController>();
            if (_characterController == null) return;

            // Візуалізація фінальної точки перевірки (де опиниться голова, якщо ми встанемо)
            Gizmos.color = Color.red;
            float radius = _characterController.radius * 0.9f;
            Vector3 endCenter = transform.position + Vector3.up * (standingHeight - radius);
            
            Gizmos.DrawWireSphere(endCenter, radius);
            
            // Малюємо лінію від поточної голови до майбутньої
            Vector3 startCenter = transform.position + Vector3.up * (_characterController.height - radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startCenter, endCenter);
        }
    }
}