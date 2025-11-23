using R3;

namespace GameArchitecture.Core.Interfaces
{
    /// <summary>
    /// Decoupled event communication system using R3 reactive extensions.
    /// Allows systems to publish and subscribe to events without direct references.
    /// Enables loose coupling between game systems for better testability and maintainability.
    /// </summary>
    /// <remarks>
    /// Events should be defined as structs to ensure zero-allocation publishing.
    /// The event bus is thread-safe and can be used from any thread.
    /// Always dispose subscriptions when no longer needed to prevent memory leaks.
    /// </remarks>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an event to all subscribers.
        /// Events are delivered synchronously to all current subscribers.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish. Must be a struct.</typeparam>
        /// <param name="eventData">The event data to publish to all subscribers.</param>
        /// <remarks>
        /// If there are no subscribers, the event is safely ignored.
        /// Events are delivered in the order subscriptions were created.
        /// </remarks>
        void Publish<TEvent>(TEvent eventData) where TEvent : struct;

        /// <summary>
        /// Subscribes to events of type TEvent.
        /// Returns a disposable subscription that should be disposed when no longer needed.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to observe. Must be a struct.</typeparam>
        /// <returns>
        /// An observable sequence of events that can be subscribed to using R3's Subscribe method.
        /// </returns>
        /// <remarks>
        /// Use the AddTo() extension method to tie disposal to GameObject lifecycle.
        /// Multiple subscribers can observe the same event type independently.
        /// Subscriptions receive events published after subscription is created.
        /// </remarks>
        /// <example>
        /// eventBus.Observe&lt;GameStateChangedEvent&gt;()
        ///     .Subscribe(evt => Debug.Log($"State: {evt.NewState}"))
        ///     .AddTo(this);
        /// </example>
        Observable<TEvent> Observe<TEvent>() where TEvent : struct;
    }
}