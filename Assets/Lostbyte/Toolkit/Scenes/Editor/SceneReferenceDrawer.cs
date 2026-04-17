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
            EditorGUI.ObjectField(
                position, 
                sceneAssetProp, 
                typeof(SceneAsset), 
                new GUIContent(label.text)
            );
            EditorGUI.EndProperty();
        }
    }
}
