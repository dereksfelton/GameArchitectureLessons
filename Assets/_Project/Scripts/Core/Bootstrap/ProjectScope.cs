using UnityEngine;
using Reflex.Core;
using GameArchitecture.Core.Events;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Services;

namespace GameArchitecture.Core.Bootstrap
{
    /// <summary>
    /// Project-wide dependency injection scope that persists across all scenes.
    /// Implements singleton pattern to ensure only one instance exists.
    /// Uses modular installers for clean separation of concerns.
    /// </summary>
    public class ProjectScope : MonoBehaviour
    {
        [Header("Service Installers")]
        [SerializeField] 
        [Tooltip("Optional ScriptableObject installers for modular service registration")]
        private ScriptableObject[] serviceInstallers;
        
        private static ProjectScope _instance;
        private Container _container;

        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static ProjectScope Instance => _instance;

        private void Awake()
        {
            // Singleton pattern: Destroy duplicates
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[ProjectScope] Duplicate instance detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Build the DI container
            var builder = new ContainerBuilder();
            InstallBindings(builder);
            _container = builder.Build();

            Debug.Log("[ProjectScope] Container built successfully.");
        }

        /// <summary>
        /// Installs all project-level service bindings into the DI container.
        /// First registers core services, then runs modular installers.
        /// </summary>
        /// <param name="containerBuilder">The container builder to register services with.</param>
        /// <remarks>
        /// Core services (EventBus, GameStateService, SceneLoader) are registered directly.
        /// Additional services (Input, Audio, etc.) are registered via installers for modularity.
        /// </remarks>
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            // Register core services (no external dependencies)
            var eventBus = RegisterCoreServices(containerBuilder);

            // Run modular installers for optional services
            RunInstallers(containerBuilder, eventBus);

            Debug.Log("[ProjectScope] All services registered.");
        }

        /// <summary>
        /// Registers core services that have no external dependencies.
        /// These services are always registered regardless of installers.
        /// Returns the EventBus instance for use by installers.
        /// </summary>
        private IEventBus RegisterCoreServices(ContainerBuilder containerBuilder)
        {
            // Register EventBus with factory functions
            var eventBus = new EventBus();
            containerBuilder.AddSingleton(container => eventBus);
            containerBuilder.AddSingleton<IEventBus>(container => eventBus);

            // Register GameStateService with factory functions
            var gameStateService = new GameStateService(eventBus);
            containerBuilder.AddSingleton(container => gameStateService);
            containerBuilder.AddSingleton<IGameStateService>(container => gameStateService);

            // Register SceneLoader with factory functions
            var sceneLoader = new SceneLoader(eventBus);
            containerBuilder.AddSingleton(container => sceneLoader);
            containerBuilder.AddSingleton<ISceneLoader>(container => sceneLoader);

            Debug.Log("[ProjectScope] Core services registered.");
            
            return eventBus;
        }

        /// <summary>
        /// Runs all configured service installers.
        /// Installers provide modular service registration without coupling ProjectScope to specific systems.
        /// </summary>
        private void RunInstallers(ContainerBuilder containerBuilder, IEventBus eventBus)
        {
            if (serviceInstallers == null || serviceInstallers.Length == 0)
            {
                Debug.LogWarning("[ProjectScope] No service installers configured.");
                return;
            }

            foreach (var installer in serviceInstallers)
            {
                if (installer == null)
                {
                    Debug.LogWarning("[ProjectScope] Null installer in array, skipping.");
                    continue;
                }

                if (installer is IServiceInstaller serviceInstaller)
                {
                    Debug.Log($"[ProjectScope] Running installer: {installer.name}");
                    serviceInstaller.Install(containerBuilder, eventBus);
                }
                else
                {
                    Debug.LogWarning($"[ProjectScope] {installer.name} does not implement IServiceInstaller interface!");
                }
            }
        }

        /// <summary>
        /// Gets the DI container for manual service resolution.
        /// Other components can call this to resolve dependencies without using [Inject] attributes.
        /// </summary>
        /// <returns>The Reflex Container instance containing all registered services.</returns>
        /// <remarks>
        /// This method is typically called by GameManager and other components
        /// that need to manually resolve services from the container.
        /// </remarks>
        public Container GetContainer()
        {
            return _container;
        }

        private void OnValidate()
        {
            // Editor-time validation
            if (serviceInstallers != null)
            {
                foreach (var installer in serviceInstallers)
                {
                    if (installer != null && !(installer is IServiceInstaller))
                    {
                        Debug.LogWarning($"[ProjectScope] {installer.name} does not implement IServiceInstaller!");
                    }
                }
            }
        }
    }
}