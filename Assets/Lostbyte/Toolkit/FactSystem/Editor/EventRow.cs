using System;
using System.Linq;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class EventRow : VisualElement
    {
        public EventDefinition Event { get; private set; }
        public KeyContainer Key { get; private set; }
        private readonly SerializedObject _so;
        private Button _reactionIndicator;
        private Label _reactionCountLabel;

        public EventRow(EventDefinition @event, KeyContainer key)
        {
            Key = key;
            Event = @event;

            _so = new SerializedObject(@event);

            this.MakeRow().SetAlignItems(Align.Center).SetFlex(1, 0);

            AddNameField();
            AddBtn();
            AddReactionIndicator();

            UpdateBindings();

            if (_so != null)
            {
                var tracker = new VisualElement { name = "Tracker" }.Hide();
                Add(tracker);
                tracker.TrackSerializedObjectValue(_so, so => UpdateBindings());
            }
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
            field.SetFlex(grow, 0).SetFlexBasis(0).SetPadding(0).SetMargin(0, 4, 0, 0);
            Add(field);
        }
        private void UpdateBindings()
        {
            if (_so == null) return;
            _so.Update();
            if (_reactionIndicator != null && Key != null)
            {
                int index = GetRegistrationIndex();
                UpdateIndicatorUI(index < 0 || Key.EventRegistrations[index].Reactions == null ? 0 : Key.EventRegistrations[index].Reactions.Count);
            }
        }
        private int GetRegistrationIndex() => Key?.EventRegistrations?.FindIndex(r => r.Event == Event) ?? -1;

        private void AddReactionIndicator()
        {
            if (Key == null) return;

            _reactionIndicator = new Button()
                .SetEnabledState(false).MakeRow().SetAlignItems(Align.Center).SetJustifyContent(Justify.Center)
                .SetPadding(2, 4).SetMargin(0).SetTooltip("Right-click to manage reactions")
                .SetMinSize(30, 20);

            var icon = new Image { image = EditorGUIUtility.IconContent("d_EventSystem Icon").image }.SetSize(14, 14).SetMargin(0, 2, 0, 0);
            _reactionCountLabel = new Label("0").SetFontStyle(FontStyle.Normal);

            _reactionIndicator.Add(icon);
            _reactionIndicator.Add(_reactionCountLabel);

            AddColumnField(_reactionIndicator, 0f);
        }
        private void UpdateIndicatorUI(int count)
        {
            if (_reactionCountLabel == null || _reactionIndicator == null) return;
            _reactionCountLabel.text = count.ToString();
            _reactionIndicator.SetOpacity(count > 0 ? 1f : 0.4f);
        }
    }
}