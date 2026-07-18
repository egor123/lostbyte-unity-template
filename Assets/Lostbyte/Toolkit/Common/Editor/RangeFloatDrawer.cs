using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Common.Editor
{
    [CustomPropertyDrawer(typeof(RangeFloat))]
    public class RangeFloatDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var minMaxAttribute = fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true)
                .FirstOrDefault() as MinMaxRangeAttribute;

            float limitMin = minMaxAttribute?.MinLimit ?? 0f;
            float limitMax = minMaxAttribute?.MaxLimit ?? 1f;

            var minProperty = property.FindPropertyRelative(nameof(RangeFloat.Min));
            var maxProperty = property.FindPropertyRelative(nameof(RangeFloat.Max));

            return CreateMinMaxSlider(property.displayName, property, minProperty, maxProperty, limitMin, limitMax);
        }
        private VisualElement CreateMinMaxSlider(string labelText, SerializedProperty prop, SerializedProperty minProp, SerializedProperty maxProp, float minLimit, float maxLimit)
        {
            var container = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginTop = 2 } };
            var label = new Label(labelText) { style = { unityTextAlign = TextAnchor.MiddleLeft } };
            var minField = new FloatField { bindingPath = minProp.propertyPath, style = { width = 45 } };
            var maxField = new FloatField { bindingPath = maxProp.propertyPath, style = { width = 45 } };
            var slider = new MinMaxSlider(minProp.floatValue, maxProp.floatValue, minLimit, maxLimit) { style = { flexGrow = 1, paddingLeft = 2, paddingRight = 2 } };
            slider.RegisterValueChangedCallback(evt =>
            {
                minProp.floatValue = (float)System.Math.Round(evt.newValue.x, 2);
                maxProp.floatValue = (float)System.Math.Round(evt.newValue.y, 2);
                prop.serializedObject.ApplyModifiedProperties();
            });
            slider.TrackPropertyValue(minProp, prop => slider.value = new Vector2(prop.floatValue, slider.value.y));
            slider.TrackPropertyValue(maxProp, prop => slider.value = new Vector2(slider.value.x, prop.floatValue));
            container.Add(label);
            container.Add(minField);
            container.Add(slider);
            container.Add(maxField);
            return container;
        }
    }
}