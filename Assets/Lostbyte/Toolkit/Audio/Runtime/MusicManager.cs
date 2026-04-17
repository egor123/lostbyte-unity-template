using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField, Autowired(Autowired.Type.Parent, isForced: true), Hide] private KeyReference m_key;
        [SerializeField] private MusicSettings m_settings;
        [SerializeField] private AudioSource m_mainSource, m_transitionSource;
        [SerializeField, ReadOnly] private State _state;
        [SerializeField, ReadOnly] private float _time, _transitionTime;
        [SerializeField, ReadOnly] private int _currentIdx, _nextIdx;
        private List<MusicGroupRunner> _runners;
        private List<MusicGroupRunner> Runners => _runners ??= m_settings.Groups.Select(g => new MusicGroupRunner(g, m_key)).ToList();
        private enum State { Idle, Play, FadeOut, FadeIn };
        private void OnEnable()
        {
            m_mainSource.loop = false;
            m_transitionSource.loop = false;
            m_mainSource.volume = 0;
            _currentIdx = -1;
            _nextIdx = -1;
            _state = State.Idle;
            for (int i = 0; i < Runners.Count; i++)
            {
                var runner = Runners[i];
                runner.Enable();
                if (runner.IsReady)
                {
                    _currentIdx = i;
                    _time = 0;
                    _state = State.FadeIn;
                    break;
                }
            }
        }

        private void Update()
        {
            _time += Time.deltaTime;
            CheckForTransitions();
            Transition();
            Play();
        }
        private void CheckForTransitions()
        {
            if (_currentIdx != -1)
            {
                var current = Runners[_currentIdx];
                if (_time < current.Group.MinPlayTime) return;
                if (!current.IsReady)
                {
                    _nextIdx = -1;
                    _state = State.FadeOut;
                    PlayTransition();
                }
            }
            for (int i = 0; i < Runners.Count; i++)
            {
                if (Runners[i].IsReady && (i < _currentIdx || _currentIdx == -1) && i != _nextIdx)
                {
                    if (_currentIdx == -1)
                    {
                        m_mainSource.Stop();
                        _transitionTime = 0;
                        _currentIdx = i;
                        _nextIdx = -1;
                        _time = 0;
                        _state = State.FadeIn;
                    }
                    else
                    {
                        _nextIdx = i;
                        _state = State.FadeOut;
                        PlayTransition();
                        //TODO improve transition time
                    }
                    break;
                }
            }
        }
        private void PlayTransition()
        {
            var current = Runners[_currentIdx].Group;
            if (current.TransitionsOut.Length > 0 && !m_transitionSource.isPlaying) //TODO improve timing
            {
                m_transitionSource.volume = current.Volume;
                m_transitionSource.clip = current.TransitionsOut[UnityEngine.Random.Range(0, current.TransitionsOut.Length)];
                m_transitionSource.Play();
            }
        }
        private void Transition() //TODO crossfade
        {
            switch (_state)
            {
                case State.FadeOut:
                    var current = Runners[_currentIdx].Group;
                    var start = m_settings.FadeOutCurve[0].time;
                    var end = m_settings.FadeOutCurve[m_settings.FadeOutCurve.length - 1].time;
                    m_mainSource.volume = current.Volume * Mathf.Clamp01(m_settings.FadeOutCurve.Evaluate(start + (_transitionTime += Time.deltaTime) / current.FadeOutDuration * (end - start)));
                    if (_transitionTime > current.FadeOutDuration)
                    {
                        _transitionTime = 0f;
                        _time = 0;
                        _currentIdx = _nextIdx;
                        _state = _currentIdx == -1 ? State.Idle : State.FadeIn;
                        m_mainSource.clip = null;
                        m_mainSource.Stop();

                    }
                    break;
                case State.FadeIn:
                    current = Runners[_currentIdx].Group;
                    start = m_settings.FadeInCurve[0].time;
                    end = m_settings.FadeInCurve[m_settings.FadeInCurve.length - 1].time;
                    m_mainSource.volume = current.Volume * Mathf.Clamp01(m_settings.FadeInCurve.Evaluate(start + (_transitionTime += Time.deltaTime) / current.FadeInDuration * (end - start)));
                    if (_transitionTime > current.FadeInDuration)
                    {
                        _transitionTime = 0;
                        _state = State.Play;
                    }
                    break;
            }
        }
        private void Play()
        {
            if (_state != State.Idle && !m_mainSource.isPlaying)
            {
                var clips = Runners[_currentIdx].Group.Clips;
                m_mainSource.clip = clips.Length > 0 ? clips[UnityEngine.Random.Range(0, clips.Length)] : null;
                m_mainSource.Play();
            }
        }
        private void OnDisable()
        {
            Runners?.ForEach(r => r.Disable());
            if(m_mainSource) m_mainSource.Stop();
            if(m_transitionSource) m_transitionSource.Stop();
        }
    }
}
