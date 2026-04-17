using UnityEngine;
using UnityEngine.UI;

namespace Lostbyte.Toolkit.Tween
{
    public abstract class ColorTween : Tween
    {
        private Color _startColor;
        internal Color TargetColor;
        protected ColorTween(MonoBehaviour initiator, Color color, float duration) : base(initiator, duration) { TargetColor = color; }
        protected override void DoTween(float delta) => SetColor(Color.Lerp(_startColor, TargetColor, delta));
        internal override void Init() => _startColor = GetColor();
        protected abstract Color GetColor();
        protected abstract void SetColor(Color color);
    }
    public class SpriteRendererColorTween : ColorTween
    {
        protected readonly SpriteRenderer _r;
        public SpriteRendererColorTween(MonoBehaviour initiator, SpriteRenderer r, Color color, float duration) : base(initiator, color, duration) { _r = r; }
        public SpriteRendererColorTween(MonoBehaviour initiator, SpriteRenderer r, float alpha, float duration) : base(initiator, new(r.color.r, r.color.g, r.color.b, alpha), duration) { _r = r; }
        protected override Color GetColor() => _r.color;
        protected override void SetColor(Color color) => _r.color = color;
    }
    public class GraphicColorTween : ColorTween
    {
        protected readonly Graphic _r;
        public GraphicColorTween(MonoBehaviour initiator, Graphic r, Color color, float duration) : base(initiator, color, duration) { _r = r; }
        public GraphicColorTween(MonoBehaviour initiator, Graphic r, float alpha, float duration) : base(initiator, new(r.color.r, r.color.g, r.color.b, alpha), duration) { _r = r; }
        protected override Color GetColor() => _r.color;
        protected override void SetColor(Color color) => _r.color = color;
    }
    public class MeshRendererColorTween : ColorTween
    {
        protected readonly MeshRenderer _r;
        public MeshRendererColorTween(MonoBehaviour initiator, MeshRenderer r, Color color, float duration) : base(initiator, color, duration) { _r = r; }
        public MeshRendererColorTween(MonoBehaviour initiator, MeshRenderer r, float alpha, float duration) : base(initiator, new(r.material.color.r, r.material.color.g, r.material.color.b, alpha), duration) { _r = r; }
        protected override Color GetColor() => _r.material.color;
        protected override void SetColor(Color color) => _r.material.color = color;
    }
    public class SkinnedMeshRendererColorTween : ColorTween
    {
        protected readonly SkinnedMeshRenderer _r;
        public SkinnedMeshRendererColorTween(MonoBehaviour initiator, SkinnedMeshRenderer r, Color color, float duration) : base(initiator, color, duration) { _r = r; }
        public SkinnedMeshRendererColorTween(MonoBehaviour initiator, SkinnedMeshRenderer r, float alpha, float duration) : base(initiator, new(r.material.color.r, r.material.color.g, r.material.color.b, alpha), duration) { _r = r; }
        protected override Color GetColor() => _r.material.color;
        protected override void SetColor(Color color) => _r.material.color = color;
    }
    public class LightColorTween : ColorTween
    {
        protected readonly Light _r;
        public LightColorTween(MonoBehaviour initiator, Light r, Color color, float duration) : base(initiator, color, duration) { _r = r; }
        public LightColorTween(MonoBehaviour initiator, Light r, float alpha, float duration) : base(initiator, new(r.color.r, r.color.g, r.color.b, alpha), duration) { _r = r; }
        protected override Color GetColor() => _r.color;
        protected override void SetColor(Color color) => _r.color = color;
    }
}
