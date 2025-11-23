using Cysharp.Threading.Tasks;

namespace GameArchitecture.Core.Interfaces
{
    /// <summary>
    /// Handles scene loading operations asynchronously.
    /// Decouples scene management from game logic.
    /// Publishes events for loading progress and completion.
    /// </summary>
    /// <remarks>
    /// Uses UniTask for non-blocking async operations.
    /// Scenes must be added to Build Settings to be loadable.
    /// Publishes SceneLoadStartedEvent, SceneLoadProgressEvent, and SceneLoadCompletedEvent.
    /// </remarks>
    public interface ISceneLoader
    {
        /// <summary>
        /// Loads a scene asynchronously by name.
        /// Publishes progress events during loading and completion event when done.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load (must match Build Settings).</param>
        /// <param name="showLoadingScreen">
        /// Optional parameter to show a loading screen during the transition.
        /// Currently not implemented - reserved for future use.
        /// </param>
        /// <returns>A UniTask that completes when the scene is fully loaded and activated.</returns>
        /// <remarks>
        /// The scene is loaded additively by default.
        /// Progress events are published continuously with values from 0 to 1.
        /// Note: Unity's progress caps at 0.9 until the scene is fully activated.
        /// </remarks>
        UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = false);

        /// <summary>
        /// Reloads the currently active scene asynchronously.
        /// Useful for restarting levels or resetting game state.
        /// </summary>
        /// <returns>A UniTask that completes when the scene is fully reloaded.</returns>
        /// <remarks>
        /// This is equivalent to calling LoadSceneAsync with the current scene's name.
        /// All objects in the current scene will be destroyed and recreated.
        /// DontDestroyOnLoad objects will persist through the reload.
        /// </remarks>
        UniTask ReloadCurrentSceneAsync();
    }
}