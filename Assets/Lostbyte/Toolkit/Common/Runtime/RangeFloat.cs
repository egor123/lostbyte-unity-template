using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    [System.Serializable]
    public struct RangeFloat
    {
        public float Min;
        public float Max;
        public RangeFloat(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public readonly float GetRandomValueInRange() => Random.Range(Min, Max);
    }

    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float MinLimit { get; private set; }
        public float MaxLimit { get; private set; }

        public MinMaxRangeAttribute(float minLimit, float maxLimit)
        {
            MinLimit = minLimit;
            MaxLimit = maxLimit;
        }
    }
}
