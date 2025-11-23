using System;

namespace GameArchitecture.Core.Interfaces
{
    /// <summary>
    /// Interface for the input service that translates Input System events to game events.
    /// </summary>
    public interface IInputService : IDisposable
    {
        /// <summary>
        /// Enables input processing.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables input processing.
        /// </summary>
        void Disable();

        /// <summary>
        /// Checks if input is currently enabled.
        /// </summary>
        bool IsEnabled { get; }
    }
}