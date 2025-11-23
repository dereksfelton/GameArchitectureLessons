using Cysharp.Threading.Tasks;
using UnityEngine;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Services;
using R3;
using R3.Unity;
using System;

namespace GameArchitecture.Core.Bootstrap
{
    /// <summary>
    /// Main game manager that orchestrates initialization and game loop.
    /// Uses Reflex for dependency injection and R3 for reactive subscriptions.
    /// Handles game state transitions and optional scene loading.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private bool _autoLoadGameplayScene = false;
        [SerializeField] private string _gameplaySceneName = "Gameplay";

        private IGameStateService _gameStateService;
        private IEventBus _eventBus;
        private ISceneLoader _sceneLoader;

        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// Unity Awake callback. Resolves dependencies from ProjectScope container.
        /// </summary>
        private void Awake()
        {
            // Use the singleton instance instead of FindFirstObjectByType
            var projectScope = ProjectScope.Instance;

            if (projectScope == null)
            {
                Debug.LogError("[GameManager] ProjectScope instance not found!");
                return;
            }

            var container = projectScope.GetContainer();

            if (container == null)
            {
                Debug.LogError("[GameManager] Container is null!");
                return;
            }

            _gameStateService = container.Resolve<IGameStateService>();
            _eventBus = container.Resolve<IEventBus>();
            _sceneLoader = container.Resolve<ISceneLoader>();

            Debug.Log("[GameManager] Dependencies resolved from ProjectScope container.");
        }

        /// <summary>
        /// Unity Start callback. Initializes game systems and optionally loads gameplay scene.
        /// </summary>
        private async void Start()
        {
            // Add null checks for debugging
            if (_gameStateService == null)
            {
                Debug.LogError("[GameManager] IGameStateService resolution failed!");
                return;
            }

            if (_eventBus == null)
            {
                Debug.LogError("[GameManager] IEventBus resolution failed!");
                return;
            }

            Debug.Log("[GameManager] Dependencies validated.");
            await InitializeGameAsync();

            // Optionally load gameplay scene
            if (_autoLoadGameplayScene && _sceneLoader != null)
            {
                Debug.Log($"[GameManager] Loading {_gameplaySceneName} scene...");
                await _sceneLoader.LoadSceneAsync(_gameplaySceneName);
            }
        }

        /// <summary>
        /// Initializes game systems asynchronously.
        /// Subscribes to game state change events and starts the game.
        /// </summary>
        /// <returns>A UniTask representing the async initialization operation.</returns>
        private async UniTask InitializeGameAsync()
        {
            Debug.Log("[GameManager] Initializing game systems...");

            // Subscribe to game state changes
            _eventBus.Observe<GameStateChangedEvent>()
                .Subscribe(OnGameStateChanged)
                .AddTo(_disposables);

            // Initialize game systems
            await _gameStateService.InitializeAsync();

            // Automatically start game for this lesson
            await UniTask.Delay(1000);
            await _gameStateService.StartGameAsync();

            Debug.Log("[GameManager] Game initialized and started.");
        }

        /// <summary>
        /// Handles game state change events.
        /// Adjusts Time.timeScale based on Playing/Paused states.
        /// </summary>
        /// <param name="evt">The state change event containing previous and new states.</param>
        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            Debug.Log($"[GameManager] State changed: {evt.PreviousState} → {evt.NewState}");

            // React to state changes here
            switch (evt.NewState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
            }
        }

        /// <summary>
        /// Unity OnDestroy callback. Cleans up subscriptions and shuts down services.
        /// </summary>
        private void OnDestroy()
        {
            _gameStateService?.Shutdown();
            _disposables?.Dispose();
        }
    }
}