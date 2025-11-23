using UnityEngine;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Events;
using GameArchitecture.Core.Bootstrap;
using R3;
using R3.Unity;

namespace GameArchitecture.Gameplay
{
    /// <summary>
    /// Visualizes object movement by listening to ObjectMovedEvent.
    /// Demonstrates how visual systems can be completely decoupled from gameplay logic.
    /// This component doesn't know about PlayerController or any other movement source.
    /// </summary>
    public class MovementTrailVisualizer : MonoBehaviour
    {
        [Header("Trail Settings")]
        [SerializeField] private GameObject trailMarkerPrefab;
        [SerializeField] private float markerSpawnInterval = 0.5f;
        [SerializeField] private int maxMarkers = 20;
        [SerializeField] private float markerLifetime = 3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private IEventBus _eventBus;
        private float _lastMarkerTime;
        private readonly CompositeDisposable _disposables = new();

        private void Awake()
        {
            var projectScope = ProjectScope.Instance;
            if (projectScope == null)
            {
                Debug.LogError("[MovementTrailVisualizer] ProjectScope not found!");
                return;
            }

            var container = projectScope.GetContainer();
            _eventBus = container.Resolve<IEventBus>();
        }

        private void Start()
        {
            // Subscribe to movement events
            _eventBus.Observe<ObjectMovedEvent>()
                .Subscribe(OnObjectMoved)
                .AddTo(_disposables);

            if (showDebugLogs)
                Debug.Log("[MovementTrailVisualizer] Subscribed to ObjectMovedEvent.");
        }

        private void OnObjectMoved(ObjectMovedEvent evt)
        {
            // Only create markers at intervals
            if (Time.time - _lastMarkerTime < markerSpawnInterval)
                return;

            if (trailMarkerPrefab == null)
                return;

            // Spawn trail marker
            var marker = Instantiate(trailMarkerPrefab, evt.NewPosition, Quaternion.identity);
            _lastMarkerTime = Time.time;

            // Auto-destroy marker after lifetime
            Destroy(marker, markerLifetime);

            if (showDebugLogs)
                Debug.Log($"[MovementTrailVisualizer] Spawned marker at {evt.NewPosition}");
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}