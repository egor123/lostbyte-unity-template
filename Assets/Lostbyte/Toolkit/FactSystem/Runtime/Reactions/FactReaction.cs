using System;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public abstract class FactReaction
    {
        internal string Guid => GetType().Name.ToLowerInvariant();
        public FactDefinition Fact { get; private set; }
        public KeyContainer Key { get; private set; }
        protected IFactWrapper Wrapper { get; private set; }

        public virtual void Initialize(KeyContainer key, FactDefinition fact)
        {
            Wrapper?.Unsubscribe(OnValueChanged);

            Key = key;
            Fact = fact;

            Wrapper = Key.GetWrapper(Fact);
            Wrapper.Subscribe(OnValueChanged);
        }
        public virtual void Dispose()
        {
            Wrapper?.Unsubscribe(OnValueChanged);
            Wrapper = null;
        }
        protected abstract void OnValueChanged(object oldValue, object newValue);
        public virtual void OnLoad(object data) { }
        public virtual object OnSave() => null;
        public abstract FactReaction Copy();
    }
}
