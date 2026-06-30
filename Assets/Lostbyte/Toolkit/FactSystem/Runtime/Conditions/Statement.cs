using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem.Nodes;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public class Statement
    {
        [SerializeField, SerializeReference] private IActionNode m_rootNode;
        private IKeyContainer _defaultKey;

        public Statement(IActionNode rootNode = null, IKeyContainer defaultKey = null)
        {
            m_rootNode = rootNode;
            _defaultKey = defaultKey;
        }

        public void Execute() => m_rootNode?.Execute(_defaultKey);
        public void SetDefaultKey(IKeyContainer key) => _defaultKey = key;
        public override string ToString() => m_rootNode?.ToString();
        public Statement Copy() => new(m_rootNode, _defaultKey);
    }
}