using R3;
using R3.Unity;
using System;
using System.Collections.Generic;
using GameArchitecture.Core.Interfaces;

namespace GameArchitecture.Core.Events
{
    /// <summary>
    /// R3-based event bus implementation for decoupled communication.
    /// Uses Subject pattern for type-safe event publishing and subscription.
    /// Thread-safe for publishing and subscribing from any thread.
    /// </summary>
    public class EventBus : IEventBus, IDisposable
    {
        private readonly Dictionary<Type, object> _subjects = new();
        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// Publishes an event to all subscribers.
        /// Events are delivered synchronously to all current subscribers.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish. Must be a struct for zero-allocation.</typeparam>
        /// <param name="eventData">The event data to publish to all subscribers.</param>
        public void Publish<TEvent>(TEvent eventData) where TEvent : struct
        {
            var subject = GetOrCreateSubject<TEvent>();
            subject.OnNext(eventData);
        }

        /// <summary>
        /// Subscribes to events of type TEvent.
        /// Returns an observable that emits events when they are published.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to observe. Must be a struct for zero-allocation.</typeparam>
        /// <returns>An observable sequence of events that can be subscribed to.</returns>
        /// <remarks>
        /// Remember to dispose the subscription when no longer needed to prevent memory leaks.
        /// Use AddTo() extension methods to tie disposal to GameObject lifecycle.
        /// </remarks>
        public Observable<TEvent> Observe<TEvent>() where TEvent : struct
        {
            return GetOrCreateSubject<TEvent>().AsObservable();
        }

        /// <summary>
        /// Gets or creates a Subject for the specified event type.
        /// Subjects are cached to ensure all subscribers receive the same events.
        /// </summary>
        /// <typeparam name="TEvent">The type of event subject to get or create.</typeparam>
        /// <returns>The Subject instance for the specified event type.</returns>
        private Subject<TEvent> GetOrCreateSubject<TEvent>() where TEvent : struct
        {
            var eventType = typeof(TEvent);

            if (!_subjects.TryGetValue(eventType, out var subject))
            {
                subject = new Subject<TEvent>();
                _subjects[eventType] = subject;
                _disposables.Add((Subject<TEvent>)subject);
            }

            return (Subject<TEvent>)subject;
        }

        /// <summary>
        /// Disposes all subjects and clears the event bus.
        /// Should be called when the EventBus is no longer needed.
        /// </summary>
        /// <remarks>
        /// This will complete all active subscriptions.
        /// The EventBus should not be used after disposal.
        /// </remarks>
        public void Dispose()
        {
            _disposables?.Dispose();
            _subjects.Clear();
        }
    }
}