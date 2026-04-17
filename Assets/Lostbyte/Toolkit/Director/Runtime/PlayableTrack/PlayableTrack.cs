using Lostbyte.Toolkit.Common;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    [CreateAssetMenu(fileName = nameof(PlayableTrack), menuName = "Director/Track")]
    public class PlayableTrack : ScriptableObject, IPlayableData, IInvokable
    {
        public Priority Priority = Priority.Default;
        [field: SerializeField] public OnContinueBehaviour OnContinueBehaviour { get; set; }
        public PlayableTrackNode StartNode;
        public Vector2 StartNodePosition;
        public InteruptBehaviour InteruptBehaviour => InteruptBehaviour.Continue;
        public float Offset => 0;
        public IPlayableClipBehaviour GetClip() => new PlayableTrackBehaviour(this);
        public void Invoke()
        {
            if (Director.Instance)
                Director.Instance.Schedule(this, Priority);
        }
    }
    public class PlayableTrackBehaviour : PlayableBehaviour<PlayableTrack>
    {
        private IPlayableClipNodeBehaviour _node;
        private IPlayableClipNodeBehaviour _checkpointNode;
        public CheckpointNode Checkpoint;
        public PlayableTrackBehaviour(PlayableTrack data) : base(data) { }

        public override bool IsFinished() => _checkpointNode == null && _node == null;

        public override void OnStart()
        {
            _node = Data.StartNode.GetClip(this);
        }
        public override void OnUpdate()
        {
            if (_checkpointNode != null)
            {
                UpdateNode(ref _checkpointNode);
                if (_checkpointNode == null && _node != null && _node.Time > 0)
                {
                    _node.OnContinue();
                }
            }
            else UpdateNode(ref _node);
        }
        private void UpdateNode(ref IPlayableClipNodeBehaviour node)
        {
            if (node.IsReady || node.Time > 0)
            {
                if (node.Time == 0) node.OnStart();
                node.Time += UnityEngine.Time.deltaTime;
                if (node.IsFinished)
                {
                    node.OnEnd();
                    node = node.GetNext(this);
                }
                else node.OnUpdate();
            }
        }
        public override void OnContinue()
        {
            if (Checkpoint)
            {
                _checkpointNode = Checkpoint.GetOnContinueClip(this);
                switch (Checkpoint.CheckpointBehaviour)
                {
                    case CheckpointBehaviour.Restart:
                        _node?.OnEnd();
                        _node = Checkpoint.GetClip(this);
                        break;
                    case CheckpointBehaviour.Continue:
                        if (_checkpointNode == null && _node.Time > 0)
                            _node?.OnContinue();
                        break;
                    case CheckpointBehaviour.Exit:
                        _node?.OnEnd();
                        _node = null;
                        break;
                    case CheckpointBehaviour.TrySkip:
                        _node?.OnEnd();
                        CheckpointNode currentCheckPoint = Checkpoint;
                        while (currentCheckPoint == Checkpoint && _node != null)
                            _node = _node.GetNext(this);
                        break;
                }
            }
            else
            {
                _checkpointNode = null;
                _node?.OnEnd();
                _node = null;
            }
        }

        public override void OnEnd()
        {
            _checkpointNode?.OnEnd();
            _checkpointNode = null;
            _node?.OnEnd();
            _node = null;
        }

        public override void OnPause()
        {
            _checkpointNode?.OnEnd();
            _checkpointNode = null;
            _node?.OnPause();
        }
    }

    public interface IPlayableClipNodeData
    {
        IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track);
    }
    public interface IPlayableClipNodeBehaviour
    {
        IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track);
        float Time { get; set; }
        bool IsReady { get; }
        bool IsFinished { get; }
        void OnStart();
        void OnEnd();
        void OnUpdate();
        void OnPause();
        void OnContinue();
    }

    public abstract class PlayableTrackNode : ScriptableObject, IPlayableClipNodeData
    {
        public Vector2 Position;
        public abstract IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track);
    }
    public abstract class PlayableClipNodeBehaviour<T> : IPlayableClipNodeBehaviour where T : PlayableTrackNode
    {
        public T Node { get; private set; }
        public PlayableTrackBehaviour Track { get; private set; }
        public PlayableClipNodeBehaviour(T node, PlayableTrackBehaviour track) => (Node, Track) = (node, track);
        public float Time { get; set; }
        public abstract bool IsReady { get; }
        public abstract bool IsFinished { get; }
        public abstract IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track);
        public abstract void OnContinue();
        public abstract void OnEnd();
        public abstract void OnUpdate();
        public abstract void OnPause();
        public abstract void OnStart();
    }
}
