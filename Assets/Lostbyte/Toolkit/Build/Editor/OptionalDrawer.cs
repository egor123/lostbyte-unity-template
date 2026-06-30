using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.Common;

namespace Lostbyte.Toolkit.Editor
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            var hasValueProp = property.FindPropertyRelative("m_hasValue");
            var valueProp = property.FindPropertyRelative("m_value");
            var toggle = new Toggle();
            toggle.BindProperty(hasValueProp);
            toggle.style.alignSelf = Align.Center;
            toggle.style.marginRight = 2;
            var valueField = new PropertyField(valueProp);
            valueField.style.flexGrow = 1;
            valueField.label = property.displayName;
            valueField.SetEnabled(hasValueProp.boolValue);
            toggle.RegisterValueChangedCallback(evt => valueField.SetEnabled(evt.newValue));
            container.Add(toggle);
            container.Add(valueField);
            return container;
        }
    }
}