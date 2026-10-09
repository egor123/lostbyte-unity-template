using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem.Persistance;

namespace Lostbyte.Toolkit.FactSystem
{
    public static class SubscriptionGroupFactExtentions
    {
        public static void Subscribe<T>(this SubscriptionGroup group, IKeyContainer key, FactDefinition<T> fact, Action action)
        {
            group.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe(this SubscriptionGroup group, IKeyContainer key, FactDefinition fact, Action<object> action)
        {
            group.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe(this SubscriptionGroup group, IKeyContainer key, FactDefinition fact, Action action)
        {
            group.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup group, IKeyContainer key, FactDefinition<T> fact, Action<T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(key.GetValue(fact));
            group.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup group, IKeyContainer key, FactDefinition<T> fact, Action<T, T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(key.GetValue(fact), key.GetValue(fact));
            group.Subscribe(key.Subscribe, key.Unsubscribe, fact, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup group, IKeyContainer key, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            group.SubscribeValue(key.AddOnChangeListener, key.RemoveOnChangeListener, action);
        }
        public static void Subscribe(this SubscriptionGroup group, IKeyContainer key, IPersistent persistent)
        {
            group.SubscribeValue(key.Subscribe, key.Unsubscribe, persistent);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup group, IFactWrapper wrapper, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            group.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup group, IFactWrapper wrapper, Action<object> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(wrapper.RawValue);
            group.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup group, IFactWrapper<T> wrapper, Action<T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(wrapper.Value);
            group.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        public static void Subscribe<T>(this SubscriptionGroup group, IFactWrapper<T> wrapper, Action<T, T> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(wrapper.Value, wrapper.Value);
            group.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup group, IKeyContainer key, EventDefinition @event, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            group.Subscribe(key.Subscribe, key.Unsubscribe, @event, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup group, IEventWrapper wrapper, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            group.Subscribe(wrapper.Subscribe, wrapper.Unsubscribe, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup group, Condition condition, IKeyContainer defaultKey, Action action)
        {
            condition.SetDefaultKey(defaultKey);
            group.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup group, Condition condition, IKeyContainer defaultKey, Action<bool> action)
        {
            condition.SetDefaultKey(defaultKey);
            group.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup group, Condition condition, Action action)
        {
            condition.SetDefaultKey(null);
            group.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
        }
        public static void Subscribe(this SubscriptionGroup group, Condition condition, Action<bool> action)
        {
            condition.SetDefaultKey(null);
            group.Subscribe(condition.Subscribe, condition.Unsubscribe, action);
        }
        // ------------------
        public static void Subscribe(this SubscriptionGroup group, IKeyContainer key, Action onChangeAction)
        {
            group.Subscribe(key.AddOnChangeListener, key.RemoveOnChangeListener, onChangeAction);
        }
        public static void Subscribe(this SubscriptionGroup group, IKeyContainer key, Action<FactDefinition> onFactAddedAction)
        {
            group.Subscribe(key.AddOnFactAddedListener, key.RemoveOnFactAddedListener, onFactAddedAction);
        }
        // ------------------
    }
}
