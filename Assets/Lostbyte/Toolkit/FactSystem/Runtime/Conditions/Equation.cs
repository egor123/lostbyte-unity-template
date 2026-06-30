using System;
using System.Numerics;
using Lostbyte.Toolkit.FactSystem.Nodes;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public class NumericEquation : FactEvaluator<float, IValueNode<float>>
    {
        public NumericEquation(IValueNode<float> rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public NumericEquation Copy() => new(m_rootNode, _defaultKey);
    }
    [Serializable]
    public class StringEquation : FactEvaluator<string, IValueNode<string>>
    {
        public StringEquation(IValueNode<string> rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public StringEquation Copy() => new(m_rootNode, _defaultKey);
    }
    [Serializable]
    public class VectorEquation : FactEvaluator<Vector4, IValueNode<Vector4>>
    {
        public VectorEquation(IValueNode<Vector4> rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public VectorEquation Copy() => new(m_rootNode, _defaultKey);
    }
    [Serializable]
    public class BoolEquation : FactEvaluator<bool, IValueNode<bool>>
    {
        public BoolEquation(IValueNode<bool> rootNode = null, IKeyContainer defaultKey = null) : base(rootNode, defaultKey) { }
        public BoolEquation Copy() => new(m_rootNode, _defaultKey);
    }
}