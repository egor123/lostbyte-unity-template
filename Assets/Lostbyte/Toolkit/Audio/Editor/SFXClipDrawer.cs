using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Lostbyte.Toolkit.Audio.Editor
{
    [CustomPropertyDrawer(typeof(SFXClip))]
    public class SFXClipDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 24f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetObject = property.serializedObject.targetObject;
            Transform transform = (targetObject as Component).transform;

            EditorGUI.BeginProperty(position, label, property);

            Rect buttonRect = new(position.x, position.y, ButtonWidth, position.height);
            Rect fieldRect = new(position.x + ButtonWidth + 2, position.y, position.width - ButtonWidth - 2, position.height);

            EditorGUI.BeginDisabledGroup(property.objectReferenceValue == null);
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("PlayButton")))
            {
                SFXClip sfx = property.objectReferenceValue as SFXClip;
                if (sfx) sfx.Play(transform);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.ObjectField(fieldRect, property, label);

            EditorGUI.EndProperty();
        }

    }
}