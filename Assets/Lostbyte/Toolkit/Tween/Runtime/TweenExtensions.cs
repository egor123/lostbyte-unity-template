using UnityEngine;
using System;
using UnityEngine.UI;
namespace Lostbyte.Toolkit.Tween
{
    public static class TweenExtensions
    {
        public static T SetAnimation<T>(this T t, AnimationCurve curve) where T : Tween => Base(t, () => t.AnimationCurve = curve);
        public static T SetAnimation<T>(this T t, AnimationType type) where T : Tween => Base(t, () => t.AnimationCurve = type.ToCurve());
        public static T SetDuration<T>(this T t, float duration) where T : Tween => Base(t, () => t.Duration = duration);
        public static T SetCallback<T>(this T t, Action callback) where T : Tween => Base(t, () => t.Callback = callback);
        public static T SetLoop<T>(this T t, WrapMode mode = WrapMode.PingPong, int repeat = -1) where T : Tween => Base(t, () => { t.Loop = mode; t.LoopCount = repeat; });
        public static T ClearLoop<T>(this T t) where T : Tween => Base(t, () => { t.Loop = WrapMode.Clamp; t.LoopCount = 1; });
        public static T SetDeltaType<T>(this T t, TimeDeltaType type) where T : Tween => Base(t, () => { t.DeltaType = type; });

        //-----------------------------------------------------------------------------------------
        public static TweenGroup TweenGroup(this MonoBehaviour initiator) => new SequantialTweenGroup(initiator);
        public static TweenGroup And(this TweenGroup g, Tween tween)
        {

            if (g is ParallelTweenGroup pg)
            {
                pg.Tweens.Add(tween);
                return pg;
            }
            var sg = new ParallelTweenGroup(g.Initiator);
            if (g.Tweens.Count > 0)
                sg.Tweens.Add(g);
            sg.Tweens.Add(tween);
            return sg;
        }
        public static TweenGroup Then(this TweenGroup g, Tween tween)
        {
            if (g is SequantialTweenGroup sg)
            {
                sg.Tweens.Add(tween);
                return sg;
            }
            var pg = new SequantialTweenGroup(g.Initiator);
            if (g.Tweens.Count > 0)
                pg.Tweens.Add(g);
            pg.Tweens.Add(tween);
            return pg;
        }
        //-----------------------------------------------------------------------------------------
        public static TransformTween Tween(this MonoBehaviour initiator, Transform transform, Space space = Space.Self, float duration = 1f) => new(initiator, transform, space, duration);
        public static TransformTween Move(this TransformTween t, Vector3 position) => Base(t, () => t.TargetPosition = position);
        public static TransformTween MoveX(this TransformTween t, float x) => Base(t, () => t.OffsetTargetPosition(new(x, 0, 0)));
        public static TransformTween MoveY(this TransformTween t, float y) => Base(t, () => t.OffsetTargetPosition(new(0, y, 0)));
        public static TransformTween MoveZ(this TransformTween t, float z) => Base(t, () => t.OffsetTargetPosition(new(0, 0, z)));
        public static TransformTween Rotate(this TransformTween t, Quaternion rotation) => Base(t, () => t.TargetRotation = rotation);
        public static TransformTween RotateX(this TransformTween t, float x) => Base(t, () => t.OffsetTargetRotation(new(x, 0, 0)));
        public static TransformTween RotateY(this TransformTween t, float y) => Base(t, () => t.OffsetTargetRotation(new(0, y, 0)));
        public static TransformTween RotateZ(this TransformTween t, float z) => Base(t, () => t.OffsetTargetRotation(new(0, 0, z)));
        public static TransformTween Scale(this TransformTween t, Vector3 scale) => Base(t, () => t.TargetScale = scale);
        public static TransformTween Scale(this TransformTween t, float scale) => Base(t, () => t.OffsetTargetScale(new(scale, scale, scale)));
        public static TransformTween ScaleX(this TransformTween t, float x) => Base(t, () => t.OffsetTargetScale(new(x, 0, 0)));
        public static TransformTween ScaleY(this TransformTween t, float y) => Base(t, () => t.OffsetTargetScale(new(0, y, 0)));
        public static TransformTween ScaleZ(this TransformTween t, float z) => Base(t, () => t.OffsetTargetScale(new(0, 0, z)));
        //--------------------------------------------------------------------------------------------
        public static ColorTween Tween(this MonoBehaviour initiator, SpriteRenderer renderer, Color color, float duration = 1f) => new SpriteRendererColorTween(initiator, renderer, color, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, SpriteRenderer renderer, float alpha, float duration = 1f) => new SpriteRendererColorTween(initiator, renderer, alpha, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, Graphic renderer, Color color, float duration = 1f) => new GraphicColorTween(initiator, renderer, color, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, Graphic renderer, float alpha, float duration = 1f) => new GraphicColorTween(initiator, renderer, alpha, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, MeshRenderer renderer, Color color, float duration = 1f) => new MeshRendererColorTween(initiator, renderer, color, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, MeshRenderer renderer, float alpha, float duration = 1f) => new MeshRendererColorTween(initiator, renderer, alpha, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, SkinnedMeshRenderer renderer, Color color, float duration = 1f) => new SkinnedMeshRendererColorTween(initiator, renderer, color, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, SkinnedMeshRenderer renderer, float alpha, float duration = 1f) => new SkinnedMeshRendererColorTween(initiator, renderer, alpha, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, Light renderer, Color color, float duration = 1f) => new LightColorTween(initiator, renderer, color, duration);
        public static ColorTween Tween(this MonoBehaviour initiator, Light renderer, float alpha, float duration = 1f) => new LightColorTween(initiator, renderer, alpha, duration);
        public static ColorTween SetColor(this ColorTween t, Color color) => Base(t, () => t.TargetColor = color);
        //--------------------------------------------------------------------------------------------
        public static StringTween Tween(this MonoBehaviour initiator, Action<string> onChange) => new(initiator, onChange);
        public static StringTween SetTargetString(this StringTween t, string text) => Base(t, () => t.TargetText = text);
        //--------------------------------------------------------------------------------------------
        public static FloatTween Tween(this MonoBehaviour initiator, Action<float> onChange) => new(initiator, onChange);
        public static FloatTween SetStartFloat(this FloatTween t, float value) => Base(t, () => t.StartFloat = value);
        public static FloatTween SetTargetFloat(this FloatTween t, float value) => Base(t, () => t.TargetFloat = value);
        //--------------------------------------------------------------------------------------------
        public static Vector3Tween Tween(this MonoBehaviour initiator, Action<Vector3> onChange) => new(initiator, onChange);
        public static Vector3Tween SetStartVector(this Vector3Tween t, Vector3 value) => Base(t, () => t.StartVector = value);
        public static Vector3Tween SetTargetVector(this Vector3Tween t, Vector3 value) => Base(t, () => t.TargetVector = value);
        //--------------------------------------------------------------------------------------------

        public static T Base<T>(T tween, Action act) where T : Tween
        {
            if (tween.IsRunning) Debug.LogWarning("Cannot change tween while it isrunning!");
            else act.Invoke();
            return tween;
        }
    }
}