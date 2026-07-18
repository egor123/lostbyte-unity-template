using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class FactSystemRepairUtility
    {
        [MenuItem("Window/Facts/Repair Broken Database Objects")]
        public static void RepairBrokenObjects()
        {
            MonoScript dbScript = FindMonoScript("FactDatabase");
            MonoScript keyScript = FindMonoScript("KeyContainer");
            MonoScript factScript = FindMonoScript("FactDefinition");
            MonoScript eventScript = FindMonoScript("EventDefinition");

            if (dbScript == null || keyScript == null)
            {
                Debug.LogError("Repair failed: Could not locate the MonoScripts for FactDatabase or KeyContainer.");
                return;
            }
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            int repairedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (Object asset in assets)
                {
                    if (asset == null) continue;
                    if ((asset.hideFlags & HideFlags.NotEditable) != 0)
                    {
                        asset.hideFlags &= ~HideFlags.NotEditable;
                        EditorUtility.SetDirty(asset);
                        Debug.Log($"Cleared NotEditable flag on: {asset.name}");
                    }
                    SerializedObject so = new SerializedObject(asset);
                    SerializedProperty scriptProp = so.FindProperty("m_Script");
                    if (scriptProp != null && scriptProp.objectReferenceValue == null)
                    {
                        MonoScript assignedScript = null;
                        if (asset.name.Contains("Database")) assignedScript = dbScript;
                        else if (asset.name.Contains("Key")) assignedScript = keyScript;
                        else if (asset.name.Contains("Fact")) assignedScript = factScript;
                        else if (asset.name.Contains("Event")) assignedScript = eventScript;

                        if (assignedScript != null)
                        {
                            scriptProp.objectReferenceValue = assignedScript;
                            so.ApplyModifiedPropertiesWithoutUndo();
                            EditorUtility.SetDirty(asset);
                            repairedCount++;
                            Debug.Log($"Repaired missing script on: {path} -> Assigned {assignedScript.name}");
                        }
                    }
                }
            }

            if (repairedCount > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"<color=green>Successfully repaired {repairedCount} broken objects!</color>");
            }
            else
            {
                Debug.Log("Scan complete. No broken objects found.");
            }
        }

        private static MonoScript FindMonoScript(string className)
        {
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {className}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() != null && script.GetClass().Name == className)
                {
                    return script;
                }
            }
            return null;
        }
    }
}