using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public static class SubscriptionGroupExtentions
    {
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, FactDefinition<T> fact, Action action)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe(this SubscriptionGroup goup, IKeyContainer key, FactDefinition fact, Action<object> action)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, FactDefinition<T> fact, Action<T> action, bool invokeImidiate = false)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
            if (invokeImidiate) action.Invoke(key.GetValue(fact));
        }
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, FactDefinition<T> fact, Action<T, T> action, bool invokeImidiate = false)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
            if (invokeImidiate) action.Invoke(key.GetValue(fact), key.GetValue(fact));
        }
        // ------------------
        public static void Subscribe<T>(this SubscriptionGroup goup, IFactWrapper<T> wrapper, Action<T> action, bool invokeImidiate = false)
        {
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
            if (invokeImidiate) action.Invoke(wrapper.Value);
        }
        public static void Subscribe<T>(this SubscriptionGroup goup, IFactWrapper<T> wrapper, Action<T, T> action, bool invokeImidiate = false)
        {
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
            if (invokeImidiate) action.Invoke(wrapper.Value, wrapper.Value);
        }
        // ------------------
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, EventDefinition @event, Action action, bool invokeImidiate = false)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, @event, action);
            if (invokeImidiate) action.Invoke();
        }
        // ------------------
        public static void Subscribe<T>(this SubscriptionGroup goup, IEventWrapper wrapper, Action action, bool invokeImidiate = false)
        {
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
            if (invokeImidiate) action.Invoke();
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup goup, IKeyContainer key, Action onChangeAction)
        {
            goup.Subscribe(key.AddOnChangeListener, key.RemoveOnChangeListener, onChangeAction);
        }
        public static void Subscribe(this SubscriptionGroup goup, IKeyContainer key, Action<FactDefinition> onFactAddedAction)
        {
            goup.Subscribe(key.AddOnFactAddedListener, key.RemoveOnFactAddedListener, onFactAddedAction);
        }
    }
}
