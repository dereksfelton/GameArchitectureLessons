using System;
using R3;
using UnityEngine;

namespace R3.Unity
{
    /// <summary>
    /// Unity-specific Observable factory methods.
    /// Provides frame-based and time-based observable sequences for Unity game loops.
    /// </summary>
    public static class ObservableUnityExtensions
    {
        /// <summary>
        /// Creates an observable that fires every frame.
        /// Emits a long value representing the frame count starting from 0.
        /// </summary>
        /// <returns>An observable sequence that emits on every Unity Update frame.</returns>
        /// <remarks>
        /// This creates a persistent GameObject that survives scene loads (DontDestroyOnLoad).
        /// The GameObject is automatically destroyed when the subscription is disposed.
        /// Useful for frame-based animations, input polling, or continuous monitoring.
        /// </remarks>
        /// <example>
        /// ObservableUnityExtensions.EveryUpdate()
        ///     .Subscribe(frame => Debug.Log($"Frame: {frame}"))
        ///     .AddTo(this);
        /// </example>
        public static Observable<long> EveryUpdate()
        {
            return Observable.Create<long>(observer =>
            {
                var frameCount = 0L;
                var go = new GameObject("R3_EveryUpdate");
                GameObject.DontDestroyOnLoad(go);

                var updater = go.AddComponent<FrameUpdater>();
                updater.OnUpdateAction = () =>
                {
                    observer.OnNext(frameCount++);
                };

                return Disposable.Create(() =>
                {
                    if (go != null)
                    {
                        GameObject.Destroy(go);
                    }
                });
            });
        }

        /// <summary>
        /// Creates an observable that fires at specified time intervals.
        /// Emits a long value representing the tick count starting from 0.
        /// </summary>
        /// <param name="period">The time period between emissions.</param>
        /// <returns>An observable sequence that emits at regular intervals.</returns>
        /// <remarks>
        /// This uses Unity's Time.deltaTime for interval calculation.
        /// The interval is not guaranteed to be exact due to frame rate variations.
        /// The GameObject is automatically destroyed when the subscription is disposed.
        /// Useful for timed events, periodic checks, or time-based game logic.
        /// </remarks>
        /// <example>
        /// ObservableUnityExtensions.Interval(TimeSpan.FromSeconds(1))
        ///     .Subscribe(tick => Debug.Log($"Second: {tick}"))
        ///     .AddTo(this);
        /// </example>
        public static Observable<long> Interval(TimeSpan period)
        {
            return Observable.Create<long>(observer =>
            {
                var count = 0L;
                var elapsed = 0f;
                var periodSeconds = (float)period.TotalSeconds;

                var go = new GameObject("R3_Interval");
                GameObject.DontDestroyOnLoad(go);

                var updater = go.AddComponent<FrameUpdater>();
                updater.OnUpdateAction = () =>
                {
                    elapsed += Time.deltaTime;
                    if (elapsed >= periodSeconds)
                    {
                        elapsed -= periodSeconds;
                        observer.OnNext(count++);
                    }
                };

                return Disposable.Create(() =>
                {
                    if (go != null)
                    {
                        GameObject.Destroy(go);
                    }
                });
            });
        }

        /// <summary>
        /// Internal MonoBehaviour that provides Update loop for observables.
        /// Used by EveryUpdate and Interval methods.
        /// </summary>
        private class FrameUpdater : MonoBehaviour
        {
            /// <summary>
            /// Action to invoke on every Unity Update frame.
            /// Set by the observable creation methods.
            /// </summary>
            public Action OnUpdateAction;

            /// <summary>
            /// Unity Update callback. Invokes the OnUpdateAction if set.
            /// </summary>
            private void Update()
            {
                OnUpdateAction?.Invoke();
            }
        }
    }
}