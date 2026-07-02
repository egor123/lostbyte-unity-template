using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(EventRegistration))]
    public class EventRegistrationDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var card = new VisualElement()
                .SetFlex(0, 0)
                .SetBorder(1, new Color(0.15f, 0.15f, 0.15f))
                .SetPadding(8)
                .SetMargin(0, 0, 6, 0)
                .SetBackgroundColor(new Color(0.2f, 0.2f, 0.2f, 0.3f));

            var eventProp = property.FindPropertyRelative(nameof(EventRegistration.Event));
            var reactionsProp = property.FindPropertyRelative(nameof(EventRegistration.Reactions));
            card.Add(new PropertyField(eventProp).SetEnabledState(false));
            DrawReactionsSection(card, property, eventProp, reactionsProp);
            root.Add(card);
            return root;
        }

        private void DrawFlattenedProperties(SerializedProperty parentProp, VisualElement container)
        {
            var field = new PropertyField(parentProp, string.Empty);
            field.BindProperty(parentProp);

            field.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var foldout = field.Q<Foldout>();
                if (foldout != null)
                {
                    foldout.Q<Toggle>()?.Hide();
                    foldout.style.marginLeft = 0;
                    foldout.style.paddingLeft = 0;
                    foldout.Q("unity-content")?.ClearPaddingAndMargin();
                }
                field.ClearPaddingAndMargin();
            });
            container.Add(field);
        }

        private void DrawReactionsSection(VisualElement root, SerializedProperty parentProp, SerializedProperty eventProp, SerializedProperty reactionsProp)
        {
            var container = new VisualElement()
                .SetMargin(8, 0, 0, 0)
                .SetBorderWidth(1, 0, 0, 0)
                .SetBorderColor(new Color(0.15f, 0.15f, 0.15f))
                .SetPadding(6, 0, 0, 0);

            var headerRow = new VisualElement()
                .MakeRow(Align.Center, Justify.SpaceBetween)
                .SetMargin(0, 0, 6, 0);

            var listContainer = new VisualElement();
            container.Add(listContainer);

            var addBtn = new Button(() =>
            {
                var @event = eventProp.objectReferenceValue as EventDefinition;
                if (@event == null)
                {
                    Debug.LogWarning("You must assign an Event before adding reactions.");
                    return;
                }
                FactReactionMenuBuilder.ShowAddReactionMenu(container, @event, (selectedType) =>
                {
                    reactionsProp.arraySize++;
                    var newElem = reactionsProp.GetArrayElementAtIndex(reactionsProp.arraySize - 1);
                    newElem.managedReferenceValue = Activator.CreateInstance(selectedType);
                    newElem.isExpanded = true;
                    parentProp.serializedObject.ApplyModifiedProperties();
                    RebuildList();
                });
            })
            { text = "+ Add Reaction" };

            headerRow.Add(addBtn);
            container.Add(headerRow);

            void RebuildList()
            {
                listContainer.Clear();

                if (reactionsProp.arraySize == 0)
                {
                    listContainer.Add(new Label("No reactions configured.")
                        .SetOpacity(0.5f)
                        .SetMargin(0, 0, 5, 0));
                }

                for (int i = 0; i < reactionsProp.arraySize; i++)
                {
                    int index = i;
                    var elementProp = reactionsProp.GetArrayElementAtIndex(index);

                    string cleanName = elementProp.managedReferenceValue != null
                        ? ObjectNames.NicifyVariableName(elementProp.managedReferenceValue.GetType().Name.Replace("Reaction", ""))
                        : "Empty Reaction";

                    var itemFoldout = new Foldout { text = cleanName }
                        .SetBackgroundColor(new Color(0, 0, 0, 0.1f))
                        .SetBorderWidth(0, 0, 1, 0)
                        .SetBorderColor(new Color(0.15f, 0.15f, 0.15f))
                        .SetPadding(2, 0, 4, 0)
                        .SetMargin(0, 0, 2, 0);

                    itemFoldout.value = elementProp.isExpanded;
                    itemFoldout.RegisterValueChangedCallback(evt => { elementProp.isExpanded = evt.newValue; });

                    var headerToggle = itemFoldout.Q<Toggle>();
                    if (headerToggle != null)
                    {
                        headerToggle.Add(new VisualElement().SetFlex(1, 0));

                        var upBtn = new Button(() =>
                        {
                            reactionsProp.MoveArrayElement(index, index - 1);
                            parentProp.serializedObject.ApplyModifiedProperties();
                            RebuildList();
                        })
                        { text = "↑" }.SetSize(20, 20).SetPadding(0).SetEnabledState(index > 0);

                        var downBtn = new Button(() =>
                        {
                            reactionsProp.MoveArrayElement(index, index + 1);
                            parentProp.serializedObject.ApplyModifiedProperties();
                            RebuildList();
                        })
                        { text = "↓" }.SetSize(20, 20).SetPadding(0).SetEnabledState(index < reactionsProp.arraySize - 1);

                        var removeBtn = new Button(() =>
                        {
                            reactionsProp.DeleteArrayElementAtIndex(index);
                            parentProp.serializedObject.ApplyModifiedProperties();
                            RebuildList();
                        })
                        { text = "x" }.SetSize(20, 20).SetPadding(0).SetMargin(0, 0, 0, 4).SetTooltip("Remove");

                        headerToggle.Add(upBtn);
                        headerToggle.Add(downBtn);
                        headerToggle.Add(removeBtn);
                    }

                    DrawFlattenedProperties(elementProp, itemFoldout);
                    listContainer.Add(itemFoldout);
                }
            }
            RebuildList();
            root.Add(container);
        }
    }
}