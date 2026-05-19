using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class KeyFactField : BindableElement
    {
        private Label m_LabelElement;
        private Button m_SelectorButton;

        private SerializedProperty m_KeyProp;
        private SerializedProperty m_FactProp;
        private Type m_FactArgType = typeof(FactDefinition);

        public KeyFactField(string label = null)
        {
            AddToClassList("unity-base-field");

            m_LabelElement = new Label(label);
            m_LabelElement.AddToClassList("unity-base-field__label");
            m_LabelElement.AddToClassList("unity-property-field__label");
            m_LabelElement.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;

            m_SelectorButton = new Button(OnSelectorClicked)
            {
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    textOverflow = TextOverflow.Ellipsis,
                    overflow = Overflow.Hidden,
                    marginRight = 0,
                    marginLeft = 0
                }
            };

            Add(m_LabelElement);
            Add(m_SelectorButton);
        }

        public void BindToProperties(SerializedProperty keyProp, SerializedProperty factProp, Type factArgType = null)
        {
            m_KeyProp = keyProp;
            m_FactProp = factProp;
            m_FactArgType = factArgType ?? GetFactValueType(m_FactProp.GetTargetType()); // FIXME make better arg extractor

            this.TrackPropertyValue(m_KeyProp, _ => UpdateText());
            this.TrackPropertyValue(m_FactProp, _ => UpdateText());

            UpdateText();
        }
        public static Type GetFactValueType(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FactDefinition<>))
                    return type.GetGenericArguments()[0];
                type = type.BaseType;
            }
            return null;
        }
        private void UpdateText()
        {
            if (m_KeyProp == null || m_FactProp == null) return;
            string keyName = m_KeyProp.objectReferenceValue != null ? m_KeyProp.objectReferenceValue.name : "this";
            string factName = m_FactProp.objectReferenceValue != null ? m_FactProp.objectReferenceValue.name : "None";
            m_SelectorButton.text = $"{keyName}[{factName}]";
        }

        private void OnSelectorClicked()
        {
            var dropdown = new KeyFactSearchDropdown(new AdvancedDropdownState(), m_FactArgType);
            dropdown.OnItemSelected += (key, fact) =>
            {
                if (m_KeyProp != null && m_FactProp != null)
                {
                    m_KeyProp.objectReferenceValue = key;
                    m_FactProp.objectReferenceValue = fact;
                    m_KeyProp.serializedObject.ApplyModifiedProperties();
                }
            };
            dropdown.Show(m_SelectorButton.worldBound);
        }


        private class KeyFactSearchDropdown : AdvancedDropdown
        {
            public event Action<ScriptableObject, ScriptableObject> OnItemSelected;
            private readonly Type m_FactArgType;

            public KeyFactSearchDropdown(AdvancedDropdownState state, Type factTypeLimit) : base(state)
            {
                m_FactArgType = factTypeLimit;
                minimumSize = new Vector2(250, 300);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Select Key & Fact");

                var allFacts = FactDatabase.Instance.FactStorage
                    .Where(f => m_FactArgType == null || f.GenericType == m_FactArgType)
                    .ToList();

                var thisGroup = new AdvancedDropdownItem("this (Local)");
                foreach (var fact in allFacts)
                    thisGroup.AddChild(new KeyFactDropdownItem($"this[{fact.name}]", null, fact));

                if (thisGroup.children.Any())
                    root.AddChild(thisGroup);

                foreach (var rootKey in FactDatabase.Instance.RootKeys)
                {
                    var keyNode = BuildKeyHierarchy(rootKey, allFacts);

                    if (keyNode != null && keyNode.children.Any())
                        root.AddChild(keyNode);
                }

                return root;
            }

            private AdvancedDropdownItem BuildKeyHierarchy(ScriptableObject key, List<FactDefinition> allFacts)
            {
                if (key == null) return null;

                var keyGroup = new AdvancedDropdownItem(key.name);

                var savedFacts = GetSavedFactsFromKey(key);
                var otherFacts = allFacts.Except(savedFacts).ToList();

                foreach (var fact in savedFacts)
                    keyGroup.AddChild(new KeyFactDropdownItem($"{key.name}[{fact.name}]", key, fact));

                if (savedFacts.Any() && otherFacts.Any())
                    keyGroup.AddChild(new AdvancedDropdownItem("   ------   ") { enabled = false });

                foreach (var fact in otherFacts)
                    keyGroup.AddChild(new KeyFactDropdownItem($"{key.name}[{fact.name}]", key, fact));

                var children = GetChildrenFromKey(key);
                foreach (var childKey in children)
                {
                    var childNode = BuildKeyHierarchy(childKey, allFacts);
                    if (childNode != null && childNode.children.Any())
                        keyGroup.AddChild(childNode);
                }
                return keyGroup;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is KeyFactDropdownItem kfItem)
                    OnItemSelected?.Invoke(kfItem.Key, kfItem.Fact);
            }

            private List<ScriptableObject> GetSavedFactsFromKey(ScriptableObject key)
            {
                var savedFacts = new List<ScriptableObject>();
                if (key == null) return savedFacts;

                var property = key.GetType().GetProperty("Facts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null && property.GetValue(key) is IList list)
                    foreach (var item in list)
                        if (item is ScriptableObject so) savedFacts.Add(so);

                return savedFacts;
            }

            private List<ScriptableObject> GetChildrenFromKey(ScriptableObject key)
            {
                var childrenList = new List<ScriptableObject>();
                if (key == null) return childrenList;

                var property = key.GetType().GetProperty("Children", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null && property.GetValue(key) is IEnumerable list)
                    foreach (var item in list)
                        if (item is ScriptableObject so) childrenList.Add(so);

                return childrenList;
            }
        }
        private class KeyFactDropdownItem : AdvancedDropdownItem
        {
            public ScriptableObject Key { get; }
            public ScriptableObject Fact { get; }

            public KeyFactDropdownItem(string name, ScriptableObject key, ScriptableObject fact) : base(name)
            {
                Key = key;
                Fact = fact;
            }
        }
    }
}
