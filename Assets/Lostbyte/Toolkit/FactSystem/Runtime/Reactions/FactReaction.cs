using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public abstract class FactReaction
    {
        [SerializeField, HideInInspector] private string _guid;
        public string Guid => string.IsNullOrEmpty(_guid) ? (_guid = System.Guid.NewGuid().ToString()) : _guid;
        public FactDefinition Fact { get; private set; }
        public KeyContainer Key { get; private set; }
        protected IFactWrapper Wrapper { get; private set; }
        public object Value => Wrapper.RawValue;
        public virtual void Initialize(KeyContainer key, FactDefinition fact)
        {
            Wrapper?.Unsubscribe(ChangeValue);

            Key = key;
            Fact = fact;

            Wrapper = Key.GetWrapper(Fact);
            Wrapper.Subscribe(ChangeValue);
        }
        public virtual void Dispose()
        {
            Wrapper?.Unsubscribe(ChangeValue);
            Wrapper = null;
        }
        public void ChangeValue(object oldValue, object newValue)
        {
            if(Key.PreConditionIsMet) OnValueChanged(oldValue, newValue);
        }
        protected abstract void OnValueChanged(object oldValue, object newValue);
        public virtual void OnLoad(object data) { }
        public virtual object OnSave() => null;
        public abstract FactReaction Copy();
    }
}
