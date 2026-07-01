using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public enum CountMode { Always, Once, Exactly, GreaterThan, LessThan }

    #region Base Architecture
    [Serializable]
    public abstract class ActionCondition<TValue>
    {
        public abstract bool IsValid(TValue oldValue, TValue newValue);
        public abstract ActionCondition<TValue> Copy();
    }

    [Serializable]
    public class ReactionRule<TValue, TCondition> where TCondition : ActionCondition<TValue>
    {
        public CountMode Mode;
        public int TargetCount;

        [SerializeReference, UniqeReference] public TCondition Condition;
        [OfType(typeof(IInvokable))] public ScriptableObject Action;

        public bool IsCountMet(int currentCount, TValue oldValue, TValue newValue)
        {
            if (Condition != null && !Condition.IsValid(oldValue, newValue))
                return false;

            return Mode switch
            {
                CountMode.Always => true,
                CountMode.Once => currentCount == 0,
                CountMode.Exactly => currentCount == TargetCount,
                CountMode.GreaterThan => currentCount > TargetCount,
                CountMode.LessThan => currentCount < TargetCount,
                _ => false
            };
        }

        public ReactionRule<TValue, TCondition> Copy()
        {
            return new ReactionRule<TValue, TCondition>
            {
                Mode = Mode,
                TargetCount = TargetCount,
                Action = Action,
                Condition = (TCondition)Condition?.Copy()
            };
        }
    }

    public abstract class BaseActionReaction<TValue, TCondition> : FactReaction
        where TCondition : ActionCondition<TValue>
    {
        [PlayModeOnly(PlayModeOnly.Type.Hide)] public int InvocationCount = 0;
        public Condition PreCondition;
        public List<ReactionRule<TValue, TCondition>> Rules = new();

        public override void Initialize(KeyContainer key, FactDefinition fact)
        {
            InvocationCount = 0;
            PreCondition?.SetDefaultKey(key);
            base.Initialize(key, fact);
        }

        public override void OnLoad(object data)
        {
            if (data is int count) InvocationCount = count;
        }

        public override object OnSave() => InvocationCount;

        protected override void OnValueChanged(object oldValue, object newValue)
        {
            if (PreCondition != null && !PreCondition.IsMet) return;

            TValue oldVal = oldValue != null ? (TValue)oldValue : default;
            TValue newVal = newValue != null ? (TValue)newValue : default;

            foreach (var rule in Rules)
            {
                if (rule.IsCountMet(InvocationCount, oldVal, newVal))
                {
                    (rule.Action as IInvokable)?.Invoke();
                    Print.Log($"Invoking Rule ({InvocationCount}): {rule.Action}");
                    InvocationCount++;
                    break;
                }
            }
        }
    }
    #endregion

    #region Primitive Types (Float, Int, Bool, String)

    // --- FLOAT ---
    [Serializable] public abstract class FloatCondition : ActionCondition<float> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(float))]
    public class FloatActionReaction : BaseActionReaction<float, FloatCondition>
    {
        public override FactReaction Copy()
        {
            var copy = new FloatActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

        [Serializable]
        public class GreaterThan : FloatCondition
        {
            public float TargetValue;
            public override bool IsValid(float oldF, float newF) => newF > TargetValue;
            public override ActionCondition<float> Copy() => new GreaterThan { TargetValue = TargetValue };
        }

        [Serializable]
        public class LessThan : FloatCondition
        {
            public float TargetValue;
            public override bool IsValid(float oldF, float newF) => newF < TargetValue;
            public override ActionCondition<float> Copy() => new LessThan { TargetValue = TargetValue };
        }

        [Serializable]
        public class EqualsTo : FloatCondition
        {
            public float TargetValue;
            public override bool IsValid(float oldF, float newF) => Mathf.Approximately(newF, TargetValue);
            public override ActionCondition<float> Copy() => new EqualsTo { TargetValue = TargetValue };
        }

        [Serializable]
        public class Increased : FloatCondition
        {
            public override bool IsValid(float oldF, float newF) => oldF < newF;
            public override ActionCondition<float> Copy() => new Increased { };
        }
    }

    // --- INT ---
    [Serializable] public abstract class IntCondition : ActionCondition<int> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(int))]
    public class IntActionReaction : BaseActionReaction<int, IntCondition>
    {
        public override FactReaction Copy()
        {
            var copy = new IntActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

        [Serializable]
        public class EqualsTo : IntCondition
        {
            public int TargetValue;
            public override bool IsValid(int oldI, int newI) => newI == TargetValue;
            public override ActionCondition<int> Copy() => new EqualsTo { TargetValue = TargetValue };
        }

        [Serializable]
        public class GreaterThan : IntCondition
        {
            public int TargetValue;
            public override bool IsValid(int oldI, int newI) => newI > TargetValue;
            public override ActionCondition<int> Copy() => new GreaterThan { TargetValue = TargetValue };
        }
    }

    // --- BOOL ---
    [Serializable] public abstract class BoolCondition : ActionCondition<bool> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(bool))]
    public class BoolActionReaction : BaseActionReaction<bool, BoolCondition>
    {
        public override FactReaction Copy()
        {
            var copy = new BoolActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

        [Serializable]
        public class IsTrue : BoolCondition
        {
            public override bool IsValid(bool oldB, bool newB) => newB == true;
            public override ActionCondition<bool> Copy() => new IsTrue();
        }

        [Serializable]
        public class IsFalse : BoolCondition
        {
            public override bool IsValid(bool oldB, bool newB) => newB == false;
            public override ActionCondition<bool> Copy() => new IsFalse();
        }

        [Serializable]
        public class OnChanged : BoolCondition
        {
            public override bool IsValid(bool oldB, bool newB) => oldB != newB;
            public override ActionCondition<bool> Copy() => new OnChanged();
        }
    }

    // --- STRING ---
    [Serializable] public abstract class StringCondition : ActionCondition<string> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(string))]
    public class StringActionReaction : BaseActionReaction<string, StringCondition>
    {
        public override FactReaction Copy()
        {
            var copy = new StringActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

        [Serializable]
        public class EqualsTo : StringCondition
        {
            public string TargetValue;
            public override bool IsValid(string oldS, string newS) => newS == TargetValue;
            public override ActionCondition<string> Copy() => new EqualsTo { TargetValue = TargetValue };
        }

        [Serializable]
        public class Contains : StringCondition
        {
            public string TargetValue;
            public override bool IsValid(string oldS, string newS) => !string.IsNullOrEmpty(newS) && newS.Contains(TargetValue);
            public override ActionCondition<string> Copy() => new Contains { TargetValue = TargetValue };
        }
    }
    #endregion

    #region Unity Math Types (Vector2, Vector3, Vector4, Color)

    // --- VECTOR 3 ---
    [Serializable] public abstract class Vector3Condition : ActionCondition<Vector3> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(Vector3))]
    public class Vector3ActionReaction : BaseActionReaction<Vector3, Vector3Condition>
    {
        public override FactReaction Copy()
        {
            var copy = new Vector3ActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }
    }

    // --- VECTOR 2 ---
    [Serializable] public abstract class Vector2Condition : ActionCondition<Vector2> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(Vector2))]
    public class Vector2ActionReaction : BaseActionReaction<Vector2, Vector2Condition>
    {
        public override FactReaction Copy()
        {
            var copy = new Vector2ActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

    }

    // --- VECTOR 4 ---
    [Serializable] public abstract class Vector4Condition : ActionCondition<Vector4> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(Vector4))]
    public class Vector4ActionReaction : BaseActionReaction<Vector4, Vector4Condition>
    {
        public override FactReaction Copy()
        {
            var copy = new Vector4ActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

        [Serializable]
        public class ExactMatch : Vector4Condition
        {
            public Vector4 TargetValue;
            public override bool IsValid(Vector4 oldV, Vector4 newV) => newV == TargetValue;
            public override ActionCondition<Vector4> Copy() => new ExactMatch { TargetValue = TargetValue };
        }
    }

    // --- COLOR ---
    [Serializable] public abstract class ColorCondition : ActionCondition<Color> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(Color))]
    public class ColorActionReaction : BaseActionReaction<Color, ColorCondition>
    {
        public override FactReaction Copy()
        {
            var copy = new ColorActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

        [Serializable]
        public class ExactMatch : ColorCondition
        {
            public Color TargetColor;
            public override bool IsValid(Color oldC, Color newC) => newC == TargetColor;
            public override ActionCondition<Color> Copy() => new ExactMatch { TargetColor = TargetColor };
        }
    }
    #endregion

    #region Enum

    [Serializable]
    public abstract class EnumCondition : ActionCondition<Enum> { }

    [Tag("Events")]
    [SupportedFactTypes(typeof(Enum))]
    public class EnumActionReaction : BaseActionReaction<Enum, EnumCondition>
    {
        public override FactReaction Copy()
        {
            var copy = new EnumActionReaction { PreCondition = PreCondition, Rules = new() };
            foreach (var rule in Rules) copy.Rules.Add(rule.Copy());
            return copy;
        }

        [Serializable]
        public class EqualsTo : EnumCondition
        {
            [SerializeReference] public Enum TargetValue;

            public override bool IsValid(Enum oldE, Enum newE)
            {
                if (newE == null || TargetValue == null) return false;
                return newE.Equals(TargetValue);
            }

            public override ActionCondition<Enum> Copy() => new EqualsTo { TargetValue = TargetValue };
        }

        [Serializable]
        public class NotEqualsTo : EnumCondition
        {
            [SerializeReference] public Enum TargetValue;

            public override bool IsValid(Enum oldE, Enum newE)
            {
                if (newE == null || TargetValue == null) return false;
                return !newE.Equals(TargetValue);
            }

            public override ActionCondition<Enum> Copy() => new NotEqualsTo { TargetValue = TargetValue };
        }
    }

    #endregion
}