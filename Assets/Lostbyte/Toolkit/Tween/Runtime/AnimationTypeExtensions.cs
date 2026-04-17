using UnityEngine;
using System;

namespace Lostbyte.Toolkit.Tween
{
    public static class AnimationTypeExtentions
    {
        public static AnimationCurve ToCurve(this AnimationType type)
        {
            return type switch
            {
                AnimationType.Linear => AnimationCurve.Linear(0, 0, 1, 1),

                AnimationType.EaseIn => AnimationCurve.EaseInOut(0, 0, 1, 1)
                    .SetTangent(0, 2f),
                AnimationType.EaseOut => AnimationCurve.EaseInOut(0, 0, 1, 1)
                    .SetTangent(2f, 0),
                AnimationType.EaseInOut => AnimationCurve.EaseInOut(0, 0, 1, 1),

                AnimationType.EaseInQuad => MakeCurve(EaseInQuad),
                AnimationType.EaseOutQuad => MakeCurve(EaseOutQuad),
                AnimationType.EaseInOutQuad => MakeCurve(EaseInOutQuad),

                AnimationType.EaseInCubic => MakeCurve(EaseInCubic),
                AnimationType.EaseOutCubic => MakeCurve(EaseOutCubic),
                AnimationType.EaseInOutCubic => MakeCurve(EaseInOutCubic),

                AnimationType.EaseInQuart => MakeCurve(EaseInQuart),
                AnimationType.EaseOutQuart => MakeCurve(EaseOutQuart),
                AnimationType.EaseInOutQuart => MakeCurve(EaseInOutQuart),

                AnimationType.EaseInQuint => MakeCurve(EaseInQuint),
                AnimationType.EaseOutQuint => MakeCurve(EaseOutQuint),
                AnimationType.EaseInOutQuint => MakeCurve(EaseInOutQuint),

                AnimationType.BounceIn => MakeCurve(BounceIn),
                AnimationType.BounceOut => MakeCurve(BounceOut),

                AnimationType.ElasticIn => MakeCurve(ElasticIn),
                AnimationType.ElasticOut => MakeCurve(ElasticOut),

                AnimationType.SmoothStep => MakeCurve(SmoothStep),
                AnimationType.SmootherStep => MakeCurve(SmootherStep),

                _ => throw new NotImplementedException(type.ToString())
            };
        }

        private static AnimationCurve MakeCurve(Func<float, float> func, int samples = 32)
        {
            Keyframe[] keys = new Keyframe[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                float t = i / (float)samples;
                keys[i] = new Keyframe(t, Mathf.Clamp01(func(t)));
            }
            return new AnimationCurve(keys);
        }

        private static float EaseInQuad(float t) => t * t;
        private static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        private static float EaseInOutQuad(float t) =>
            t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

        private static float EaseInCubic(float t) => t * t * t;
        private static float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
        private static float EaseInOutCubic(float t) =>
            t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;

        private static float EaseInQuart(float t) => t * t * t * t;
        private static float EaseOutQuart(float t) => 1 - Mathf.Pow(1 - t, 4);
        private static float EaseInOutQuart(float t) =>
            t < 0.5f ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;

        private static float EaseInQuint(float t) => t * t * t * t * t;
        private static float EaseOutQuint(float t) => 1 - Mathf.Pow(1 - t, 5);
        private static float EaseInOutQuint(float t) =>
            t < 0.5f ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;

        private static float BounceOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1 / d1) return n1 * t * t;
            else if (t < 2 / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            else if (t < 2.5 / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            else return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        private static float BounceIn(float t) => 1 - BounceOut(1 - t);

        private static float ElasticIn(float t) =>
            t == 0 ? 0 : t == 1 ? 1 :
            -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * (2 * Mathf.PI) / 3);

        private static float ElasticOut(float t) =>
            t == 0 ? 0 : t == 1 ? 1 :
            Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * (2 * Mathf.PI) / 3) + 1;

        private static float SmoothStep(float t) => t * t * (3 - 2 * t);
        private static float SmootherStep(float t) => t * t * t * (t * (t * 6 - 15) + 10);

        private static AnimationCurve SetTangent(this AnimationCurve curve, float inTan, float outTan)
        {
            var keys = curve.keys;
            if (keys.Length > 0)
            {
                keys[0].outTangent = outTan;
                keys[^1].inTangent = inTan;
                curve.keys = keys;
            }
            return curve;
        }

    }
}