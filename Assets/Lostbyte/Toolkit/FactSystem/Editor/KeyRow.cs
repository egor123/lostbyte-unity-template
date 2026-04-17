using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class KeyRow : VisualElement
    {
        public KeyContainer Key { get; private set; }
        private readonly SerializedObject _so;
        public KeyRow(KeyContainer key)
        {
            Key = key;

            focusable = false;
            pickingMode = PickingMode.Ignore;
            style.flexDirection = FlexDirection.Row;
            _so = new SerializedObject(key);
            AddNameField();
            AddSerializationField();
        }
        private void AddNameField()
        {
            TextField nameField = new()
            {
                label = "Key",
                value = Key.name,
            };
            nameField.RegisterCallback<FocusOutEvent>((evt) =>
                 {
                     if (nameField.value == Key.name) return;
                     if (!FactEditorUtils.ValidateIdentifier(nameField.value)) nameField.value = Key.name;
                     else
                     {
                         Key.name = nameField.value;
                         EditorUtility.SetDirty(FactEditorUtils.Database);
                         AssetDatabase.SaveAssets();
                     }
                 });
            Add(nameField);
        }
        private void AddSerializationField()
        {
            var prop = _so.FindProperty($"<{nameof(KeyContainer.IsSerializable)}>k__BackingField");


            var toggle = new Toggle();
            toggle.SetEnabled(!Application.isPlaying);
            toggle.BindProperty(prop);
            var icon = new Image
            {
                image = EditorGUIUtility.IconContent(toggle.value ? "SaveAs" : "CrossIcon").image,
                tooltip = "Toggles save serialization"
            };
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginLeft = 0;
            toggle.RegisterValueChangedCallback(evt =>
            {
                icon.image = EditorGUIUtility.IconContent(evt.newValue ? "SaveAs" : "CrossIcon").image;
            });
            toggle.RemoveAt(0);
            toggle.Add(icon);

            Add(toggle);
        }
    }
}