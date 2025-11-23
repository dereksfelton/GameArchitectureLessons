using Reflex.Core;
using UnityEngine;
using GameArchitecture.Core.Events;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Services;

namespace GameArchitecture.Core.Bootstrap
{
    /// <summary>
    /// Project-level dependency injection container.
    /// Persists across scenes and provides global services.
    /// Implements singleton pattern to ensure only one instance exists.
    /// </summary>
    public class ProjectScope : MonoBehaviour, IInstaller
    {
        private static ProjectScope _instance;
        private Container _container;

        /// <summary>
        /// Gets the singleton instance of ProjectScope.
        /// Returns null if ProjectScope has not been initialized yet.
        /// </summary>
        public static ProjectScope Instance => _instance;
        
        /// <summary>
        /// Unity Awake callback. Initializes the singleton and builds the DI container.
        /// Ensures persistence across scene loads via DontDestroyOnLoad.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern to prevent duplicates
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[ProjectScope] Duplicate ProjectScope detected, destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Create and build the container
            var builder = new ContainerBuilder();
            InstallBindings(builder);
            _container = builder.Build();

            Debug.Log("[ProjectScope] Container built successfully.");
        }

        /// <summary>
        /// Installs all project-level service bindings into the DI container.
        /// Called by Reflex during container initialization.
        /// </summary>
        /// <param name="containerBuilder">The container builder to register services with.</param>
        /// <remarks>
        /// Services registered here persist across all scenes and are available globally.
        /// Each service is registered both as its concrete type and interface type.
        /// </remarks>
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            // Register EventBus with factory function
            containerBuilder.AddSingleton(c => new EventBus());
            containerBuilder.AddSingleton<IEventBus>(c => c.Resolve<EventBus>());
            
            // Register GameStateService with factory function
            containerBuilder.AddSingleton(c => new GameStateService(c.Resolve<IEventBus>()));
            containerBuilder.AddSingleton<IGameStateService>(c => c.Resolve<GameStateService>());
            
            // Register SceneLoader with factory function
            containerBuilder.AddSingleton(c => new SceneLoader(c.Resolve<IEventBus>()));
            containerBuilder.AddSingleton<ISceneLoader>(c => c.Resolve<SceneLoader>());

            Debug.Log("[ProjectScope] Project-level services registered.");
        }

        /// <summary>
        /// Gets the DI container for manual service resolution.
        /// Other components can call this to resolve dependencies without using [Inject] attributes.
        /// </summary>
        /// <returns>The Reflex Container instance containing all registered services.</returns>
        /// <remarks>
        /// This method is typically called by GameManager, EventSubscriberDemo, and other
        /// components that need to manually resolve services from the container.
        /// </remarks>
        public Container GetContainer()
        {
            return _container;
        }
    }
}