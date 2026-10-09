using System.IO;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    public class BuildPipelineWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private UnityEditor.Editor _configEditor;

        [MenuItem("Tools/Build Pipeline/Open Window", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildPipelineWindow>("Build Pipeline");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            BuildPipelineUtils.TasksUpdated += Repaint;
            if (BuildPipelineUtils.GetPipelineConf(out var conf))
            {
                _configEditor = UnityEditor.Editor.CreateEditor(conf);
            }
        }

        private void OnDisable()
        {

            BuildPipelineUtils.TasksUpdated -= Repaint;
            if (_configEditor != null)
            {
                DestroyImmediate(_configEditor);
            }
        }

        private void OnGUI()
        {
            DrawConfigPanel();
            DrawPipelineConfig();
            DrawTaskList();
            GUILayout.FlexibleSpace();
            DrawBottomButtons();
        }
        private void DrawPipelineConfig()
        {
            if (_configEditor != null)
            {
                EditorGUILayout.LabelField("Build Pipeline", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                _configEditor.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck()) BuildPipelineConfig.instance.Save();
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.HelpBox("BuildPipelineConfig instance not found.", MessageType.Warning); //[cite: 1]
            }
        }
        private void DrawConfigPanel()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Project Configuration", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            string newProductName = EditorGUILayout.TextField("Product Name", PlayerSettings.productName);
            if (EditorGUI.EndChangeCheck()) PlayerSettings.productName = newProductName;
            EditorGUI.BeginChangeCheck();
            string newVersion = EditorGUILayout.TextField("Bundle Version", PlayerSettings.bundleVersion);
            if (EditorGUI.EndChangeCheck()) PlayerSettings.bundleVersion = newVersion;
            EditorGUILayout.Space();
        }

        private void DrawTaskList()
        {
            EditorGUILayout.LabelField("Build Tasks", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, "box", GUILayout.ExpandHeight(true));

            if (BuildPipelineUtils.BuildTasks == null || BuildPipelineUtils.BuildTasks.Count == 0)
            {
                EditorGUILayout.HelpBox("No active build tasks.", MessageType.Info);
            }
            else
            {
                foreach (var task in BuildPipelineUtils.BuildTasks)
                {
                    Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 24);
                    Color originalColor = GUI.color;
                    GUI.color = GetColorForBuildResult(task.Result);
                    EditorGUI.ProgressBar(rect, task.Progress, $"{task.TaskName} ({task.TimeText}) - {task.Result}");
                    GUI.color = originalColor;
                    GUILayout.Space(2);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private Color GetColorForBuildResult(BuildResult result)
        {
            return result switch
            {
                BuildResult.Succeeded => new Color(0.2f, 0.8f, 0.2f),
                BuildResult.Failed => new Color(0.8f, 0.2f, 0.2f),
                BuildResult.Cancelled => new Color(0.8f, 0.8f, 0.2f),
                _ => Color.white
            };
        }
        private void DrawBottomButtons()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            GUIStyle buttonStyle = new(GUI.skin.button) { fixedHeight = 30 };

            if (GUILayout.Button("Build", buttonStyle))
            {
                GUILayout.EndHorizontal();
                BuildPipelineUtils.Build();
                return;
            }
            if (GUILayout.Button("Upload", buttonStyle))
            {
                GUILayout.EndHorizontal();
                BuildPipelineUtils.Upload();
                return;
            }
            if (GUILayout.Button("Run", buttonStyle))
            {
                GUILayout.EndHorizontal();
                BuildPipelineUtils.Run();
                return;
            }
            if (GUILayout.Button("Show Folder", buttonStyle))
            {
                GUILayout.EndHorizontal();
                BuildPipelineUtils.OpenFolder();
                return;
            }
            if (GUILayout.Button("Open Player Log", buttonStyle))
            {
                GUILayout.EndHorizontal();
                BuildPipelineUtils.OpenPlayerLog();
                return;
            }
            if (GUILayout.Button("Build & Upload", buttonStyle))
            {
                GUILayout.EndHorizontal();
                BuildPipelineUtils.BuildAndUpload();
                return;
            }
            GUILayout.EndHorizontal();
        }
    }
}
