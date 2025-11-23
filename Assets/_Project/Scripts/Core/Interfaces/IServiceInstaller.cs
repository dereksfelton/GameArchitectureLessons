using Reflex.Core;

namespace GameArchitecture.Core.Interfaces
{
    /// <summary>
    /// Interface for service installers.
    /// Allows modular registration of services in the DI container.
    /// Installers can be ScriptableObjects or MonoBehaviours that implement this interface.
    /// </summary>
    public interface IServiceInstaller
    {
        /// <summary>
        /// Install services into the container.
        /// </summary>
        /// <param name="builder">The container builder to register services with.</param>
        /// <param name="eventBus">The global event bus for services that need it.</param>
        void Install(ContainerBuilder builder, IEventBus eventBus);
    }
}