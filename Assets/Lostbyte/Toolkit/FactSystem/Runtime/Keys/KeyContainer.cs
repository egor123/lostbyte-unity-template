using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public class KeyContainer : ScriptableObject, IKeyContainer
    {
        [field: SerializeField] public string Guid { get; internal set; }
        public string Name => name;
        public KeyContainer Key => this;
        [field: SerializeField] public bool IsSerializable { get; internal set; }
        [field: SerializeField, TextArea] public string Description { get; internal set; }
        [SerializeField] private List<KeyContainer> m_children = new();
        private List<KeyContainer> _children;
        public List<KeyContainer> Children
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying ? _children ??= m_children.ToList() : m_children;
#else
                return _children ??= m_children.ToList();
#endif
            }
        }
        [field: SerializeField] internal List<FactDefinition> Facts { get; private set; } = new();
        public IReadOnlyCollection<FactDefinition> DefinedFacts
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying ? _factStorage.Keys : Facts;
#else
                return _factStorage.Keys;
#endif
            }
        }
        [field: SerializeField] internal List<EventDefinition> Events { get; private set; } = new();
        public IReadOnlyCollection<EventDefinition> DefinedEvents
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying ? _eventStorage.Keys : Events;
#else
                return _eventStorage.Keys;
#endif
            }
        }
        [field: SerializeField] internal List<FactSerializationOverride> SerializationOverrides { get; private set; } = new();
        [field: SerializeField] internal List<FactValueOverride> ValueOverrides { get; private set; } = new();

        private Action<FactDefinition> OnFactAdded;
        private Action OnChange;
        private readonly Dictionary<FactDefinition, IFactWrapper> _factStorage = new();
        private readonly Dictionary<EventDefinition, IEventWrapper> _eventStorage = new();

        private object ApplyValueOverride(FactDefinition fact, object defaultValue)
        {
            foreach (var v in ValueOverrides)
                if (v.Fact == fact)
                    return v.Wrapper.RawValue;
            return defaultValue;
        }

        public void Load(object file = null)
        {
            Dictionary<string, object> dict = file as Dictionary<string, object> ?? new();
            _children = null;
            foreach (var key in Children)
            {
                key.Load(dict.TryGetValue(key.Guid, out var f) ? f : null);
            }
            // _factStorage.Clear();
            foreach (var fact in Facts)
            {
                var wrapper = _factStorage.TryGetValue(fact, out var w) ? w : fact.GetValueWrapper();
                wrapper.RawValue = dict.TryGetValue(fact.Guid, out var v) ? v : ApplyValueOverride(fact, fact.DefaultValueRaw);
                _factStorage[fact] = wrapper;
                wrapper.Subscribe(RaiseChange);
            }
            // _eventStorage.Clear();
            foreach (var @event in Events)
            {
                var wrapper = _eventStorage.TryGetValue(@event, out var w) ? w : new EventValueWrapper();
                _eventStorage[@event] = wrapper;
            }
        }
        public object Save()
        {
            Dictionary<string, object> dict = new();
            foreach (var key in Children)
            {
                if (key.IsSerializable)
                {
                    var data = key.Save();
                    if (data != null)
                    {
                        dict[key.Guid] = data;
                    }
                }
            }
            foreach ((var fact, var wrapper) in _factStorage)
            {
                if (TryGetSerializationOverride(fact, out var so) ? so.IsSerializable : fact.IsSerializable)
                {
                    if (TryGetValueOverride(fact, out var v))
                    {
                        if (!v.Wrapper?.RawValue?.Equals(wrapper.RawValue) ?? true)
                        {
                            dict[fact.Guid] = wrapper.RawValue;
                        }
                    }
                    else if (!fact.DefaultValueRaw.Equals(wrapper.RawValue))
                    {
                        dict[fact.Guid] = wrapper.RawValue;
                    }
                }
            }
            return dict.Count > 0 ? dict : null;
        }
        private bool TryGetSerializationOverride(FactDefinition fact, out FactSerializationOverride serializationOverride)
        {
            foreach (var v in SerializationOverrides)
            {
                if (v.Fact == fact)
                {
                    serializationOverride = v;
                    return true;
                }
            }
            serializationOverride = default;
            return false;
        }
        private bool TryGetValueOverride(FactDefinition fact, out FactValueOverride valueOverride)
        {
            foreach (var v in ValueOverrides)
            {
                if (v.Fact == fact)
                {
                    valueOverride = v;
                    return true;
                }
            }
            valueOverride = default;
            return false;
        }
        public void SetValue<T>(FactDefinition<T> fact, T value)
        {
            var wrapper = GetWrapper(fact);
            if (wrapper.Value.Equals(value)) return;
            wrapper.Value = value;
        }
        public T GetValue<T>(FactDefinition<T> fact) => GetWrapper(fact).Value;
        public void Raise(EventDefinition @event) => GetWrapper(@event).Raise();
        public void AddOnFactAddedListener(Action<FactDefinition> callback) => OnFactAdded += callback;
        public void RemoveOnFactAddedListener(Action<FactDefinition> callback) => OnFactAdded += callback;
        public void AddOnChangeListener(Action callback) => OnChange += callback;
        public void RemoveOnChangeListener(Action callback) => OnChange -= callback;
        public void Subscribe(FactDefinition fact, Action<object> callback) => GetWrapper(fact).Subscribe(callback);
        public void Unsubscribe(FactDefinition fact, Action<object> callback) => GetWrapper(fact).Unsubscribe(callback);
        public void Subscribe(FactDefinition fact, Action callback) => GetWrapper(fact).Subscribe(callback);
        public void Unsubscribe(FactDefinition fact, Action callback) => GetWrapper(fact).Unsubscribe(callback);
        public void Subscribe<T>(FactDefinition<T> fact, Action callback) => GetWrapper(fact).Subscribe(callback);
        public void Unsubscribe<T>(FactDefinition<T> fact, Action callback) => GetWrapper(fact).Unsubscribe(callback);
        public void Subscribe<T>(FactDefinition<T> fact, Action<T> callback) => GetWrapper(fact).Subscribe(callback);
        public void Unsubscribe<T>(FactDefinition<T> fact, Action<T> callback) => GetWrapper(fact).Unsubscribe(callback);
        public void Subscribe<T>(FactDefinition<T> fact, Action<T, T> callback) => GetWrapper(fact).Subscribe(callback);
        public void Unsubscribe<T>(FactDefinition<T> fact, Action<T, T> callback) => GetWrapper(fact).Unsubscribe(callback);
        public void Subscribe(EventDefinition @event, Action callback) => GetWrapper(@event).Subscribe(callback);
        public void Unsubscribe(EventDefinition @event, Action callback) => GetWrapper(@event).Unsubscribe(callback);
        private void RaiseChange() => OnChange?.Invoke();
        public IFactWrapper<T> GetWrapper<T>(FactDefinition<T> fact)
        {
            if (_factStorage.TryGetValue(fact, out var wrapperRaw) == false || wrapperRaw is not IFactWrapper<T> wrapper)
            {
                wrapper = (IFactWrapper<T>)fact.GetValueWrapper();
                _factStorage[fact] = wrapper;
                wrapper.Subscribe(RaiseChange);
                OnFactAdded?.Invoke(fact);
            }
            return wrapper;
        }
        public IFactWrapper GetWrapper(FactDefinition fact)
        {
            if (_factStorage.TryGetValue(fact, out var wrapperRaw) == false || wrapperRaw is not IFactWrapper wrapper)
            {
                wrapper = fact.GetValueWrapper();
                _factStorage[fact] = wrapper;
                wrapper.Subscribe(RaiseChange);
                OnFactAdded?.Invoke(fact);
            }
            return wrapper;
        }
        public IEventWrapper GetWrapper(EventDefinition @event)
        {
            if (_eventStorage.TryGetValue(@event, out var wrapperRaw) == false || wrapperRaw is not IEventWrapper wrapper)
            {
                wrapper = @event.GetValueWrapper();
                _eventStorage[@event] = wrapper;
            }
            return wrapper;
        }
        public IWrapper GetWrapper(Definition def)
        {
            if (def is EventDefinition @event)
            {
                if (_eventStorage.TryGetValue(@event, out var wrapperRaw) == false || wrapperRaw is not IEventWrapper wrapper)
                {
                    wrapper = @event.GetValueWrapper();
                    _eventStorage[@event] = wrapper;
                }
                return wrapper;
            }
            if (def is FactDefinition fact)
            {
                if (_factStorage.TryGetValue(fact, out var wrapperRaw) == false || wrapperRaw is not IFactWrapper wrapper)
                {
                    wrapper = fact.GetValueWrapper();
                    _factStorage[fact] = wrapper;
                    wrapper.Subscribe(RaiseChange);
                    OnFactAdded?.Invoke(fact);
                }
                return wrapper;
            }
            Debug.LogError("Unknown defenition!");
            return null;
        }
        public KeyContainer RequestTempKey(string name, List<FactValueOverride> overrides = null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogError("Requesting temp key is only allowed at runtime!");
                return null;
            }
#endif
            var key = CreateInstance<KeyContainer>();
            key.name = FactUtils.GenerateValidName(name + "_temp");
            key.IsSerializable = false;
            Children.Add(key);
            overrides?.ForEach(o =>
            {
                if (o.Fact != null && o.Wrapper != null)
                {
                    key.ValueOverrides.Add(o.Copy());
                    if (!key.Facts.Contains(o.Fact))
                        key.Facts.Add(o.Fact);
                }
            });
            key.Load();
            return key;
        }
    }
}