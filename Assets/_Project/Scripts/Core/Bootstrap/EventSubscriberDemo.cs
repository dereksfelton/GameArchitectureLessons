using R3;
using R3.Unity;
using UnityEngine;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Services;

namespace GameArchitecture.Core.Bootstrap
{
    /// <summary>
    /// Demonstrates event subscription and reactive patterns.
    /// Shows decoupled communication between systems without direct references.
    /// Subscribes to game state changes and scene loading events.
    /// </summary>
    public class EventSubscriberDemo : MonoBehaviour
    {
        private IEventBus _eventBus;
        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// Unity Awake callback. Resolves EventBus from ProjectScope container.
        /// </summary>
        private void Awake()
        {
            // Use the singleton instance instead of FindFirstObjectByType
            var projectScope = ProjectScope.Instance;

            if (projectScope != null)
            {
                var container = projectScope.GetContainer();
                _eventBus = container.Resolve<IEventBus>();
                Debug.Log("[EventSubscriberDemo] IEventBus resolved from container.");
            }
            else
            {
                Debug.LogError("[EventSubscriberDemo] ProjectScope instance not found!");
            }
        }

        /// <summary>
        /// Unity Start callback. Subscribes to game state and scene loading events.
        /// </summary>
        private void Start()
        {
            if (_eventBus == null)
            {
                Debug.LogError("[EventSubscriberDemo] IEventBus is null, cannot subscribe to events!");
                return;
            }

            // Subscribe to game state changes
            _eventBus.Observe<GameStateChangedEvent>()
                .Subscribe(evt =>
                {
                    Debug.Log($"[EventSubscriberDemo] Received state change event: " +
                              $"{evt.PreviousState} → {evt.NewState}");
                })
                .AddTo(_disposables);

            // Subscribe to scene load events
            _eventBus.Observe<SceneLoadCompletedEvent>()
                .Subscribe(evt =>
                {
                    Debug.Log($"[EventSubscriberDemo] Scene loaded: {evt.SceneName}");
                })
                .AddTo(_disposables);

            Debug.Log("[EventSubscriberDemo] Subscribed to events.");
        }

        /// <summary>
        /// Unity OnDestroy callback. Disposes all event subscriptions to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}