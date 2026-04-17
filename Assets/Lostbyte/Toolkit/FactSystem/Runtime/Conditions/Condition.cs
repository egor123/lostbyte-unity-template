using System;
using System.Linq;
using Lostbyte.Toolkit.FactSystem.Nodes;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public class Condition
    {
        [SerializeField, SerializeReference] private IBoolNode m_rootNode;
        private IKeyContainer _defaultKey;
        private Action OnTrigger;
        private Action<bool> OnChange;

        private bool? _isMet = null;
        public bool IsMet => _isMet ?? m_rootNode?.Evaluate(_defaultKey) ?? false;
        public Condition(IBoolNode rootNode = null, IKeyContainer defaultKey = null) => (m_rootNode, _defaultKey) = (rootNode, defaultKey);
        public void OnConditionChange(object _)
        {
            var newValue = m_rootNode?.Evaluate(_defaultKey) ?? false;
            if (!_isMet.HasValue || (_isMet.Value != newValue))
            {
                _isMet = newValue;
                if (newValue) OnTrigger?.Invoke();
                OnChange?.Invoke(newValue);
            }
        }
        public void Subscribe(Action callback)
        {
            if (IsMet) callback?.Invoke();
            OnTrigger += callback;
        }
        public void Unsubscribe(Action callback) => OnTrigger -= callback;
        public void Subscribe(Action<bool> callback)
        {
            callback?.Invoke(IsMet);
            OnChange += callback;
        }
        public void Unsubscribe(Action<bool> callback) => OnChange -= callback;
        public void SetDefaultKey(IKeyContainer key)
        {
            m_rootNode?.Unsubscribe(_defaultKey, OnConditionChange);
            _defaultKey = key;
            m_rootNode?.Subscribe(_defaultKey, OnConditionChange);
        }
        public override string ToString() => m_rootNode?.ToString();
        public Condition Copy()
        {
            return new Condition() { m_rootNode = m_rootNode, _defaultKey = _defaultKey };
        }
    }
}