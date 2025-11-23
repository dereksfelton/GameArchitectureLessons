using UnityEngine;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Events;
using GameArchitecture.Core.Bootstrap;
using R3;
using R3.Unity;

namespace GameArchitecture.Gameplay
{
    /// <summary>
    /// Spawns visual effects when PlayerJumpedEvent is received.
    /// Demonstrates visual system responding to gameplay events without direct coupling.
    /// This component doesn't need any reference to the player or controller.
    /// </summary>
    public class JumpEffectVisualizer : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private GameObject jumpEffectPrefab;
        [SerializeField] private float effectLifetime = 1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private IEventBus _eventBus;
        private readonly CompositeDisposable _disposables = new();

        private void Awake()
        {
            var projectScope = ProjectScope.Instance;
            if (projectScope == null)
            {
                Debug.LogError("[JumpEffectVisualizer] ProjectScope not found!");
                return;
            }

            var container = projectScope.GetContainer();
            _eventBus = container.Resolve<IEventBus>();
        }

        private void Start()
        {
            // Subscribe to jump events
            _eventBus.Observe<PlayerJumpedEvent>()
                .Subscribe(OnPlayerJumped)
                .AddTo(_disposables);

            if (showDebugLogs)
                Debug.Log("[JumpEffectVisualizer] Subscribed to PlayerJumpedEvent.");
        }

        private void OnPlayerJumped(PlayerJumpedEvent evt)
        {
            if (jumpEffectPrefab == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[JumpEffectVisualizer] No jump effect prefab assigned!");
                return;
            }

            // Spawn jump effect at player position
            var effect = Instantiate(jumpEffectPrefab, evt.Player.transform.position, Quaternion.identity);
            Destroy(effect, effectLifetime);

            if (showDebugLogs)
                Debug.Log($"[JumpEffectVisualizer] Jump effect spawned for {evt.Player.name}");
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}