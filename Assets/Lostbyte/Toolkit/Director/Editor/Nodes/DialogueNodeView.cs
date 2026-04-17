using System;
using System.Collections.Generic;
using System.Linq;
using Core.CustomEditor.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Director.Editor
{
    [NodeType(typeof(DialogueNode), "Dialogue Node")]
    public class DialogueNodeView : PlayableTrackNodeView
    {
        private readonly ObjectField _actorField;
        private readonly VisualElement paragraphContainer;
        public DialogueNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {
            // contentContainer.Add(_preDelayField = new("PreDelay") { value = 0.5f });
            // contentContainer.Add(_floatField = new("Duration") { value = 2 });
            // contentContainer.Add(_textField = new() { multiline = true });
            contentContainer.Add(_actorField = new() { label = "Actor", objectType = typeof(ScriptableObject), value = null });

            contentContainer.Add(new Button(() => AddParagraph()) { text = "Add Paragraph" });
            contentContainer.Add(paragraphContainer = new() { });


            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PlayableTrackNode));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PlayableTrackNode));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }
        private void AddParagraph(float pause = 0.5f, float duration = 2f, string table = "", string entry = "")
        {
            string key = $"{table}/{entry}";
            var container = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            var innerContainer = new VisualElement() { style = { flexDirection = FlexDirection.Column, flexGrow = 1 } };
            container.Add(new Button(() => paragraphContainer.Remove(container)) { text = "X" });
            container.Add(innerContainer);

            var pauseField = new FloatField("Pause") { value = pause };
            var durationField = new FloatField("Duration") { value = duration };
            var stringField = new TextField("Key") { value = key };
            var stringValueField = new Label(GetStringValue(key)) { style = { backgroundColor = new Color(0.1f, 0.1f, 0.1f), borderBottomLeftRadius = 5, borderBottomRightRadius = 5, borderTopLeftRadius = 5, borderTopRightRadius = 5 } };
            stringField.RegisterValueChangedCallback((evt) => { stringValueField.text = GetStringValue(evt.newValue); });

            innerContainer.Add(pauseField);
            innerContainer.Add(durationField);
            innerContainer.Add(stringField);
            innerContainer.Add(stringValueField);

            paragraphContainer.Add(container);

        }

        private string GetStringValue(string key)
        {
            var pair = key.Split('/');
            if (key != "/" && pair.Length == 2)
                return LocalizationSettings.StringDatabase.GetLocalizedString(pair[0], pair[1], LocalizationSettings.ProjectLocale) ?? "Null";
            return "Null";

        }

        public override void Load()
        {
            _actorField.value = (Node as DialogueNode).Actor;
            (Node as DialogueNode).Paragraphs.ForEach(p => AddParagraph(p.Pause, p.Duration, p.String?.TableReference.TableCollectionName, p.String?.TableEntryReference.Key));
            Graph.Connect(OutputPort, Graph.GetNodeView((Node as DialogueNode).NextNode)?.InputPort);
        }

        public override void Save()
        {
            (Node as DialogueNode).Actor = (ScriptableObject)_actorField.value;
            (Node as DialogueNode).Paragraphs = paragraphContainer.Children().Select(c =>
            {
                var e = c.ElementAt(1);
                var p = (e.ElementAt(2) as TextField).value.Split('/');
                return new DialogueNode.Paragraph()
                {
                    Pause = (e.ElementAt(0) as FloatField).value,
                    Duration = (e.ElementAt(1) as FloatField).value,
                    String = p.Length == 2 ? new(p[0], p[1]) : new()
                };
            }
            ).ToList();
            // (Node as DialogueNode).Paragraphs = _keys.Select(t => new DialogueNode.Paragraph() { Pause = t.Item1, Duration = t.Item2, String = new(t.Item3, t.Item4) }).ToList();
            (Node as DialogueNode).NextNode = OutputPort.connected ? ((PlayableTrackNodeView)OutputPort.connections.First()?.input.node).Node : null;
        }
    }
}
