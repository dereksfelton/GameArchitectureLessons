using Cysharp.Threading.Tasks;
using UnityEngine;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Services;
using R3;
using R3.Unity;

namespace GameArchitecture.Core.Bootstrap
{
    /// <summary>
    /// Main game manager that orchestrates initialization and game loop.
    /// Uses Reflex for dependency injection and R3 for reactive subscriptions.
    /// Handles game state transitions, scene loading, and input initialization.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private bool autoLoadGameplayScene = true;
        [SerializeField] private string gameplaySceneName = "Gameplay";

        private IGameStateService _gameStateService;
        private IEventBus _eventBus;
        private ISceneLoader _sceneLoader;
        private IInputService _inputService;

        private readonly CompositeDisposable _disposables = new();

        private void Awake()
        {
            var projectScope = ProjectScope.Instance;

            if (projectScope == null)
            {
                Debug.LogError("[GameManager] ProjectScope not found!");
                return;
            }

            var container = projectScope.GetContainer();
            _gameStateService = container.Resolve<IGameStateService>();
            _eventBus = container.Resolve<IEventBus>();
            _sceneLoader = container.Resolve<ISceneLoader>();
            _inputService = container.Resolve<IInputService>();

            Debug.Log("[GameManager] Dependencies resolved.");
        }

        private async void Start()
        {
            if (_gameStateService == null || _eventBus == null)
            {
                Debug.LogError("[GameManager] Dependency resolution failed!");
                return;
            }

            // Subscribe to game state changes - FIXED: Use Observe().Subscribe()
            _eventBus.Observe<GameStateChangedEvent>()
                .Subscribe(OnGameStateChanged)
                .AddTo(_disposables);

            Debug.Log("[GameManager] Initialization started.");

            // Initialize game state service
            await _gameStateService.InitializeAsync();

            // Enable input
            _inputService.Enable();
            Debug.Log("[GameManager] Input enabled.");

            // Auto-load gameplay scene if configured
            if (autoLoadGameplayScene && !string.IsNullOrEmpty(gameplaySceneName))
            {
                Debug.Log($"[GameManager] Loading scene: {gameplaySceneName}");
                await _sceneLoader.LoadSceneAsync(gameplaySceneName);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            // FIXED: Use PreviousState and NewState (not OldState)
            Debug.Log($"[GameManager] Game state changed: {evt.PreviousState} -> {evt.NewState}");
            
            // React to state changes
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

        private void OnDestroy()
        {
            _disposables.Dispose();
            Debug.Log("[GameManager] Disposed subscriptions.");
        }
    }
}