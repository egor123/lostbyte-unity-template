using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lostbyte.Toolkit.UI.Editor
{
    [UnityEditor.CustomEditor(typeof(UIElement))]
    public class UIElementEditor : UnityEditor.Editor
    {

        // Hover Properties
        private SerializedProperty m_selectOnHover;
        private SerializedProperty m_enableHoverAnim;
        private SerializedProperty m_enableSelectedAnim;

        private SerializedProperty m_animType;
        private SerializedProperty m_animDuration;
        private SerializedProperty m_animScale;
        // Audio Properties
        private SerializedProperty m_onSelectClip;
        private SerializedProperty m_onDeselectClip;
        private SerializedProperty m_onChangeClip;
        private SerializedProperty m_onSubmitClip;

        private void OnEnable()
        {
            m_selectOnHover = serializedObject.FindProperty("m_selectOnHover");
            m_enableHoverAnim = serializedObject.FindProperty("m_enableHoverAnim");
            m_enableSelectedAnim = serializedObject.FindProperty("m_enableSelectedAnim");

            m_animType = serializedObject.FindProperty("m_animType");
            m_animDuration = serializedObject.FindProperty("m_animDuration");
            m_animScale = serializedObject.FindProperty("m_animScale");

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

            EditorGUILayout.PropertyField(m_selectOnHover);

            EditorGUILayout.LabelField("Tween Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_enableHoverAnim);
            EditorGUILayout.PropertyField(m_enableSelectedAnim);

            if (m_enableHoverAnim.boolValue || m_enableSelectedAnim.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_animType);
                EditorGUILayout.PropertyField(m_animDuration);
                EditorGUILayout.PropertyField(m_animScale);
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