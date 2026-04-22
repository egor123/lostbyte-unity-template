using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lostbyte.Toolkit.UI.Editor
{
    [UnityEditor.CustomEditor(typeof(UIElement))]
    public class UIElementEditor : UnityEditor.Editor
    {
        // Constraints Properties
        private SerializedProperty m_disableOnPlatforms;
        // Hover Properties
        private SerializedProperty m_enableHoverAnim;
        private SerializedProperty m_hoverAnimType;
        private SerializedProperty m_hoverAnimDuration;
        private SerializedProperty m_hoverScale;
        // Audio Properties
        private SerializedProperty m_onSelectClip;
        private SerializedProperty m_onDeselectClip;
        private SerializedProperty m_onChangeClip;
        private SerializedProperty m_onSubmitClip;

        private void OnEnable()
        {
            m_disableOnPlatforms = serializedObject.FindProperty("m_disableOnPlatforms");

            m_enableHoverAnim = serializedObject.FindProperty("m_enableHoverAnim");
            m_hoverAnimType = serializedObject.FindProperty("m_hoverAnimType");
            m_hoverAnimDuration = serializedObject.FindProperty("m_hoverAnimDuration");
            m_hoverScale = serializedObject.FindProperty("m_hoverScale");

            m_onSelectClip = serializedObject.FindProperty("m_onSelectClip");
            m_onDeselectClip = serializedObject.FindProperty("m_onDeselectClip");
            m_onChangeClip = serializedObject.FindProperty("m_onChangeClip");
            m_onSubmitClip = serializedObject.FindProperty("m_onSubmitClip");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UIElement uiElement = (UIElement)target;
            bool hasInputField = uiElement.GetComponent<TMP_InputField>() != null;
            bool hasSlider = uiElement.GetComponent<Slider>() != null;
            bool hasButton = uiElement.GetComponent<Button>() != null;

            EditorGUILayout.LabelField("Constraints Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_disableOnPlatforms);

            EditorGUILayout.LabelField("Hover Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_enableHoverAnim);

            if (m_enableHoverAnim.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_hoverAnimType);
                EditorGUILayout.PropertyField(m_hoverAnimDuration);
                EditorGUILayout.PropertyField(m_hoverScale);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Audio Settings", EditorStyles.boldLabel);

            if (hasInputField)
            {
                EditorGUILayout.PropertyField(m_onSelectClip);
                EditorGUILayout.PropertyField(m_onDeselectClip);
            }

            if (hasInputField || hasSlider)
            {
                EditorGUILayout.PropertyField(m_onChangeClip);
            }

            if (hasInputField || hasButton)
            {
                EditorGUILayout.PropertyField(m_onSubmitClip);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}