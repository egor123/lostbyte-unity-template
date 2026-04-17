using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Localization.Editor
{
    [CustomEditor(typeof(LocalizationDatabase))]
    public class LocalizationDatabaseEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var entries = serializedObject.FindProperty("m_tables");

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
                        var field = new ObjectField();
                        field.SetEnabled(false);
                        field.style.flexGrow = 1;
                        return field;
                    },
                bindItem = (element, index) =>
                    {
                        var itemProp = entries.GetArrayElementAtIndex(index);
                        var value = itemProp.objectReferenceValue as LocalizedTable;
                        ((ObjectField)element).label = value.name;
                        ((ObjectField)element).value = value;
                    }
            };
            root.Add(listView);
            root.Bind(serializedObject);
            return root;
        }
    }
}
