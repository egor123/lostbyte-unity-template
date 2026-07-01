using System;
using Lostbyte.Toolkit.FactSystem.Nodes;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public class Condition : FactEvaluator<bool, IBoolNode>
    {
        private event Action OnTrigger;
        public bool IsMet => m_rootNode == null || Value;
        public Condition(IBoolNode rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        protected override void OnValueChanged(bool newValue)
        {
            if (newValue) OnTrigger?.Invoke();
        }
        public void Subscribe(Action callback)
        {
            if (IsMet) callback?.Invoke();
            OnTrigger += callback;
        }
        public void Unsubscribe(Action callback) => OnTrigger -= callback;
        public Condition Copy() => new(m_rootNode, _defaultKey);
    }
}