using Lostbyte.Toolkit.Audio.Music;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Audio.Editor
{
    [CustomPropertyDrawer(typeof(StemData))]
    public class StemDataDrawer : PropertyDrawer
    {
        private static readonly Color k_KnobBaseColor = new(0.18f, 0.18f, 0.18f, 1f);
        private static readonly Color k_KnobBorderColor = new(0.35f, 0.35f, 0.35f, 1f);
        private static readonly Color k_KnobBorderHoverColor = new(0.55f, 0.55f, 0.55f, 1f);
        private static readonly Color k_KnobBorderSelectedColor = new(0.0f, 0.75f, 1.0f, 1f);
        private static readonly Color k_IndicatorColor = new(0.0f, 0.75f, 1.0f, 1f);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement().MakeRow(Align.Center, Justify.SpaceBetween).SetMargin(1);
            var volumeProp = property.FindPropertyRelative(nameof(StemData.DefaultVolume));
            var clipProp = property.FindPropertyRelative(nameof(StemData.Clip));

            IntegerField intVolumeField = new IntegerField()
            {
                isDelayed = true,
                value = Mathf.RoundToInt(volumeProp.floatValue * 100f)
            }.SetSize(36, 18).SetMargin(0, 5, 0, 2).SetParent(root);

            var knobContainer = new VisualElement() { focusable = true }
                .ClearPaddingAndMargin()
                .MakeColumn(Align.Center, Justify.FlexStart)
                .SetSize(18, 18)
                .SetFlex(0, 0)
                .SetBackgroundColor(k_KnobBaseColor)
                .SetBorder(1, 9, k_KnobBorderColor)
                .SetMargin(0, 5, 0, 0)
                .SetParent(root);

            var knobIndicator = new VisualElement()
                .SetSize(2, 5)
                .SetMargin(1, 0, 0, 0)
                .SetBackgroundColor(k_IndicatorColor)
                .SetBorderRadius(1)
                .SetParent(knobContainer);

            var clipField = new PropertyField(clipProp, string.Empty)
                .SetFlex(1, 0)
                .SetParent(root);

            bool isDragging = false;
            bool isHovered = false;
            bool isFocused = false;
            float startMouseX = 0f;
            float startVal = 0f;
            const float dragSensitivity = 60f;

            void UpdateBorderColor()
            {
                if (isDragging || isFocused) knobContainer.SetBorderColor(k_KnobBorderSelectedColor);
                else if (isHovered) knobContainer.SetBorderColor(k_KnobBorderHoverColor);
                else knobContainer.SetBorderColor(k_KnobBorderColor);
            }
            void UpdateKnobVisuals(float val)
            {
                knobContainer.style.rotate = new Rotate(Mathf.Lerp(-135f, 135f, val));
            }
            knobContainer.RegisterCallback<FocusInEvent>(evt =>
            {
                isFocused = true;
                UpdateBorderColor();
            });
            knobContainer.RegisterCallback<FocusOutEvent>(evt =>
            {
                isFocused = false;
                UpdateBorderColor();
            });
            UpdateKnobVisuals(volumeProp.floatValue);
            knobContainer.RegisterCallback<PointerDownEvent>(evt =>
            {
                isDragging = true;
                startMouseX = evt.position.x;
                startVal = volumeProp.floatValue;
                knobContainer.Focus();
                UpdateBorderColor();
                knobContainer.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });
            knobContainer.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!isDragging) return;
                float deltaX = evt.position.x - startMouseX;
                float newVal = Mathf.Clamp01(startVal + (deltaX / dragSensitivity));
                volumeProp.serializedObject.Update();
                volumeProp.floatValue = newVal;
                volumeProp.serializedObject.ApplyModifiedProperties();
                intVolumeField.SetValueWithoutNotify(Mathf.RoundToInt(newVal * 100f));
                UpdateKnobVisuals(newVal);
                evt.StopPropagation();
            });
            knobContainer.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!isDragging) return;
                isDragging = false;
                knobContainer.ReleasePointer(evt.pointerId);
                UpdateBorderColor();
                evt.StopPropagation();
            });
            intVolumeField.RegisterValueChangedCallback(evt =>
            {
                volumeProp.serializedObject.Update();
                float val = Mathf.Clamp01(evt.newValue / 100f);
                volumeProp.floatValue = val;
                volumeProp.serializedObject.ApplyModifiedProperties();
                UpdateKnobVisuals(val);
            });
            root.TrackPropertyValue(volumeProp, (p) =>
            {
                intVolumeField.SetValueWithoutNotify(Mathf.RoundToInt(p.floatValue * 100f));
                UpdateKnobVisuals(p.floatValue);
            });
            root.Bind(property.serializedObject);
            return root;
        }
    }
}