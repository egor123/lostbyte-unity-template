using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Lostbyte.Toolkit.Common
{
    public class SubscriptionGroup : IDisposable
    {
        private readonly List<Action> _unsubscribers = new();

        public void Subscribe(Action @event, Action action)
        {
            @event += action;
            _unsubscribers.Add(() => @event -= action);
        }
        public void Subscribe<T>(Action<T> @event, Action<T> action)
        {
            @event += action;
            _unsubscribers.Add(() => @event -= action);
        }
        public void Subscribe<T>(Action<T, T> @event, Action<T, T> action)
        {
            @event += action;
            _unsubscribers.Add(() => @event -= action);
        }


        public void Subscribe(UnityEvent @event, Action action)
        {
            UnityAction unityAction = new(action);
            @event.AddListener(unityAction);
            _unsubscribers.Add(() => @event.RemoveListener(unityAction));
        }
        public void Subscribe<T>(UnityEvent<T> @event, Action<T> action)
        {
            UnityAction<T> unityAction = new(action);
            @event.AddListener(unityAction);
            _unsubscribers.Add(() => @event.RemoveListener(unityAction));
        }
        public void Subscribe<T>(UnityEvent<T> @event, Action action)
        {
            void unityAction(T v) => action.Invoke();
            @event.AddListener(unityAction);
            _unsubscribers.Add(() => @event.RemoveListener(unityAction));
        }

        public delegate void SubscriptionHandler(Action action);
        public delegate void UnsubscriptionHandler(Action action);
        public void Subscribe(SubscriptionHandler subscribe, SubscriptionHandler unsubscribe, Action action)
        {
            subscribe.Invoke(action);
            _unsubscribers.Add(() => unsubscribe.Invoke(action));
        }
        public delegate void SubscriptionHandler<Value>(Action<Value> action);
        public delegate void UnsubscriptionHandler<Value>(Action<Value> action);
        public void Subscribe<T>(SubscriptionHandler<T> subscribe, UnsubscriptionHandler<T> unsubscribe, Action<T> action)
        {
            subscribe.Invoke(action);
            _unsubscribers.Add(() => unsubscribe.Invoke(action));
        }
        public delegate void OnChangeSubscriptionHandler<Value>(Action<Value, Value> action);
        public delegate void OnChangeUnsubscriptionHandler<Value>(Action<Value, Value> action);
        public void Subscribe<T>(OnChangeSubscriptionHandler<T> subscribe, OnChangeUnsubscriptionHandler<T> unsubscribe, Action<T, T> action)
        {
            subscribe.Invoke(action);
            _unsubscribers.Add(() => unsubscribe.Invoke(action));
        }
        public delegate void KeySubscriptionHandler<Key>(Key key, Action action);
        public delegate void KeyUnsubscriptionHandler<Key>(Key key, Action action);
        public void Subscribe<K>(KeySubscriptionHandler<K> subscribe, KeyUnsubscriptionHandler<K> unsubscribe, K key, Action action)
        {
            subscribe.Invoke(key, action);
            _unsubscribers.Add(() => unsubscribe.Invoke(key, action));
        }
        public delegate void KeySubscriptionHandler<Key, Value>(Key key, Action<Value> action);
        public delegate void KeyUnsubscriptionHandler<Key, Value>(Key key, Action<Value> action);
        public void Subscribe<K, T>(KeySubscriptionHandler<K, T> subscribe, KeyUnsubscriptionHandler<K, T> unsubscribe, K key, Action<T> action)
        {
            subscribe.Invoke(key, action);
            _unsubscribers.Add(() => unsubscribe.Invoke(key, action));
        }
        public delegate void KeyOnChangeSubscriptionHandler<Key, Value>(Key key, Action<Value, Value> action);
        public delegate void KeyOnChangeUnsubscriptionHandler<Key, Value>(Key key, Action<Value, Value> action);
        public void Subscribe<K, T>(KeyOnChangeSubscriptionHandler<K, T> subscribe, KeyOnChangeUnsubscriptionHandler<K, T> unsubscribe, K key, Action<T, T> action)
        {
            subscribe.Invoke(key, action);
            _unsubscribers.Add(() => unsubscribe.Invoke(key, action));
        }
        public void Dispose()
        {
            foreach (var unsubscribe in _unsubscribers)
            {
                try
                {
                    unsubscribe?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            _unsubscribers.Clear();
        }
    }
}
