using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lostbyte.Toolkit.CustomEditor
{
        public class RequiredAttribute : CombinedAttribute
        {
#if UNITY_EDITOR
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var component = (Component)property.serializedObject.targetObject;
                var type = property.GetTargetType();
                if (component.GetComponent(type) == null)
                    component.gameObject.AddComponent(type);
            }
#endif
        }
}
