using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class PlayModeOnly : CombinedAttribute
    {
        public enum Type
        {
            Disable,
            Hide
        }
        private Type _type;
        public PlayModeOnly(Type type = Type.Disable) { _type = type; }
#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!Application.isPlaying && _type == Type.Disable)
                GUI.enabled = false;
        }
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!Application.isPlaying && _type == Type.Hide)
                return 0f;
            return base.GetPropertyHeight(property, label);
        }
        public override bool DrawDefaultPropertyField()
        {
            if (!Application.isPlaying && _type == Type.Hide)
                return false;
            return base.DrawDefaultPropertyField();
        }

#endif
    }
}