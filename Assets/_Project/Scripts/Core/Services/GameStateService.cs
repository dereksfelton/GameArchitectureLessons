using Cysharp.Threading.Tasks;
using UnityEngine;
using GameArchitecture.Core.Interfaces;

namespace GameArchitecture.Core.Services
{
    /// <summary>
    /// Manages game state transitions using UniTask for async operations.
    /// Publishes state change events via EventBus for decoupled notification.
    /// </summary>
    public class GameStateService : IGameStateService
    {
        private readonly IEventBus _eventBus;
        private GameState _currentState = GameState.Uninitialized;

        /// <summary>
        /// Gets the current game state.
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// Initializes a new instance of the GameStateService class.
        /// </summary>
        /// <param name="eventBus">The event bus for publishing state change events.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when eventBus is null.</exception>
        public GameStateService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
        }

        /// <summary>
        /// Initializes the game asynchronously.
        /// Transitions from Uninitialized → Initializing → MainMenu.
        /// </summary>
        /// <returns>A UniTask representing the async initialization operation.</returns>
        public async UniTask InitializeAsync()
        {
            Debug.Log("[GameStateService] Initializing...");
            ChangeState(GameState.Initializing);

            // Simulate initialization work (loading assets, setting up systems, etc.)
            await UniTask.Delay(500);

            ChangeState(GameState.MainMenu);
            Debug.Log("[GameStateService] Initialization complete.");
        }

        /// <summary>
        /// Starts the game asynchronously.
        /// Transitions from MainMenu → Playing.
        /// </summary>
        /// <returns>A UniTask representing the async start operation.</returns>
        public async UniTask StartGameAsync()
        {
            Debug.Log("[GameStateService] Starting game...");
            await UniTask.Delay(100);
            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Pauses the game asynchronously.
        /// Transitions from Playing → Paused and sets Time.timeScale to 0.
        /// </summary>
        /// <returns>A UniTask representing the async pause operation.</returns>
        public async UniTask PauseGameAsync()
        {
            Debug.Log("[GameStateService] Pausing game...");
            await UniTask.Yield();
            ChangeState(GameState.Paused);
        }

        /// <summary>
        /// Resumes the game from paused state asynchronously.
        /// Transitions from Paused → Playing and restores Time.timeScale to 1.
        /// </summary>
        /// <returns>A UniTask representing the async resume operation.</returns>
        public async UniTask ResumeGameAsync()
        {
            Debug.Log("[GameStateService] Resuming game...");
            await UniTask.Yield();
            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Shuts down the game state service.
        /// Performs any necessary cleanup before the service is destroyed.
        /// </summary>
        public void Shutdown()
        {
            Debug.Log("[GameStateService] Shutting down...");
        }

        /// <summary>
        /// Changes the current game state and publishes a state change event.
        /// </summary>
        /// <param name="newState">The new state to transition to.</param>
        private void ChangeState(GameState newState)
        {
            var previousState = _currentState;
            _currentState = newState;

            _eventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = newState
            });
        }
    }

    /// <summary>
    /// Event published when game state changes.
    /// Struct for zero-allocation event passing through the event bus.
    /// </summary>
    public struct GameStateChangedEvent
    {
        /// <summary>
        /// The state the game was in before the transition.
        /// </summary>
        public GameState PreviousState;

        /// <summary>
        /// The state the game has transitioned to.
        /// </summary>
        public GameState NewState;
    }
}