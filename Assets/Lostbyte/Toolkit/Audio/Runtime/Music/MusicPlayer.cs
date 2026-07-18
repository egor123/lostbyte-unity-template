using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    public class MusicPlayer : MonoBehaviour
    {
        public MusicTrackData Track;
        public int Priority = 0;
        public TransitionSyncMode SyncMode = TransitionSyncMode.NextBar;
        public int FadeInBeats = 4;
        public int FadeOutBeats = 4;

        private void OnEnable() => MusicManager.RegisterPlayer(this);
        private void OnDisable() => MusicManager.UnregisterPlayer(this);
    }
}