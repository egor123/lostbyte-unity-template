using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Localization.Editor
{
    [UnityEditor.CustomEditor(typeof(LocalizedTable))]
    public class LocalizedTableEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var fallback = serializedObject.FindProperty("m_fallback");
            var entries = serializedObject.FindProperty("m_entries");

            var fallbackField = new ObjectField("Fallback Locale") { bindingPath = fallback.propertyPath };
            fallbackField.SetEnabled(false);
            root.Add(fallbackField);

            var listView = new ListView
            {
                reorderable = false,
                selectionType = SelectionType.None,
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                bindingPath = entries.propertyPath,
                makeItem = () =>
                    {
                        var field = new TextField() { isReadOnly = true };
                        field.style.flexGrow = 1;
                        return field;
                    },
                bindItem = (element, index) =>
                    {
                        var itemProp = entries.GetArrayElementAtIndex(index);
                        var value = itemProp.boxedValue as LocalizedTable.StringEntry;
                        ((TextField)element).label = value.Key;
                        ((TextField)element).value = value.Value;
                    }
            };
            root.Add(listView);
            root.Bind(serializedObject);
            return root;
        }
    }
}
