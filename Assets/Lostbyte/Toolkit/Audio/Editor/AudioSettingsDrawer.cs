using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.Audio.Editor
{
    [CustomPropertyDrawer(typeof(AudioSettings))]
    public class AudioSettingsDrawer : PropertyDrawer
    {
        private SerializedProperty GetProp(SerializedProperty property, string propName)
            => property.FindPropertyRelative($"<{propName}>k__BackingField");

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var nameProp = GetProp(property, nameof(AudioSettings.Name));
            var clipsProp = GetProp(property, nameof(AudioSettings.Clips));
            var priorityProp = GetProp(property, nameof(AudioSettings.Priority));
            var stopActiveProp = GetProp(property, nameof(AudioSettings.StopActive));
            var allowMultipleProp = GetProp(property, nameof(AudioSettings.AllowMultiple));
            var minVolProp = GetProp(property, nameof(AudioSettings.MinVolume));
            var maxVolProp = GetProp(property, nameof(AudioSettings.MaxVolume));
            var minPitchProp = GetProp(property, nameof(AudioSettings.MinPitch));
            var maxPitchProp = GetProp(property, nameof(AudioSettings.MaxPitch));
            var triggerProp = GetProp(property, nameof(AudioSettings.Trigger));

            var rootFoldout = new Foldout
            {
                viewDataKey = $"AudioSettingsDrawer_{property.propertyPath}_Main",
                value = true
            }.ClearPaddingAndMargin();

            rootFoldout.Q(className: "unity-foldout__content")?.SetMargin(0, 0, 0, 10);
            var mainToggle = rootFoldout.Q<Toggle>();
            if (mainToggle != null)
            {
                mainToggle.Q<Label>()?.Hide();

                var nameField = new TextField { bindingPath = nameProp.propertyPath }.ClearPaddingAndMargin().SetFlex(100, 0);
                nameField.style.unityFontStyleAndWeight = FontStyle.Bold;
                var textInput = nameField.Q(className: "unity-text-element");
                if (textInput != null) textInput.style.marginLeft = 0;
                nameField.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
                nameField.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
                mainToggle.Add(nameField);
            }

            var playbackCard = CreateTogglableCard("Playback Settings", property.propertyPath);
            playbackCard.Add(CreateCompactProperty(priorityProp));

            var boolRow = new VisualElement();
            boolRow.AddToClassList("unity-base-field");
            boolRow.AddToClassList("unity-base-field__aligned");

            var boolLabel = new Label("Behaviors");
            boolLabel.AddToClassList("unity-base-field__label");

            var togglesContainer = new VisualElement().MakeRow().SetFlex(1, 0);
            var stopActiveToggle = new Toggle("Stop Active") { bindingPath = stopActiveProp.propertyPath, style = { flexGrow = 1, marginLeft = 0, marginRight = 10 } };
            var allowMultipleToggle = new Toggle("Allow Multiple") { bindingPath = allowMultipleProp.propertyPath, style = { flexGrow = 1, marginLeft = 0 } };

            togglesContainer.Add(stopActiveToggle);
            togglesContainer.Add(allowMultipleToggle);

            boolRow.Add(boolLabel);
            boolRow.Add(togglesContainer);
            playbackCard.Add(boolRow);

            playbackCard.Add(CreateMinMaxSlider("Volume", minVolProp, maxVolProp, 0f, 1f));
            playbackCard.Add(CreateMinMaxSlider("Pitch", minPitchProp, maxPitchProp, 0f, 3f));

            playbackCard.Add(CreateCompactProperty(triggerProp));


            var clipsCard = CreateTogglableCard("Audio Clips", property.propertyPath);
            var listView = new ListView
            {
                bindingPath = clipsProp.propertyPath,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = false,
                showBorder = false,
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = { marginTop = 0, flexGrow = 1 },
                makeItem = () => new PropertyField { style = { marginTop = 0, marginBottom = 0, paddingRight = 0 } },
                bindItem = (element, i) =>
                {
                    var propField = (PropertyField)element;
                    propField.BindProperty(clipsProp.GetArrayElementAtIndex(i));
                    propField.label = string.Empty;
                }
            };
            clipsCard.Add(listView);
            rootFoldout.Add(clipsCard);
            rootFoldout.Add(playbackCard);

            return rootFoldout;
        }

        private void ApplyCardStyle(VisualElement element)
        {
            element.SetBackgroundColor(new Color(0f, 0f, 0f, 0.1f))
                .SetBorderRadius(4)
                .SetPadding(2, 3, 0)
                .SetMargin(0, 0, 3, 0);
        }

        private Foldout CreateTogglableCard(string title, string propertyPath, bool defaultState = true)
        {
            var foldout = new Foldout
            {
                text = title,
                value = defaultState,
                viewDataKey = $"AudioSettingsDrawer_{propertyPath}_{title.Replace(" ", "")}"
            };

            ApplyCardStyle(foldout);
            var toggle = foldout.Q<Toggle>();
            if (toggle != null)
            {
                toggle.SetMargin(2, 0).SetPadding(0, 0, 2);
                toggle.style.borderBottomWidth = 1;
                toggle.style.borderBottomColor = new Color(0f, 0f, 0f, 0.2f);
                var label = toggle.Q<Label>();
                if (label != null) label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            return foldout;
        }

        private VisualElement CreateCompactProperty(SerializedProperty prop)
            => new PropertyField(prop) { style = { marginBottom = 0, marginTop = 0 } };

        private VisualElement CreateMinMaxSlider(string labelText, SerializedProperty minProp, SerializedProperty maxProp, float minLimit, float maxLimit)
        {
            var container = new VisualElement();
            container.AddToClassList("unity-base-field");
            container.AddToClassList("unity-base-field__aligned");

            var label = new Label(labelText);
            label.AddToClassList("unity-base-field__label");

            var inputContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };

            var minField = new FloatField { bindingPath = minProp.propertyPath, style = { width = 45, flexShrink = 0, marginLeft = 0 } };
            var maxField = new FloatField { bindingPath = maxProp.propertyPath, style = { width = 45, flexShrink = 0 } };
            var slider = new MinMaxSlider(minProp.floatValue, maxProp.floatValue, minLimit, maxLimit) { style = { flexGrow = 1, paddingLeft = 4, paddingRight = 4 } };

            slider.RegisterValueChangedCallback(evt =>
            {
                minProp.floatValue = (float)System.Math.Round(evt.newValue.x, 2);
                maxProp.floatValue = (float)System.Math.Round(evt.newValue.y, 2);
                minProp.serializedObject.ApplyModifiedProperties();
            });

            slider.TrackPropertyValue(minProp, prop => slider.value = new Vector2(prop.floatValue, slider.value.y));
            slider.TrackPropertyValue(maxProp, prop => slider.value = new Vector2(slider.value.x, prop.floatValue));

            inputContainer.Add(minField);
            inputContainer.Add(slider);
            inputContainer.Add(maxField);

            container.Add(label);
            container.Add(inputContainer);

            return container;
        }
    }
}