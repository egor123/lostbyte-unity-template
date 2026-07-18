using System;
using Lostbyte.Toolkit.FactSystem.Nodes;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public class NumericEquation : FactEvaluator<float, INumericNode>
    {
        public NumericEquation(INumericNode rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public NumericEquation Copy() => new(m_rootNode, _defaultKey);
    }
    [Serializable]
    public class StringEquation : FactEvaluator<string, IStringNode>
    {
        public StringEquation(IStringNode rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public StringEquation Copy() => new(m_rootNode, _defaultKey);
    }
    [Serializable]
    public class VectorEquation : FactEvaluator<Vector4, IVectorNode>
    {
        public VectorEquation(IVectorNode rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public VectorEquation Copy() => new(m_rootNode, _defaultKey);
    }
    [Serializable]
    public class BoolEquation : FactEvaluator<bool, IBoolNode>
    {
        public BoolEquation(IBoolNode rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public BoolEquation Copy() => new(m_rootNode, _defaultKey);
    }
}