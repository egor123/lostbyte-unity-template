using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.Director
{
    public class SubtitleTrackMixer : PlayableBehaviour
    {
        public ScriptableObject Actor;
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var manager = playerData as SubtitlesManager;
            if (!manager) return;
            string text = "";
            float time = 1, duration = 1;
            int count = playable.GetInputCount();
            for (int i = 0; i < count; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight > 0f)
                {
                    var iPlayable = (ScriptPlayable<SubtitleBehaviour>)playable.GetInput(i);
                    time = (float)iPlayable.GetTime();
                    duration = (float)iPlayable.GetDuration();
                    var iBehaviour = iPlayable.GetBehaviour();
                    text = GetText(iBehaviour.SubtitleText);
                }
            }
            manager.SetFrame(Actor, text, time, duration);//FIXME
        }

        private string GetText(LocalizedString text)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var collection = UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(text.TableReference);
                if (collection == null) return "";
                var table = collection.GetTable(UnityEditor.Localization.LocalizationEditorSettings.GetLocales()[0].Identifier); //FIXME
                if (table is not UnityEngine.Localization.Tables.StringTable stringTable) return "";
                var entry = stringTable.GetEntry(text.TableEntryReference.KeyId);
                return entry?.LocalizedValue ?? "";
            }
#endif
            return LocalizationSettings.StringDatabase.GetLocalizedString(text.TableReference, text.TableEntryReference, text.Arguments) ?? "";
        }
    }
}
