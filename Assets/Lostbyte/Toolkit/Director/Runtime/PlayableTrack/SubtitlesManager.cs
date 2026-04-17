using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Rendering;

namespace Lostbyte.Toolkit.Director
{
    public abstract class SubtitlesManager : MonoBehaviour
    {
        public static SubtitlesManager Instance { get; private set; }
        // [SerializeField] private TMP_Text m_tmp;
        // [SerializeField] private float m_charTypingDuration = 0.1f;
        // [SerializeField, Range(0f, 1f)] private float m_maxTypingDuration = 0.8f;
        // [SerializeField] private float m_fadeOut = 0.15f;
        // [SerializeField] private AudioSource m_source;
        // [SerializeField] private List<AudioClip> m_clips = new();
        // [SerializeField, Range(0, 1f)] private float m_pitchVariation = 0.1f;
        // public UnityEvent OnTypeEvent;

        public ScriptableObject CurentActor { get; protected set; }
        public string CurrentText { get; protected set; }
        public float CurrentDuration { get; protected set; }
        public float Time { get; private set; }
        protected virtual void Awake()
        {
            Instance = this;
            Clear();
        }
        public virtual void Clear()
        {
            CurentActor = null;
            CurrentText = null;
            CurrentDuration = 0;
            Time = 0;
        }
        public abstract void Set(ScriptableObject actor, string table, string entry, float duration);
        public abstract void SetFrame(ScriptableObject actor, string value, float time, float duration);
        protected virtual void Update()
        {
            if (CurrentText != null)
            {
                Time = Mathf.Min(Time + UnityEngine.Time.deltaTime, CurrentDuration);
                SetFrame(CurentActor, CurrentText, Time, CurrentDuration);
                if (Time >= CurrentDuration) Clear();
            }
        }
        // private LocalizedString _localizedString;
        // private string _text = null;
        // private float _progress;
        // private float _d;
        // private float _duration;
        // public void Set(LocalizedString text, float duration)
        // {
        //     _progress = 0;
        //     _localizedString = text;
        //     _d = _duration = duration;
        //     _localizedString.StringChanged += UpdateText;
        // }
        // public void SetFrame(string text, float time, float duration)
        // {
        //     Clear();
        //     SetValues(text, time, duration);
        // }
        // private void SetValues(string text, float time, float duration)
        // {
        //     var d = Mathf.Min(duration * m_maxTypingDuration, text.Length * m_charTypingDuration);
        //     if (time < d)
        //     {
        //         OnTypeEvent?.Invoke();
        //         m_tmp.text = text[..Mathf.CeilToInt(text.Length * Mathf.Clamp01(time / d))];
        //         if (m_source && m_clips.Count > 0 && !m_source.isPlaying) //FIXME check if needed
        //         {
        //             m_source.clip = m_clips[Random.Range(0, m_clips.Count)];
        //             m_source.pitch = Random.Range(1f - m_pitchVariation, 1f + m_pitchVariation);
        //             m_source.Play();
        //         }
        //     }
        //     else
        //     {
        //         m_tmp.text = text;
        //     }
        //     var color = m_tmp.color;
        //     var fadeOut = m_fadeOut - (duration - time);
        //     if (fadeOut < 0) color.a = 1f;
        //     else color.a = 1f - fadeOut / m_fadeOut;
        //     m_tmp.color = color;
        // }

        // private void UpdateText(string value)
        // {
        //     _text = value;
        // }
        // public void Clear()
        // {
        //     if (_localizedString != null)
        //         _localizedString.StringChanged -= UpdateText;
        //     _text = null;
        //     m_tmp.text = "";
        // }
        // private void Update()
        // {
        //     if (_text != null)
        //     {
        //         _progress += Time.deltaTime;
        //         SetValues(_text, _progress, _duration);
        //     }
        // }
    }
}
