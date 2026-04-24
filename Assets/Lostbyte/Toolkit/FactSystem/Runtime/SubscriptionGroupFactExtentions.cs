using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem.Persistance;

namespace Lostbyte.Toolkit.FactSystem
{
    public static class SubscriptionGroupFactExtentions
    {
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, FactDefinition<T> fact, Action action)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe(this SubscriptionGroup goup, IKeyContainer key, FactDefinition fact, Action<object> action)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe(this SubscriptionGroup goup, IKeyContainer key, FactDefinition fact, Action action)
        {
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, FactDefinition<T> fact, Action<T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(key.GetValue(fact));
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, FactDefinition<T> fact, Action<T, T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(key.GetValue(fact), key.GetValue(fact));
            goup.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup goup, IKeyContainer key, IPersistent persistent)
        {
            goup.SubscribeValue(key.Subscribe, key.Unsubscribe, persistent);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup goup, IFactWrapper wrapper, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup goup, IFactWrapper wrapper, Action<object> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(wrapper.RawValue);
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup goup, IFactWrapper<T> wrapper, Action<T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(wrapper.Value);
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup goup, IFactWrapper<T> wrapper, Action<T, T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(wrapper.Value, wrapper.Value);
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        // ------------------
        public static void Subscribe<T>(this SubscriptionGroup goup, IKeyContainer key, EventDefinition @event, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            goup.Subscribe(key.Subscribe, key.Unsubscribe, @event, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup goup, IEventWrapper wrapper, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            goup.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup goup, Condition condition, IKeyContainer defaultKey, Action action)
        {
            condition.SetDefaultKey(defaultKey);
            goup.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup goup, Condition condition, IKeyContainer defaultKey, Action<bool> action)
        {
            condition.SetDefaultKey(defaultKey);
            goup.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup goup, Condition condition, Action action)
        {
            condition.SetDefaultKey(null);
            goup.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup goup, Condition condition, Action<bool> action)
        {
            condition.SetDefaultKey(null);
            goup.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
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
        // ------------------
    }
}
