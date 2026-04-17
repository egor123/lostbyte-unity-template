using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Lostbyte.Toolkit.Director
{
    public class DialogueNode : PlayableTrackNode
    {
        public ScriptableObject Actor;
        public PlayableTrackNode NextNode;
        public List<Paragraph> Paragraphs = new();
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new DialogueNodeBehaviour(this, Actor, track);
        [Serializable]
        public struct Paragraph
        {
            public float Pause;
            public float Duration;
            public LocalizedString String;
        }
        
    }
    public class DialogueNodeBehaviour : PlayableClipNodeBehaviour<DialogueNode>
    {
        private ScriptableObject _actor;
        private bool _isSet;
        private int _idx = 0;
        public DialogueNodeBehaviour(DialogueNode node, ScriptableObject actor, PlayableTrackBehaviour track) : base(node, track) => _actor = actor;
        public override bool IsReady => true;
        public override bool IsFinished => _idx >= Node.Paragraphs.Count;
        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track) => Node.NextNode ? Node.NextNode.GetClip(track) : null;
        public override void OnStart() { }
        public override void OnContinue() => _isSet = false;
        public override void OnEnd()
        {
            _idx = 0;
            _isSet = false;
            SubtitlesManager.Instance.Clear();

        }
        public override void OnPause()
        {
            Time = 0;
            _isSet = false;
            SubtitlesManager.Instance.Clear();
        }
        public override void OnUpdate()
        {
            var paragraph = Node.Paragraphs[_idx];
            if (Time > paragraph.Pause && !_isSet)
            {
                _isSet = true;
                SubtitlesManager.Instance.Set(_actor, paragraph.String.TableReference, paragraph.String.TableEntryReference, paragraph.Duration);
            }
            if (Time > paragraph.Pause + paragraph.Duration)
            {
                _isSet = false;
                Time = 0;
                _idx++;
                SubtitlesManager.Instance.Clear();
            }
        }
    }
}
