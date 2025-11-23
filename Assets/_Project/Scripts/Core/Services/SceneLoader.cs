using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameArchitecture.Core.Interfaces;

namespace GameArchitecture.Core.Services
{
    /// <summary>
    /// Async scene loader using UniTask for non-blocking operations.
    /// Publishes events for loading progress and completion.
    /// </summary>
    public class SceneLoader : ISceneLoader
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        /// Initializes a new instance of the SceneLoader class.
        /// </summary>
        /// <param name="eventBus">The event bus for publishing scene loading events.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when eventBus is null.</exception>
        public SceneLoader(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
        }

        /// <summary>
        /// Loads a scene asynchronously by name.
        /// Publishes progress events during loading and completion event when done.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="showLoadingScreen">Optional parameter to show a loading screen (not yet implemented).</param>
        /// <returns>A UniTask that completes when the scene is fully loaded.</returns>
        /// <remarks>
        /// The scene must be added to Build Settings for this to work.
        /// Progress events are published continuously during the load operation.
        /// </remarks>
        public async UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = false)
        {
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");

            _eventBus.Publish(new SceneLoadStartedEvent { SceneName = sceneName });

            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncOperation.isDone)
            {
                var progress = asyncOperation.progress;
                _eventBus.Publish(new SceneLoadProgressEvent { Progress = progress });
                await UniTask.Yield();
            }

            _eventBus.Publish(new SceneLoadCompletedEvent { SceneName = sceneName });
            Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
        }

        /// <summary>
        /// Reloads the currently active scene asynchronously.
        /// Useful for restarting levels or resetting game state.
        /// </summary>
        /// <returns>A UniTask that completes when the scene is fully reloaded.</returns>
        public async UniTask ReloadCurrentSceneAsync()
        {
            var currentScene = SceneManager.GetActiveScene().name;
            await LoadSceneAsync(currentScene);
        }
    }

    /// <summary>
    /// Event published when a scene load operation begins.
    /// </summary>
    public struct SceneLoadStartedEvent
    {
        /// <summary>
        /// The name of the scene that has started loading.
        /// </summary>
        public string SceneName;
    }

    /// <summary>
    /// Event published during scene loading to report progress.
    /// Published continuously until the scene is fully loaded.
    /// </summary>
    public struct SceneLoadProgressEvent
    {
        /// <summary>
        /// The loading progress as a value between 0 and 1.
        /// Note: Unity's progress caps at 0.9 until the scene is activated.
        /// </summary>
        public float Progress;
    }

    /// <summary>
    /// Event published when a scene has completed loading.
    /// </summary>
    public struct SceneLoadCompletedEvent
    {
        /// <summary>
        /// The name of the scene that has completed loading.
        /// </summary>
        public string SceneName;
    }
}