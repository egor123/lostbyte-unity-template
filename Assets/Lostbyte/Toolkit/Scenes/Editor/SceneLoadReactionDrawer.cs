using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Scenes.Editor
{
    [CustomPropertyDrawer(typeof(SceneLoadReaction))]
    public class SceneLoadReactionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.Add(CreateRow("Parent", property.FindPropertyRelative(nameof(SceneLoadReaction.ParentScene))));
            root.Add(CreateRow("Use Loading Screen", property.FindPropertyRelative(nameof(SceneLoadReaction.UseLoadingScreen))));

            var factDef = GetParentFactDefinition(property);
            if (factDef == null || factDef.EnumType == null)
            {
                root.Add(new Label("Target Fact must be an Enum to use SceneLoadReaction.")
                    .SetFontStyle(FontStyle.Italic)
                    .SetOpacity(0.6f));
                return root;
            }
            SyncScenesToEnum(property, factDef.EnumType);
            var scenesProp = property.FindPropertyRelative(nameof(SceneLoadReaction.Scenes));
            for (int i = 0; i < scenesProp.arraySize; i++)
            {
                var itemProp = scenesProp.GetArrayElementAtIndex(i);
                var conditionProp = itemProp.FindPropertyRelative(nameof(SceneLoadReaction.SceneCondition.Condition));
                var sceneProp = itemProp.FindPropertyRelative(nameof(SceneLoadReaction.SceneCondition.Scene));
                string enumName = conditionProp.managedReferenceValue?.ToString() ?? "Unknown";
                root.Add(CreateRow(enumName, sceneProp));
            }

            return root;
        }

        private VisualElement CreateRow(string name, SerializedProperty sceneProp)
        {
            var row = new VisualElement().MakeRow(Align.Center);
            row.Add(new Label(name).SetFlex(0.4f, 0));
            row.Add(new PropertyField(sceneProp, "").SetFlex(0.6f, 0));
            return row;
        }

        private void SyncScenesToEnum(SerializedProperty property, Type enumType)
        {
            if (property.managedReferenceValue is not SceneLoadReaction reaction) return;

            var enumValues = Enum.GetValues(enumType);
            bool modified = false;

            var newScenes = new List<SceneLoadReaction.SceneCondition>();

            foreach (Enum enumVal in enumValues)
            {
                int existingIndex = reaction.Scenes.FindIndex(s => s.Condition != null && s.Condition.Equals(enumVal));
                if (existingIndex >= 0)
                {
                    newScenes.Add(reaction.Scenes[existingIndex]);
                }
                else
                {
                    newScenes.Add(new SceneLoadReaction.SceneCondition { Condition = enumVal, Scene = default });
                    modified = true;
                }
            }
            if (newScenes.Count != reaction.Scenes.Count) modified = true;
            if (modified)
            {
                reaction.Scenes = newScenes;
                property.serializedObject.Update();
            }
        }

        private EnumFactDefinition GetParentFactDefinition(SerializedProperty property)
        {
            string path = property.propertyPath;
            int reactionsIndex = path.IndexOf(".Reactions", StringComparison.Ordinal);
            if (reactionsIndex > 0)
            {
                string registrationPath = path[..reactionsIndex];
                var registrationProp = property.serializedObject.FindProperty(registrationPath);
                if (registrationProp != null)
                {
                    var factProp = registrationProp.FindPropertyRelative(nameof(FactRegistration.Fact));
                    return factProp?.objectReferenceValue as EnumFactDefinition;
                }
            }
            return null;
        }
    }
}