using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.Audio.Music
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CutsceneAudioController : MonoBehaviour
    {
        [Header("Mixer Snapshots")]
        public AudioMixerSnapshot defaultSnapshot;
        public AudioMixerSnapshot cutsceneSnapshot;

        [Header("Transition Speeds")]
        public float enterFadeDuration = 0.5f;
        public float exitFadeDuration = 1.5f;

        private PlayableDirector director;

        private void Awake()
        {
            director = GetComponent<PlayableDirector>();
        }

        private void OnEnable()
        {
            if (director != null)
            {
                director.played += OnTimelinePlayed;
                director.stopped += OnTimelineStopped;
            }
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.played -= OnTimelinePlayed;
                director.stopped -= OnTimelineStopped;
            }
        }

        private void OnTimelinePlayed(PlayableDirector obj)
        {
            EnterCutscene(enterFadeDuration);
        }

        private void OnTimelineStopped(PlayableDirector obj)
        {
            ExitCutscene(exitFadeDuration);
        }

        public void EnterCutscene(float fadeDuration = 1.0f)
        {
            if (cutsceneSnapshot != null)
                cutsceneSnapshot.TransitionTo(fadeDuration);
        }

        public void ExitCutscene(float fadeDuration = 1.5f)
        {
            if (defaultSnapshot != null)
                defaultSnapshot.TransitionTo(fadeDuration);
        }
    }
}