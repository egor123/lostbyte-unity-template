using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes.Editor
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty sceneAssetProp = property.FindPropertyRelative("m_sceneAsset");
            SerializedProperty scenePathProp = property.FindPropertyRelative("m_scenePath");
            EditorGUI.BeginChangeCheck();
            EditorGUI.ObjectField(
                position,
                sceneAssetProp,
                typeof(SceneAsset),
                new GUIContent(label.text)
            );
            if (EditorGUI.EndChangeCheck())
            {
                if (sceneAssetProp.objectReferenceValue != null)
                    scenePathProp.stringValue = AssetDatabase.GetAssetPath(sceneAssetProp.objectReferenceValue);
                else
                    scenePathProp.stringValue = string.Empty;
            }
            EditorGUI.EndProperty();
        }
    }
}
