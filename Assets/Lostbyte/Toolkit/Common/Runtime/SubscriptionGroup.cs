using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Lostbyte.Toolkit.Common
{
    public sealed class SubscriptionGroup : IDisposable
    {
        private readonly List<Action> _unsubscribers = new();

        // --------------------------------------------------
        // CORE
        // --------------------------------------------------
        public int Count => _unsubscribers.Count;
        public void Add(Action unsubscribe)
        {
            if (unsubscribe != null)
                _unsubscribers.Add(unsubscribe);
        }

        public void Subscribe(Action subscribe, Action unsubscribe)
        {
            subscribe?.Invoke();
            Add(unsubscribe);
        }

        // --------------------------------------------------
        // STANDARD C# EVENTS (via accessors, NOT delegates!)
        // --------------------------------------------------

        public void Subscribe(Action<Action> add, Action<Action> remove, Action handler)
        {
            add(handler);
            Add(() => remove(handler));
        }

        public void Subscribe<T>(Action<Action<T>> add, Action<Action<T>> remove, Action<T> handler)
        {
            add(handler);
            Add(() => remove(handler));
        }

        public void Subscribe<T>(Action<Action<T, T>> add, Action<Action<T, T>> remove, Action<T, T> handler)
        {
            add(handler);
            Add(() => remove(handler));
        }

        // --------------------------------------------------
        // UNITY EVENTS
        // --------------------------------------------------

        public void Subscribe(UnityEvent evt, Action handler)
        {
            UnityAction unityAction = handler.Invoke;
            evt.AddListener(unityAction);
            Add(() => evt.RemoveListener(unityAction));
        }

        public void Subscribe<T>(UnityEvent<T> evt, Action<T> handler)
        {
            UnityAction<T> unityAction = handler.Invoke;
            evt.AddListener(unityAction);
            Add(() => evt.RemoveListener(unityAction));
        }

        public void Subscribe<T>(UnityEvent<T> evt, Action handler)
        {
            void Wrapper(T _) => handler();
            evt.AddListener(Wrapper);
            Add(() => evt.RemoveListener(Wrapper));
        }

        // --------------------------------------------------
        // GENERIC "VALUE" SUBSCRIPTIONS
        // --------------------------------------------------

        public void SubscribeValue<T>(
            Action<T> subscribe,
            Action<T> unsubscribe,
            T value)
        {
            subscribe(value);
            Add(() => unsubscribe(value));
        }

        public void SubscribeCallback<T>(
            Action<Action<T>> subscribe,
            Action<Action<T>> unsubscribe,
            Action<T> handler)
        {
            subscribe(handler);
            Add(() => unsubscribe(handler));
        }

        public void SubscribeCallback<T>(
            Action<Action<T, T>> subscribe,
            Action<Action<T, T>> unsubscribe,
            Action<T, T> handler)
        {
            subscribe(handler);
            Add(() => unsubscribe(handler));
        }

        // --------------------------------------------------
        // KEYED SUBSCRIPTIONS
        // --------------------------------------------------

        public void Subscribe<K>(
            Action<K, Action> subscribe,
            Action<K, Action> unsubscribe,
            K key,
            Action handler)
        {
            subscribe(key, handler);
            Add(() => unsubscribe(key, handler));
        }

        public void Subscribe<K, T>(
            Action<K, Action<T>> subscribe,
            Action<K, Action<T>> unsubscribe,
            K key,
            Action<T> handler)
        {
            subscribe(key, handler);
            Add(() => unsubscribe(key, handler));
        }

        public void Subscribe<K, T>(
            Action<K, Action<T, T>> subscribe,
            Action<K, Action<T, T>> unsubscribe,
            K key,
            Action<T, T> handler)
        {
            subscribe(key, handler);
            Add(() => unsubscribe(key, handler));
        }

        // --------------------------------------------------
        // CLEANUP
        // --------------------------------------------------

        public void Dispose()
        {
            for (int i = _unsubscribers.Count - 1; i >= 0; i--)
            {
                try
                {
                    _unsubscribers[i]?.Invoke();
                }
                catch (Exception e)
                {
                    DebugLogger.LogError(e);
                }
            }

            _unsubscribers.Clear();
        }
    }
}
