using System;
using System.Diagnostics;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.Build.Editor
{
    public class SceneBuildValidation : IProcessSceneWithReport, IPreprocessBuildWithReport
    {
        [ClearStatic] public static bool ValidateBuild = true;
        public int callbackOrder { get => 0; }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!BuildPipeline.isBuildingPlayer || !ValidateBuild) return;
            Stopwatch sw = new();
            sw.Start();
            bool hasErrors = false;
            AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
                .WhereNotNull()
                .ForEach(so => DeepValidateSerializedObject(new SerializedObject(so), so, ref hasErrors));
            AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .WhereNotNull()
                .ForEach(go => ValidateGameObject(go, ref hasErrors));

            Print.MLog($"Project validation took {sw.Elapsed.Seconds}s");
            sw.Stop();
            if (hasErrors) throw new BuildFailedException($"Build aborted! ScriptableObject validation found errors.");
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (!BuildPipeline.isBuildingPlayer || !ValidateBuild) return;
            Stopwatch sw = new();
            sw.Start();
            bool hasErrors = false;
            scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .Distinct()
                .ForEach(go => ValidateGameObject(go, ref hasErrors));

            Print.MLog($"Scene '{scene.name}' validation took {sw.Elapsed.Seconds}s");
            sw.Stop();
            if (hasErrors) throw new BuildFailedException($"Build aborted! {scene.name} has validation errors.");
        }

        private void ValidateGameObject(GameObject go, ref bool hasErrors)
        {
            var components = go.GetComponents<Component>();
            if (components.Any(c => c == null))
            {
                bool isHidden = go.hideFlags.HasFlag(HideFlags.HideInHierarchy);
                Print.MWarn($"{go.name} has missing script! {(isHidden ? " [HideInHierarchy]" : "")}", go);
                hasErrors = true;
            }
            foreach (var comp in components)
            {
                if (comp == null) continue;
                DeepValidateSerializedObject(new SerializedObject(comp), comp, ref hasErrors);
            }
        }
        private void DeepValidateSerializedObject(SerializedObject serializedObject, UnityEngine.Object contextObject, ref bool hasErrors)
        {
            if (contextObject is ScriptableObject so)
            {
                var scriptProp = serializedObject.FindProperty("m_Script");
                if (scriptProp != null && scriptProp.objectReferenceValue == null)
                {
                    Print.MWarn($"ScriptableObject '{contextObject.name}' has a missing script!", contextObject);
                    hasErrors = true;
                    return;
                }
            }
            if (contextObject is IValidatable rootValidatable)
            {
                try
                {
                    rootValidatable.Validate();
                }
                catch (Exception ex)
                {
                    Print.MWarn($"[IValidatable] Validation failed on '{contextObject.GetType().Name}' ('{contextObject.name}'): {ex.Message}", contextObject);
                    hasErrors = true;
                }
            }
            var property = serializedObject.GetIterator();
            while (property.NextVisible(true))
            {
                var fieldInfo = property.GetTargetField();
                if (fieldInfo != null && Attribute.IsDefined(fieldInfo, typeof(RequiredAttribute)))
                {
                    if (RequiredAttribute.IsEmpty(property))
                    {
                        Print.MWarn($"[Required] Field '{property.propertyPath}' is empty on '{contextObject.name}'!", contextObject);
                        hasErrors = true;
                    }
                }
                if (property.propertyType == SerializedPropertyType.Generic ||
                    property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    object value = property.GetTargetObject();
                    if (value is IValidatable nestedValidatable)
                    {
                        try
                        {
                            nestedValidatable.Validate();
                        }
                        catch (Exception ex)
                        {
                            Print.MWarn($"[IValidatable] Validation failed on struct/class '{property.propertyPath}' inside '{contextObject.name}': {ex.Message}", contextObject);
                            hasErrors = true;
                        }
                    }
                }
            }
        }
    }
}
