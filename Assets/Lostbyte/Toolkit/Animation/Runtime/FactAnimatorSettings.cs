using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Animation
{
    [CreateAssetMenu(fileName = nameof(FactAnimatorSettings), menuName = "Facts/Animation/FactAnimatorSettings")]
    public class FactAnimatorSettings : ScriptableObject
    {
        [field: SerializeField, SerializeReference, UniqeReference] public List<IAnimationParameter> Properties { get; private set; }
        public interface IAnimationParameter
        {
            ParameterBehaviour GetBehaviour(Animator animator, KeyContainer key);
        }
        public class TriggerParameter : IAnimationParameter
        {
            public Definition Definition;
            public string TriggerName;
            public ParameterBehaviour GetBehaviour(Animator animator, KeyContainer key) => new DefinitionParameterBehaviour(key, Definition, () => animator.SetTrigger(TriggerName));
        }
        public class IntParameter : IAnimationParameter
        {
            public IntFactDefinition Fact;
            public string IntName;
            public ParameterBehaviour GetBehaviour(Animator animator, KeyContainer key) => new FactParameterBehaviour<int>(key, Fact, (value) => animator.SetInteger(IntName, value));
        }
        public class EnumParameter : IAnimationParameter
        {
            public EnumFactDefinition Fact;
            public string IntName;
            public ParameterBehaviour GetBehaviour(Animator animator, KeyContainer key) => new FactParameterBehaviour<Enum>(key, Fact, (value) => animator.SetInteger(IntName, Convert.ToInt32(value)));
        }
        public class BoolParameter : IAnimationParameter
        {
            public BoolFactDefinition Fact;
            public string BoolName;
            public ParameterBehaviour GetBehaviour(Animator animator, KeyContainer key) => new FactParameterBehaviour<bool>(key, Fact, (value) => animator.SetBool(BoolName, value));
        }
        public class FloatParameter : IAnimationParameter
        {
            public FloatFactDefinition Fact;
            public string FloatName;
            public ParameterBehaviour GetBehaviour(Animator animator, KeyContainer key) => new FactParameterBehaviour<float>(key, Fact, (value) => animator.SetFloat(FloatName, value));
        }
        public class DampedFloatParameter : IAnimationParameter
        {
            public FloatFactDefinition Fact;
            public string FloatName;
            public float DampTime;
            public ParameterBehaviour GetBehaviour(Animator animator, KeyContainer key) => new FactParameterBehaviour<float>(key, Fact, null, (value) => animator.SetFloat(FloatName, value, DampTime, Time.deltaTime));
        }
        public FactAnimatorRunner GetInstance(Animator animator, KeyContainer key) => new(this, animator, key);
    }
    public abstract class ParameterBehaviour
    {
        public abstract void Enable();
        public abstract void Disable();
        public abstract void Update();
    }
    public class DefinitionParameterBehaviour : ParameterBehaviour
    {
        private readonly IWrapper _def;
        private readonly Action Callback;
        public DefinitionParameterBehaviour(KeyContainer key, Definition def, Action callback = null)
        {
            (Callback, _def) = (callback, key.GetWrapper(def));
        }
        public override void Enable() { if (Callback != null) _def.Subscribe(Callback); }
        public override void Disable() { if (Callback != null) _def.Unsubscribe(Callback); }
        public override void Update() { }
    }
    public class FactParameterBehaviour<TValue> : ParameterBehaviour
    {
        private readonly IFactWrapper<TValue> _fact;
        private readonly Action<TValue> UpdateCallback;
        private readonly Action<TValue> Callback;
        public FactParameterBehaviour(KeyContainer key, FactDefinition<TValue> fact, Action<TValue> callback = null, Action<TValue> updateCallback = null)
        {
            (Callback, UpdateCallback, _fact) = (callback, updateCallback, key.GetWrapper(fact));
            Callback?.Invoke(_fact.Value);
        }
        public override void Enable() { if (Callback != null) _fact.Subscribe(Callback); }
        public override void Disable() { if (Callback != null) _fact.Unsubscribe(Callback); }
        public override void Update() { UpdateCallback?.Invoke(_fact.Value); }
    }
    public class FactAnimatorRunner
    {
        private readonly List<ParameterBehaviour> _props;
        public FactAnimatorRunner(FactAnimatorSettings settings, Animator animator, KeyContainer key)
        {
            _props = settings.Properties.Select(p => p.GetBehaviour(animator, key)).ToList();
        }
        public void Enable() => _props.ForEach(p => p.Enable());
        public void Disable() => _props.ForEach(p => p.Disable());
        public void Update() => _props.ForEach(p => p.Update());
    }
}
