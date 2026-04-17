using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
#if UNITY_EDITOR
    // [UnityEditor.CustomEditor(typeof(ScriptableObject), true)]
    // public class ScriptableObjectEditorOverride : EditorOverride { }

    // [UnityEditor.CustomEditor(typeof(MonoBehaviour), true)]
    // [CanEditMultipleObjects]
    public class MonoBehaviourEditorOverride : EditorOverride { }

    // [UnityEditor.CustomEditor(, true)]
    // [CanEditMultipleObjects] //TODO
    public abstract class EditorOverride : UnityEditor.Editor
    {
        public static Dictionary<string, bool> FoldoutStates = new Dictionary<string, bool>();
        public static GUIStyle FoldoutStyle = null;
        public static GUIStyle FoldoutBackDropStyle = null;

        public abstract class EditorObject
        {
            public abstract void Draw(SerializedObject obj);
        }

        public class Foldout : EditorObject
        {
            public string Name;
            public List<EditorObject> children = new();
            public Foldout(string name) { Name = name; }
            public string GetID(SerializedObject obj) => $"{obj.targetObject.GetInstanceID()}.{Name}";
            public override void Draw(SerializedObject obj)
            {
                var state = FoldoutStates.GetValueOrDefault(GetID(obj), true);
                EditorGUILayout.BeginVertical(FoldoutBackDropStyle);
                state = EditorGUILayout.Foldout(state, Name, true, FoldoutStyle);
                if (state)
                    children.ForEach(c => c.Draw(obj));
                EditorGUILayout.EndVertical();
                FoldoutStates[GetID(obj)] = state;
            }
        }

        public class Field : EditorObject
        {
            public string Path;
            public Field(string path) { Path = path; }
            public override void Draw(SerializedObject obj)
            {
                EditorGUILayout.PropertyField(obj.FindProperty(Path), true);
            }
        }

        public List<EditorObject> Properties;
        public bool OverrideDefaultEditor = false;

        public override void OnInspectorGUI()
        {
            InitStyles();
            Init();
            serializedObject.Update();
            Draw();
            serializedObject.ApplyModifiedProperties();
        }

        private void Init()
        {
            if (Properties != null)
                return;

            Foldout activeGroup = null;
            Properties = new List<EditorObject>();
            var property = serializedObject.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                try
                {
                    var field = new Field(property.propertyPath);
                    var groupAtr = property.GetTargetField()?.GetCustomAttribute<GroupAttribute>();
                    if (groupAtr != null && groupAtr.GroupName == null)
                        activeGroup = null;

                    if (groupAtr != null && groupAtr.GroupName != null)
                    {
                        var group = GetFoldout(groupAtr.GroupName);
                        group.children.Add(field);
                        OverrideDefaultEditor = true;
                        if (groupAtr.LookNext)
                            activeGroup = group;
                    }
                    else if (activeGroup != null)
                        activeGroup.children.Add(field);
                    else
                        Properties.Add(field);
                }
                catch { }
            }
        }

        private Foldout GetFoldout(string name)
        {
            //TODO nested foldouts
            foreach (var prop in Properties)
                if (prop is Foldout f && f.Name == name)
                    return f;
            var foldout = new Foldout(name);
            Properties.Add(foldout);
            return foldout;
        }

        private void Draw()
        {
            if (OverrideDefaultEditor)
                Properties.ForEach(p => p.Draw(serializedObject));
            else
            {
                var property = serializedObject.GetIterator();
                if (property.NextVisible(true))
                    while (property.NextVisible(false))
                        EditorGUILayout.PropertyField(property, true);
            }
        }
        // target.GetType().GetFields(Extentions.FIELD_FLAGS);


        //.......................................................
        // The serializedObject should not be used inside OnSceneGUI or OnPreviewGUI. 
        // Use the target property directly instead.
        public void OnSceneGUI()
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);
            while (prop.NextVisible(true))
            {
                try
                {
                    prop.GetTargetField()
                        ?.GetCustomAttributes<CombinedAttribute>()
                        ?.ToList()
                        .ForEach(a => a.OnSceeneGUI(prop));
                }
                catch (System.Exception)
                {
                    Debug.LogWarning(prop.propertyPath + ";");
                    throw;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void InitStyles()
        {
            if (FoldoutStyle != null)
                return;

            FoldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                margin = new RectOffset(8, 0, -3, 0),
                fontStyle = FontStyle.Bold
            };
            FoldoutBackDropStyle = new GUIStyle(EditorStyles.helpBox);
        }
    }
#endif
}