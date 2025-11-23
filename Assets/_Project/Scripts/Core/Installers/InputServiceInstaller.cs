using UnityEngine;
using UnityEngine.InputSystem;
using Reflex.Core;
using GameArchitecture.Core.Interfaces;
using GameArchitecture.Core.Services;

namespace GameArchitecture.Core.Installers
{
    /// <summary>
    /// ScriptableObject installer for InputService.
    /// Decouples input system configuration from ProjectScope.
    /// This allows ProjectScope to remain pure and not depend on Unity's Input System.
    /// </summary>
    [CreateAssetMenu(
        fileName = "InputServiceInstaller", 
        menuName = "Game Architecture/Installers/Input Service Installer",
        order = 0)]
    public class InputServiceInstaller : ScriptableObject, IServiceInstaller
    {
        [Header("Input Configuration")]
        [SerializeField] 
        [Tooltip("Drag the GameInputActions.inputactions asset here")]
        private InputActionAsset inputActions;

        /// <summary>
        /// Installs InputService into the DI container.
        /// Called by ProjectScope during initialization.
        /// </summary>
        /// <param name="builder">The container builder to register services with.</param>
        /// <param name="eventBus">The event bus to pass to InputService.</param>
        public void Install(ContainerBuilder builder, IEventBus eventBus)
        {
            if (inputActions == null)
            {
                Debug.LogError("[InputServiceInstaller] InputActionAsset not assigned! Input will not work.");
                return;
            }

            // Create and register InputService with factory functions
            var inputService = new InputService(eventBus, inputActions);
            builder.AddSingleton(container => inputService);
            builder.AddSingleton<IInputService>(container => inputService);

            Debug.Log("[InputServiceInstaller] InputService registered successfully.");
        }

        private void OnValidate()
        {
            // Editor-time validation
            if (inputActions == null)
            {
                Debug.LogWarning($"[InputServiceInstaller] '{name}' has no InputActionAsset assigned.");
            }
        }
    }
}