using Lostbyte.Toolkit.Audio.Music;
using Lostbyte.Toolkit.CustomEditor.Editor.Graphs;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Editor
{
    public class MusicTrackWindow : CustomGraphEditorWindow<MusicTrackGraphView, MusicTrackData, MusicTrackNodeView, MusicTrackNode>
    {
        [MenuItem("Window/Music Track Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<MusicTrackWindow>();
            window.titleContent = new GUIContent("MusicTrackEditor");
        }
    }
}
