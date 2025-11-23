using Reflex.Core;
using UnityEngine;
using GameArchitecture.Core.Events;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Services;

namespace GameArchitecture.Core.Utilities
{
    /// <summary>
    /// Debugging utility for Reflex DI container.
    /// Helps identify registration and resolution issues by attempting to resolve all registered services.
    /// Can be invoked automatically on Start or manually via the context menu.
    /// </summary>
    /// <remarks>
    /// Attach this component to any GameObject in the scene to debug the DI container.
    /// Useful for troubleshooting dependency injection issues during development.
    /// </remarks>
    public class ReflexDebugger : MonoBehaviour
    {
        [SerializeField] 
        [Tooltip("Automatically run container debug on Start(). Disable for manual debugging only.")]
        private bool _debugOnStart = true;
        
        /// <summary>
        /// Unity Start callback. Optionally runs container debug if _debugOnStart is enabled.
        /// </summary>
        private void Start()
        {
            if (_debugOnStart)
            {
                DebugContainer();
            }
        }

        /// <summary>
        /// Debugs the Reflex DI container by attempting to resolve all registered services.
        /// Can be invoked from the Inspector context menu by right-clicking the component.
        /// </summary>
        /// <remarks>
        /// Outputs results to the Unity Console:
        /// - ✓ Green checkmark for successful resolutions
        /// - ⚠ Warning for null resolutions
        /// - ✗ Error for failed resolutions with exception details
        /// </remarks>
        [ContextMenu("Debug Container")]
        public void DebugContainer()
        {
            // Use the singleton instance to get the container
            var projectScope = GameArchitecture.Core.Bootstrap.ProjectScope.Instance;
            
            if (projectScope == null)
            {
                Debug.LogError("[ReflexDebugger] ProjectScope instance not found! " +
                              "Ensure ProjectScope GameObject exists in the scene.");
                return;
            }

            var container = projectScope.GetContainer();
            
            if (container == null)
            {
                Debug.LogError("[ReflexDebugger] Container is null! " +
                              "ProjectScope may not have initialized properly.");
                return;
            }

            Debug.Log("=== Reflex Container Debug ===");
            
            // Try to resolve each registered service
            TryResolve<EventBus>(container, "EventBus (concrete)");
            TryResolve<IEventBus>(container, "IEventBus (interface)");
            TryResolve<GameStateService>(container, "GameStateService (concrete)");
            TryResolve<IGameStateService>(container, "IGameStateService (interface)");
            TryResolve<SceneLoader>(container, "SceneLoader (concrete)");
            TryResolve<ISceneLoader>(container, "ISceneLoader (interface)");
            
            Debug.Log("=== End Container Debug ===");
        }

        /// <summary>
        /// Attempts to resolve a service from the container and logs the result.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <param name="container">The Reflex container to resolve from.</param>
        /// <param name="serviceName">Human-readable name for the service (used in log output).</param>
        /// <remarks>
        /// Handles three cases:
        /// 1. Successful resolution - logs success with resolved type name
        /// 2. Null resolution - logs warning
        /// 3. Exception during resolution - logs error with exception message
        /// </remarks>
        private void TryResolve<T>(Container container, string serviceName)
        {
            try
            {
                var service = container.Resolve<T>();
                if (service != null)
                {
                    Debug.Log($"✓ {serviceName}: Resolved successfully ({service.GetType().Name})");
                }
                else
                {
                    Debug.LogWarning($"⚠ {serviceName}: Resolved to null");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"✗ {serviceName}: Resolution failed - {ex.Message}");
            }
        }
    }
}