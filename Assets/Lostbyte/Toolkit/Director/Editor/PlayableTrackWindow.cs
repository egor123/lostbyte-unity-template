using Lostbyte.Toolkit.CustomEditor.Editor.Graphs;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Director.Editor
{
    public class PlayableTrackWindow : CustomGraphEditorWindow<PlayableTrackGraphView, PlayableTrack, PlayableTrackNodeView, PlayableTrackNode>
    {
        [MenuItem("Window/Playable Track Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<PlayableTrackWindow>();
            window.titleContent = new GUIContent("PlayableTrackEditor");
        }
    }
}
