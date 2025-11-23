using Cysharp.Threading.Tasks;

namespace GameArchitecture.Core.Interfaces
{
    /// <summary>
    /// Manages game state and lifecycle.
    /// Decouples state management from game objects.
    /// Publishes state change events through the event bus for reactive responses.
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>
        /// Gets the current game state.
        /// </summary>
        /// <value>The current state from the GameState enumeration.</value>
        GameState CurrentState { get; }

        /// <summary>
        /// Initializes the game asynchronously.
        /// Transitions through Initializing state and ends in MainMenu state.
        /// </summary>
        /// <returns>A UniTask that completes when initialization is finished.</returns>
        /// <remarks>
        /// This should be called once at game startup.
        /// Performs loading of assets, setting up systems, and other initialization tasks.
        /// Publishes GameStateChangedEvent for each state transition.
        /// </remarks>
        UniTask InitializeAsync();

        /// <summary>
        /// Starts the game asynchronously.
        /// Transitions from MainMenu to Playing state.
        /// </summary>
        /// <returns>A UniTask that completes when the game has started.</returns>
        /// <remarks>
        /// Typically called when the player starts a new game or continues from a menu.
        /// May load the first level or gameplay scene.
        /// </remarks>
        UniTask StartGameAsync();

        /// <summary>
        /// Pauses the game asynchronously.
        /// Transitions from Playing to Paused state.
        /// </summary>
        /// <returns>A UniTask that completes when the game is paused.</returns>
        /// <remarks>
        /// Typically sets Time.timeScale to 0 and shows pause menu.
        /// GameManager handles the Time.timeScale change based on state events.
        /// </remarks>
        UniTask PauseGameAsync();

        /// <summary>
        /// Resumes the game from paused state asynchronously.
        /// Transitions from Paused back to Playing state.
        /// </summary>
        /// <returns>A UniTask that completes when the game has resumed.</returns>
        /// <remarks>
        /// Typically restores Time.timeScale to 1 and hides pause menu.
        /// GameManager handles the Time.timeScale change based on state events.
        /// </remarks>
        UniTask ResumeGameAsync();

        /// <summary>
        /// Shuts down the game state service.
        /// Performs cleanup before the service is destroyed.
        /// </summary>
        /// <remarks>
        /// Called when the game is exiting or the service is being replaced.
        /// Should clean up any resources or save final state.
        /// </remarks>
        void Shutdown();
    }

    /// <summary>
    /// Enumeration of possible game states.
    /// Used to track the current state of the game lifecycle.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Initial state before any initialization has occurred.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// State during initialization process.
        /// Loading assets, setting up systems, etc.
        /// </summary>
        Initializing,

        /// <summary>
        /// State when showing the main menu.
        /// Player can start game, load save, or access settings.
        /// </summary>
        MainMenu,

        /// <summary>
        /// State when actively playing the game.
        /// Game loop is running, player has control.
        /// </summary>
        Playing,

        /// <summary>
        /// State when game is paused.
        /// Game loop is halted, pause menu is shown.
        /// </summary>
        Paused,

        /// <summary>
        /// State when game has ended.
        /// Could be victory, defeat, or other end conditions.
        /// </summary>
        GameOver
    }
}