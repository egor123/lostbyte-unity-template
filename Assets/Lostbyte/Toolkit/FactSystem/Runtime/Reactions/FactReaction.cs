using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public abstract class FactReaction
    {
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
        protected abstract void OnValueChanged(object newValue);
        public abstract FactReaction Copy();
    }
}
