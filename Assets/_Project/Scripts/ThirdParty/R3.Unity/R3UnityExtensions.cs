using System;
using R3;
using UnityEngine;

namespace R3.Unity
{
    /// <summary>
    /// Essential R3 Unity extensions for MonoBehaviour integration.
    /// Provides AddTo() functionality for automatic disposal tied to GameObject lifecycle.
    /// </summary>
    public static class R3UnityExtensions
    {
        /// <summary>
        /// Adds disposable to GameObject's lifecycle.
        /// Automatically disposes when GameObject is destroyed.
        /// </summary>
        /// <param name="disposable">The disposable to add (typically an R3 subscription).</param>
        /// <param name="gameObject">The GameObject whose lifecycle will control disposal.</param>
        /// <returns>The original disposable for method chaining.</returns>
        /// <remarks>
        /// This method adds a R3DisposeTrigger component to the GameObject if one doesn't exist.
        /// The trigger manages disposal when the GameObject is destroyed.
        /// </remarks>
        public static IDisposable AddTo(this IDisposable disposable, GameObject gameObject)
        {
            if (gameObject == null)
            {
                disposable?.Dispose();
                return Disposable.Empty;
            }

            var trigger = gameObject.GetComponent<R3DisposeTrigger>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<R3DisposeTrigger>();
            }

            trigger.Add(disposable);
            return disposable;
        }

        /// <summary>
        /// Adds disposable to Component's GameObject lifecycle.
        /// Automatically disposes when the Component's GameObject is destroyed.
        /// </summary>
        /// <param name="disposable">The disposable to add (typically an R3 subscription).</param>
        /// <param name="component">The Component whose GameObject lifecycle will control disposal.</param>
        /// <returns>The original disposable for method chaining.</returns>
        public static IDisposable AddTo(this IDisposable disposable, Component component)
        {
            if (component == null)
            {
                disposable?.Dispose();
                return Disposable.Empty;
            }

            return disposable.AddTo(component.gameObject);
        }

        /// <summary>
        /// Adds disposable to CompositeDisposable for grouped disposal.
        /// Allows manual control over when multiple disposables are disposed together.
        /// </summary>
        /// <typeparam name="T">The type of disposable.</typeparam>
        /// <param name="disposable">The disposable to add.</param>
        /// <param name="compositeDisposable">The composite disposable to add to.</param>
        /// <returns>The original disposable for method chaining.</returns>
        /// <remarks>
        /// This is useful when you want to dispose multiple subscriptions at once,
        /// such as in a MonoBehaviour's OnDestroy method.
        /// </remarks>
        public static T AddTo<T>(this T disposable, CompositeDisposable compositeDisposable)
            where T : IDisposable
        {
            if (compositeDisposable == null || compositeDisposable.IsDisposed)
            {
                disposable?.Dispose();
                return disposable;
            }

            compositeDisposable.Add(disposable);
            return disposable;
        }
    }

    /// <summary>
    /// MonoBehaviour component that manages disposables lifecycle.
    /// Automatically disposes all registered disposables when destroyed.
    /// </summary>
    /// <remarks>
    /// This component is automatically added by the AddTo(GameObject) extension method.
    /// Do not add this component manually - it's managed internally.
    /// </remarks>
    internal class R3DisposeTrigger : MonoBehaviour
    {
        private CompositeDisposable _disposables = new();

        /// <summary>
        /// Adds a disposable to be managed by this trigger.
        /// The disposable will be disposed when this component is destroyed.
        /// </summary>
        /// <param name="disposable">The disposable to add.</param>
        public void Add(IDisposable disposable)
        {
            if (_disposables == null || _disposables.IsDisposed)
            {
                _disposables = new CompositeDisposable();
            }

            _disposables.Add(disposable);
        }

        /// <summary>
        /// Unity OnDestroy callback. Disposes all registered disposables.
        /// </summary>
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}