using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.Localization;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    public abstract class SubtitlesManager : MonoBehaviour
    {
        [field: ClearStatic] public static SubtitlesManager Instance { get; private set; }

        public ScriptableObject CurentActor { get; protected set; }
        public LocalizedReference<string> CurrentText { get; protected set; }
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
        public abstract void Set(ScriptableObject actor, LocalizedReference<string> text);
        public abstract void SetFrame(ScriptableObject actor, LocalizedReference<string> text, float time, float duration);
        protected virtual void Update()
        {
            if (CurrentText != null)
            {
                Time = Mathf.Min(Time + UnityEngine.Time.deltaTime, CurrentDuration);
                SetFrame(CurentActor, CurrentText, Time, CurrentDuration);
                if (Time >= CurrentDuration) Clear();
            }
        }
    }
}
