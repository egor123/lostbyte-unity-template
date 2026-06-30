using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.FactSystem.Nodes;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public abstract class FactEvaluator<TValue, TNode> where TNode : class, IValueNode<TValue>
    {
        [SerializeField, SerializeReference] protected TNode m_rootNode;
        protected IKeyContainer _defaultKey;

        private event Action<TValue> OnChange;

        protected bool _hasCachedValue = false;
        protected TValue _cachedValue = default;

        public TValue Value => _hasCachedValue ? _cachedValue : (m_rootNode != null ? m_rootNode.Evaluate(_defaultKey) : default);

        protected FactEvaluator(TNode rootNode = null, IKeyContainer defaultKey = null)
        {
            m_rootNode = rootNode;
            _defaultKey = defaultKey;
        }

        public void OnConditionChange(object _)
        {
            var newValue = m_rootNode != null ? m_rootNode.Evaluate(_defaultKey) : default;
            if (!_hasCachedValue || !EqualityComparer<TValue>.Default.Equals(_cachedValue, newValue))
            {
                _cachedValue = newValue;
                _hasCachedValue = true;

                OnChange?.Invoke(newValue);
                OnValueChanged(newValue);
            }
        }

        protected virtual void OnValueChanged(TValue newValue) { }

        public void Subscribe(Action<TValue> callback)
        {
            callback?.Invoke(Value);
            OnChange += callback;
        }

        public void Unsubscribe(Action<TValue> callback) => OnChange -= callback;

        public void SetDefaultKey(IKeyContainer key)
        {
            m_rootNode?.Unsubscribe(_defaultKey, OnConditionChange);
            _defaultKey = key;
            m_rootNode?.Subscribe(_defaultKey, OnConditionChange);
        }

        public override string ToString() => m_rootNode?.ToString();
    }
}