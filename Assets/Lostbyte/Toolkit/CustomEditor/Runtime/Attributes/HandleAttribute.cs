using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class HandleAttribute : CombinedAttribute
    {
#if UNITY_EDITOR
        public override void OnSceeneGUI(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject as MonoBehaviour;
            var transform = target.transform;
            var value = transform.TransformPoint(property.GetCastedVector3Value());
            value = Handles.PositionHandle(value, transform.rotation);
            property.SetVectorValue(transform.InverseTransformPoint(value));
        }
#endif
    }
}