using UnityEngine;
using TMPro;

namespace Lostbyte.Toolkit.Tween.TMP
{
    // public class TextMeshProTween : Tween
    // {
    //     private TMP_Text _textMeshPro;
    //     private string _startText;
    //     internal TextMeshProTween(MonoBehaviour initiator, TMP_Text textMeshPro) : base(initiator, 1f) { _textMeshPro = textMeshPro; }
    //     internal string TargetText { private get; set; }
    //     internal override void Init() => _startText = _textMeshPro.text;
    //     protected override void DoTween(float delta) => _textMeshPro.text = _startText + TargetText.Substring(0, Mathf.RoundToInt(delta * TargetText.Length));
    // }

    // public static class TextMeshProTweenExtensions{
    //     public static TextMeshProTween Tween(this MonoBehaviour initiator, TMP_Text textMeshPro) => new(initiator, textMeshPro);
    //     public static TextMeshProTween SetTargetText(this TextMeshProTween t, string text) => TweenExtensions.Base(t, () => t.TargetText = text);
    // }
}
