using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class EventRow : VisualElement
    {
        public EventDefinition Event { get; private set; }
        public KeyContainer Key { get; private set; }
        private readonly SerializedObject _so;
        public EventRow(EventDefinition @event, KeyContainer key)
        {
            Key = key;

            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;
            style.alignItems = Align.Center;

            Event = @event;
            style.flexDirection = FlexDirection.Row;
            _so = new SerializedObject(@event);
            AddNameField();
            AddBtn();
        }
        private void AddNameField()
        {
            TextField nameField = new()
            {
                label = "Event",
                value = Event.name,
            };
            nameField.RegisterCallback<FocusOutEvent>((evt) =>
                 {
                     if (nameField.value == Event.name) return;
                     if (!FactEditorUtils.ValidateIdentifier(nameField.value)) nameField.value = Event.name;
                     else
                     {
                         Event.name = nameField.value;
                         EditorUtility.SetDirty(FactEditorUtils.Database);
                         AssetDatabase.SaveAssets();
                     }
                 });
            AddColumnField(nameField, 0.396f);
        }
        private void AddBtn()
        {
            var btn = new Button { text = "Raise" };
            btn.SetEnabled(Application.isPlaying && Key != null);
            btn.clickable.clicked += () => Key.Raise(Event);
            AddColumnField(btn, 0.601f);
        }
        private void AddColumnField(VisualElement field, float grow = 1f)
        {
            field.style.flexGrow = grow;
            field.style.flexShrink = 0;
            field.style.flexBasis = 0;
            field.style.paddingRight = 4;
            Add(field);
        }
    }
}