using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Scenes.Editor
{
    [Overlay(typeof(SceneView), "Scene Loader")]
    public class SceneLoaderOverlay : ToolbarOverlay
    {
        SceneLoaderOverlay() : base(Dropdown.k_id) { }

        [EditorToolbarElement(k_id, typeof(SceneView))]
        private class Dropdown : EditorToolbarDropdown, IAccessContainerWindow
        {
            public const string k_id = "SceneLoaderOverlay/Dropdown";
            public EditorWindow containerWindow { get; set; }
            Dropdown()
            {
                tooltip = "Open or add scenes";
                icon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image as Texture2D;
                RegisterCallback<PointerUpEvent>(OnPointerUp); // shif + click works, but just click doesnt
            }
            private void OnPointerUp(PointerUpEvent evt)
            {
                bool asSingle = evt.button == 1 || evt.shiftKey;
                evt.StopPropagation();

                GenericMenu menu = new();
                menu.AddDisabledItem(new GUIContent($"Mode: {(asSingle ? "OPEN SINGLE" : "ADDITIVE (Default)")}"));

                HashSet<string> loadedPaths = UnityEngine.SceneManagement.SceneManager.sceneCount.ToStream()
                    .Select(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).path)
                    .ToHashSet();

                AssetDatabase.FindAssets("t:scene", new[] { "Assets" })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .OrderBy(path => path)
                    .ForEach(path =>
                    {
                        string name = Path.GetFileNameWithoutExtension(path);
                        bool isLoaded = loadedPaths.Contains(path);
                        menu.AddItem(new GUIContent(name), isLoaded, () =>
                        {
                            if (isLoaded && !asSingle) UnloadScene(path);
                            else OpenScene(path, asSingle ? OpenSceneMode.Single : OpenSceneMode.Additive);
                        });
                    });
                if (!loadedPaths.Any()) menu.AddDisabledItem(new GUIContent("No scenes found"));
                menu.ShowAsContext();
            }
            private void OpenScene(string path, OpenSceneMode mode)
            {
                if (Application.isPlaying) return;
                if (mode == OpenSceneMode.Additive || EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(path, mode);
            }
            private void UnloadScene(string path)
            {
                if (Application.isPlaying) return;
                if (UnityEngine.SceneManagement.SceneManager.sceneCount <= 1) return;
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
                if (!scene.isDirty || EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
                    EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
