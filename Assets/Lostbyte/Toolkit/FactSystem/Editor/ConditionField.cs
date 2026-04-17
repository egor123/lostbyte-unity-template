using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.FactSystem.Nodes;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class ConditionField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ConditionField, UxmlTraits> { }

        private TextField _textField;
        private Image _icon;
        private string _conditionText = string.Empty;
        private bool _hasErrors = false;
        private INode _parsedNode;

        public Condition Value
        {
            get
            {
                try
                {
                    return new((IBoolNode)ConditionParser.Parse(_conditionText));
                }
                catch (Exception)
                {
                    return new();
                }
            }
            set => SetValueWithoutNotify(value?.ToString());
        }

        public Label Label { get; private set; }
        public ConditionField()
        {
            style.flexDirection = FlexDirection.Row;

            Label = new Label(string.Empty)
            {
                style = {
                    width = 120,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            _textField = new TextField
            {
                style = {
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    minWidth = 100
                }
            };

            _icon = new Image
            {
                style = {
                    width = 18,
                    height = 18,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 2
                }
            };

            Add(Label);
            Add(_textField);
            Add(_icon);

            _textField.RegisterValueChangedCallback(evt =>
            {
                ParseCondition(evt.newValue);
            });

            ParseCondition(string.Empty);
        }

        private void ParseCondition(string text)
        {
            _conditionText = text;
            try
            {
                _parsedNode = ConditionParser.Parse(text);
                _hasErrors = false;
                _icon.image = EditorGUIUtility.IconContent("d_Valid@2x").image;
            }
            catch (Exception)
            {
                _hasErrors = true;
                _parsedNode = null;
                _icon.image = EditorGUIUtility.IconContent("d_Invalid@2x").image;
            }
        }

        public void SetValueWithoutNotify(string text)
        {
            _textField.SetValueWithoutNotify(text);
            ParseCondition(text);
        }

        public void BindProperty(SerializedProperty property)
        {
            var conditionProp = property.FindPropertyRelative("m_rootNode");
            if (conditionProp == null)
            {
                Debug.LogWarning("Could not bind ConditionField: m_rootNode not found.");
                return;
            }

            var node = conditionProp.managedReferenceValue as INode;
            var initial = node?.ToString() ?? "";
            SetValueWithoutNotify(initial);

            _textField.RegisterValueChangedCallback(evt =>
            {
                try
                {
                    conditionProp.serializedObject.Update();
                    conditionProp.managedReferenceValue = ConditionParser.Parse(evt.newValue);
                    conditionProp.serializedObject.ApplyModifiedProperties();
                }
                catch
                {
                    // Handled in ParseCondition
                }
            });
        }
    }
}
