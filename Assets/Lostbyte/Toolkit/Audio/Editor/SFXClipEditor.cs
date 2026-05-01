using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Editor
{
    [UnityEditor.CustomEditor(typeof(SFXClip))]
    public class SFXClipEditor : UnityEditor.Editor
    {
        private SerializedProperty _clips;
        private SerializedProperty _minVolume;
        private SerializedProperty _maxVolume;
        private SerializedProperty _minPitch;
        private SerializedProperty _maxPitch;
        private SerializedProperty _spatialBlend;
        private SerializedProperty _steroPan;
        private SerializedProperty _reverb;
        private SerializedProperty _minDist;
        private SerializedProperty _maxDist;
        private SerializedProperty _rolloff;
        private SerializedProperty _spread;
        private SerializedProperty _dopler;

        private void OnEnable()
        {
            _clips = serializedObject.FindProperty($"<{nameof(SFXClip.Clips)}>k__BackingField");
            _minVolume = serializedObject.FindProperty($"<{nameof(SFXClip.MinVolume)}>k__BackingField");
            _maxVolume = serializedObject.FindProperty($"<{nameof(SFXClip.MaxVolume)}>k__BackingField");
            _minPitch = serializedObject.FindProperty($"<{nameof(SFXClip.MinPitch)}>k__BackingField");
            _maxPitch = serializedObject.FindProperty($"<{nameof(SFXClip.MaxPitch)}>k__BackingField");
            _spatialBlend = serializedObject.FindProperty($"<{nameof(SFXClip.SpatialBlend)}>k__BackingField");
            _steroPan = serializedObject.FindProperty($"<{nameof(SFXClip.StereoPan)}>k__BackingField");
            _reverb = serializedObject.FindProperty($"<{nameof(SFXClip.ReverbZoneMix)}>k__BackingField");
            _minDist = serializedObject.FindProperty($"<{nameof(SFXClip.MinDistance)}>k__BackingField");
            _maxDist = serializedObject.FindProperty($"<{nameof(SFXClip.MaxDistance)}>k__BackingField");
            _rolloff = serializedObject.FindProperty($"<{nameof(SFXClip.RolloffMode)}>k__BackingField");
            _spread = serializedObject.FindProperty($"<{nameof(SFXClip.Spread)}>k__BackingField");
            _dopler = serializedObject.FindProperty($"<{nameof(SFXClip.DopplerLevel)}>k__BackingField");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // --- CLIPS SECTION ---
            EditorGUILayout.PropertyField(_clips, true);

            // --- VOLUME & PITCH (MIN/MAX SLIDERS) ---
            EditorGUILayout.LabelField("Audio Dynamics", EditorStyles.boldLabel);

            DrawMinMaxSlider("Volume", _minVolume, _maxVolume, 0f, 1f);
            DrawMinMaxSlider("Pitch", _minPitch, _maxPitch, -3f, 3f);

            EditorGUILayout.Space(5);

            // --- SPATIAL SETTINGS ---
            EditorGUILayout.LabelField("Spatial Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_spatialBlend);
            EditorGUILayout.PropertyField(_steroPan);
            EditorGUILayout.PropertyField(_reverb);

            // Draw Min/Max distance on one line
            if (_spatialBlend.floatValue > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("3D Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_dopler);
                EditorGUILayout.PropertyField(_spread);
                EditorGUILayout.PropertyField(_rolloff);

                Rect rect = EditorGUILayout.GetControlRect();
                rect = EditorGUI.PrefixLabel(rect, new GUIContent("Distance (Min/Max)"));
                float halfWidth = rect.width / 2f - 5f;
                Rect minRect = new(rect.x, rect.y, halfWidth, rect.height);
                Rect maxRect = new(rect.x + halfWidth + 10f, rect.y, halfWidth, rect.height);
                EditorGUI.PropertyField(minRect, _minDist, GUIContent.none);
                EditorGUI.LabelField(new Rect(rect.x + halfWidth, rect.y, 10f, rect.height), "-", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.PropertyField(maxRect, _maxDist, GUIContent.none);


            }
            EditorGUILayout.Space(10);

            // --- PREVIEW BUTTON ---
            GUIContent playContent = EditorGUIUtility.IconContent("d_PlayButton");
            playContent.text = " Play Preview";
            if (GUILayout.Button(playContent, GUILayout.Height(30)))
            {
                PreviewAudio();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMinMaxSlider(string label, SerializedProperty minProp, SerializedProperty maxProp, float min, float max)
        {
            float lower = (float)System.Math.Round(minProp.floatValue, 2);
            float upper = (float)System.Math.Round(maxProp.floatValue, 2);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            EditorGUI.BeginChangeCheck();
            lower = EditorGUILayout.DelayedFloatField(lower, GUILayout.Width(50));
            EditorGUILayout.MinMaxSlider(ref lower, ref upper, min, max);
            upper = EditorGUILayout.DelayedFloatField(upper, GUILayout.Width(50));

            if (EditorGUI.EndChangeCheck())
            {
                minProp.floatValue = (float)System.Math.Round(lower, 2);
                maxProp.floatValue = (float)System.Math.Round(upper, 2);
            }

            EditorGUILayout.EndHorizontal();
        }
        private void PreviewAudio()
        {
            SFXClip clip = (SFXClip)target;
            Vector3 viewPos = SceneView.lastActiveSceneView.camera.transform.position;
            clip.Play(viewPos);
        }
    }
}
