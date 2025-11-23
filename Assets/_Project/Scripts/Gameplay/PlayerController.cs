using UnityEngine;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Events;
using GameArchitecture.Core.Bootstrap;
using R3;
using R3.Unity;

namespace GameArchitecture.Gameplay
{
    /// <summary>
    /// Player controller that responds to input events via the event bus.
    /// Demonstrates complete decoupling from the input system.
    /// All input is received through event subscriptions, not direct polling.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private CharacterController _characterController;
        private IEventBus _eventBus;
        private Vector2 _currentMoveInput;
        private Vector3 _velocity;
        private bool _jumpRequested;

        private readonly CompositeDisposable _disposables = new();

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            // Resolve EventBus from DI container
            var projectScope = ProjectScope.Instance;
            if (projectScope == null)
            {
                Debug.LogError("[PlayerController] ProjectScope not found!");
                return;
            }

            var container = projectScope.GetContainer();
            _eventBus = container.Resolve<IEventBus>();

            if (showDebugLogs)
                Debug.Log("[PlayerController] Dependencies resolved.");
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // Subscribe to movement input events
            _eventBus.Observe<PlayerMoveInputEvent>()
                .Subscribe(OnMoveInput)
                .AddTo(_disposables);

            // Subscribe to jump input events
            _eventBus.Observe<PlayerJumpInputEvent>()
                .Subscribe(OnJumpInput)
                .AddTo(_disposables);

            if (showDebugLogs)
                Debug.Log("[PlayerController] Subscribed to input events.");
        }

        private void OnMoveInput(PlayerMoveInputEvent evt)
        {
            _currentMoveInput = evt.MoveDirection;

            if (showDebugLogs && evt.MoveDirection.magnitude > 0.1f)
                Debug.Log($"[PlayerController] Move input received: {evt.MoveDirection}");
        }

        private void OnJumpInput(PlayerJumpInputEvent evt)
        {
            if (evt.IsPressed && _characterController.isGrounded)
            {
                _jumpRequested = true;

                if (showDebugLogs)
                    Debug.Log("[PlayerController] Jump input received.");
            }
        }

        private void Update()
        {
            HandleMovement();
            HandleGravity();
        }

        private void HandleMovement()
        {
            // Convert 2D input to 3D movement (forward/back, left/right)
            Vector3 move = new Vector3(_currentMoveInput.x, 0, _currentMoveInput.y);
            move = transform.TransformDirection(move);

            // Move character
            _characterController.Move(move * moveSpeed * Time.deltaTime);

            // Publish movement event if moving
            if (move.magnitude > 0.1f)
            {
                _eventBus?.Publish(new ObjectMovedEvent(
                    gameObject,
                    transform.position,
                    move * moveSpeed
                ));
            }

            // Handle jump
            if (_jumpRequested)
            {
                _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                _jumpRequested = false;

                // Publish jump event
                _eventBus?.Publish(new PlayerJumpedEvent(gameObject, jumpForce));

                if (showDebugLogs)
                    Debug.Log("[PlayerController] Player jumped!");
            }
        }

        private void HandleGravity()
        {
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to keep grounded
            }

            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();

            if (showDebugLogs)
                Debug.Log("[PlayerController] Disposed subscriptions.");
        }
    }
}