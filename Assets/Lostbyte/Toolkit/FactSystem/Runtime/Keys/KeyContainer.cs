using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem.Persistance;
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
        [SerializeField, ShowIf(nameof(IsSerializable))] private SaveSystem m_save;
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

        [field: SerializeField] internal List<FactRegistration> FactRegistrations { get; private set; } = new();
        public IReadOnlyCollection<FactDefinition> DefinedFacts
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying ? _factStorage.Keys : FactRegistrations.Select(r => r.Fact).ToList();
#else
                return _factStorage.Keys;
#endif
            }
        }
        [field: SerializeField] internal List<EventRegistration> EventRegistrations { get; private set; } = new();
        public IReadOnlyCollection<EventDefinition> DefinedEvents
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying ? _eventStorage.Keys : EventRegistrations.Select(r => r.Event).ToList();
#else
                return _eventStorage.Keys;
#endif
            }
        }

        private event Action<FactDefinition> OnFactAdded;
        private event Action OnChange;
        private readonly Dictionary<FactDefinition, IFactWrapper> _factStorage = new();
        private readonly Dictionary<EventDefinition, IEventWrapper> _eventStorage = new();

        internal void ClearStorages()
        {
            foreach (var reg in FactRegistrations)
                foreach (var reaction in reg.Reactions)
                    reaction?.Dispose();

            foreach (var reg in EventRegistrations)
                foreach (var reaction in reg.Reactions)
                    reaction?.Dispose();

            _factStorage.Clear();
            _eventStorage.Clear();
            m_children.ForEach(k => k.ClearStorages());
        }

        private bool UseSaveSystem => IsSerializable && m_save.Enabled;

        public void Clear()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            Load();
        }

        private readonly Store _store = new();

        public void Load(object file = null, bool forceReadFile = false)
        {
            if (UseSaveSystem && (forceReadFile || m_save.AutoLoad)) _store.SetStore(m_save.Read<Dictionary<string, object>>());
            else _store.SetStore(file as Dictionary<string, object> ?? new());

            _children = null;

            foreach (var reg in FactRegistrations)
            {
                if (reg.Fact == null) continue;
                foreach (var reaction in reg.Reactions)
                    reaction?.Dispose();

                if (_factStorage.TryGetValue(reg.Fact, out var wrapper) == false)
                {
                    wrapper = reg.Fact.GetValueWrapper();
                    wrapper.Subscribe(RaiseChange);
                    _factStorage[reg.Fact] = wrapper;
                }

                object defaultValue = reg.ValueOverride?.RawValue ?? reg.Fact.DefaultValueRaw;
                try
                {
                    wrapper.RawValue = _store.GetData(reg.Fact.Guid, defaultValue);
                }
                catch (Exception ex)
                {
                    Print.MError(ex);
                }
            }

            foreach (var reg in EventRegistrations)
            {
                if (reg.Event == null) continue;

                foreach (var reaction in reg.Reactions)
                    reaction?.Dispose();

                if (_eventStorage.TryGetValue(reg.Event, out var wrapper) == false)
                {
                    wrapper = reg.Event.GetValueWrapper();
                    _eventStorage[reg.Event] = wrapper;
                }
            }

            foreach (var key in Children)
            {
                key.Load(_store.GetData<Dictionary<string, object>>(key.Guid, null));
            }

            if (UseSaveSystem && (forceReadFile || m_save.AutoLoad)) _store.OnLoad();

            foreach (var reg in FactRegistrations)
            {
                foreach (var reaction in reg.Reactions)
                {
                    reaction?.Initialize(this, reg.Fact);
                    reaction?.OnLoad(_store.GetData<object>($"{reg.Fact.Guid}_{reaction.Guid}", null));
                }
            }

            foreach (var reg in EventRegistrations)
            {
                foreach (var reaction in reg.Reactions)
                {
                    reaction?.Initialize(this, reg.Event);
                    reaction?.OnLoad(_store.GetData<object>($"{reg.Event.Guid}_{reaction.Guid}", null));
                }
            }
        }

        public object Save()
        {
            _store.SetStore(new());
            foreach (var key in Children)
            {
                if (key.IsSerializable)
                {
                    var data = key.Save();
                    if (data != null)
                    {
                        _store.SetData(key.Guid, data);
                    }
                }
            }

            foreach (var reg in FactRegistrations)
            {
                if (reg.Fact == null) continue;
                if (!_factStorage.TryGetValue(reg.Fact, out var wrapper)) continue;

                bool isSerializable = reg.IsSerializable.GetValueOrDefault(reg.Fact.IsSerializable);
                if (isSerializable)
                {
                    object defaultValue = reg.ValueOverride?.RawValue ?? reg.Fact.DefaultValueRaw;
                    if (!defaultValue.Equals(wrapper.RawValue))
                        _store.SetData(reg.Fact.Guid, wrapper.RawValue);
                }

                foreach (var reaction in reg.Reactions)
                {
                    var data = reaction?.OnSave();
                    if (data != null) _store.SetData($"{reg.Fact.Guid}_{reaction.Guid}", data);
                }
            }

            foreach (var reg in EventRegistrations)
            {
                if (reg.Event == null) continue;
                if (!_eventStorage.TryGetValue(reg.Event, out var wrapper)) continue;

                foreach (var reaction in reg.Reactions)
                {
                    var data = reaction?.OnSave();
                    if (data != null) _store.SetData($"{reg.Event.Guid}_{reaction.Guid}", data);
                }
            }

            if (UseSaveSystem)
            {
                _store.OnSave();
                m_save.Write(_store.GetStore());
            }

            return _store.IsEmpty ? null : _store.GetStore();
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
        public void RemoveOnFactAddedListener(Action<FactDefinition> callback) => OnFactAdded -= callback;
        public void AddOnChangeListener(Action callback) => OnChange += callback;
        public void RemoveOnChangeListener(Action callback) => OnChange -= callback;
        public void Subscribe(IPersistent persistent) => _store.Subscribe(persistent);
        public void Unsubscribe(IPersistent persistent) => _store.Unsubscribe(persistent);
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

        private void RaiseChange()
        {
            if (UseSaveSystem && m_save.SaveOnChange) Save();
            OnChange?.Invoke();
        }

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
            Print.Error("Unknown defenition!");
            return null;
        }

        public KeyContainer RequestTempKey(string name, List<FactValueOverride> overrides = null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Print.Error("Requesting temp key is only allowed at runtime!");
                return null;
            }
#endif
            var key = CreateInstance<KeyContainer>();
            key.name = FactUtils.GenerateValidName(name + "_temp");
            key.IsSerializable = false;
            Children.Add(key);
            overrides?.ForEach(o =>
            {
                if (o.Fact != null)
                {
                    key.FactRegistrations.Add(new()
                    {
                        Fact = o.Fact,
                        ValueOverride = o.Wrapper,
                    });
                }
            });
            key.Load();
            return key;
        }
    }
}