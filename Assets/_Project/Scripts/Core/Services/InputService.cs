using UnityEngine;
using UnityEngine.InputSystem;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Events;

namespace GameArchitecture.Core.Services
{
    /// <summary>
    /// Service that bridges Unity's Input System and the event bus.
    /// Uses direct InputActionAsset reference instead of generated wrapper class.
    /// Translates input callbacks into game events for decoupled input handling.
    /// </summary>
    public class InputService : IInputService
    {
        private readonly IEventBus _eventBus;
        private readonly InputActionAsset _inputAsset;
        private readonly InputActionMap _gameplayMap;
        
        // Cache action references
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _interactAction;

        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Creates a new InputService instance.
        /// </summary>
        /// <param name="eventBus">Event bus for publishing input events.</param>
        /// <param name="inputAsset">The Input Actions asset to use.</param>
        public InputService(IEventBus eventBus, InputActionAsset inputAsset)
        {
            _eventBus = eventBus;
            _inputAsset = inputAsset;
            
            if (_inputAsset == null)
            {
                Debug.LogError("[InputService] InputActionAsset is null!");
                return;
            }
            
            // Get the Gameplay action map
            _gameplayMap = _inputAsset.FindActionMap("Gameplay");
            
            if (_gameplayMap == null)
            {
                Debug.LogError("[InputService] Could not find 'Gameplay' action map in InputActionAsset!");
                Debug.LogError($"[InputService] Available action maps: {string.Join(", ", GetActionMapNames())}");
                return;
            }
            
            // Cache action references
            _moveAction = _gameplayMap.FindAction("Move");
            _jumpAction = _gameplayMap.FindAction("Jump");
            _interactAction = _gameplayMap.FindAction("Interact");
            
            // Verify actions exist
            if (_moveAction == null) Debug.LogWarning("[InputService] 'Move' action not found!");
            if (_jumpAction == null) Debug.LogWarning("[InputService] 'Jump' action not found!");
            if (_interactAction == null) Debug.LogWarning("[InputService] 'Interact' action not found!");
            
            // Subscribe to input action callbacks
            SubscribeToInputActions();

            Debug.Log("[InputService] Initialized and subscribed to input actions.");
        }

        private string[] GetActionMapNames()
        {
            var maps = new System.Collections.Generic.List<string>();
            foreach (var map in _inputAsset.actionMaps)
            {
                maps.Add(map.name);
            }
            return maps.ToArray();
        }

        private void SubscribeToInputActions()
        {
            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }
            
            if (_jumpAction != null)
            {
                _jumpAction.performed += OnJumpPerformed;
                _jumpAction.canceled += OnJumpCanceled;
            }
            
            if (_interactAction != null)
            {
                _interactAction.performed += OnInteractPerformed;
                _interactAction.canceled += OnInteractCanceled;
            }
        }

        private void UnsubscribeFromInputActions()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }
            
            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpPerformed;
                _jumpAction.canceled -= OnJumpCanceled;
            }
            
            if (_interactAction != null)
            {
                _interactAction.performed -= OnInteractPerformed;
                _interactAction.canceled -= OnInteractCanceled;
            }
        }

        #region Input Callbacks

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            var moveDirection = context.ReadValue<Vector2>();
            _eventBus.Publish(new PlayerMoveInputEvent(moveDirection));
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _eventBus.Publish(new PlayerMoveInputEvent(Vector2.zero));
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _eventBus.Publish(new PlayerJumpInputEvent(true));
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            _eventBus.Publish(new PlayerJumpInputEvent(false));
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            _eventBus.Publish(new PlayerInteractInputEvent(true));
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            _eventBus.Publish(new PlayerInteractInputEvent(false));
        }

        #endregion

        public void Enable()
        {
            if (_inputAsset == null)
            {
                Debug.LogError("[InputService] Cannot enable - InputActionAsset is null!");
                return;
            }
            
            _inputAsset.Enable();
            IsEnabled = true;
            Debug.Log("[InputService] Input enabled.");
        }

        public void Disable()
        {
            if (_inputAsset == null)
            {
                Debug.LogWarning("[InputService] Cannot disable - InputActionAsset is null!");
                return;
            }
            
            _inputAsset.Disable();
            IsEnabled = false;
            Debug.Log("[InputService] Input disabled.");
        }

        public void Dispose()
        {
            Disable();
            UnsubscribeFromInputActions();
            Debug.Log("[InputService] Disposed.");
        }
    }
}