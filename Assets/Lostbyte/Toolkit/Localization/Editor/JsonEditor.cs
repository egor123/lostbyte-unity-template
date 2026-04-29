using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Localization.Editor
{
    [UnityEditor.CustomEditor(typeof(TextAsset))]
    public class JsonEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            TextAsset textAsset = (TextAsset)target;
            string path = AssetDatabase.GetAssetPath(textAsset);
            string extention = Path.GetExtension(path);
            string content = LoadFile(path);

            switch (extention)
            {
                default:
                    var textField = new TextField() { value = content, doubleClickSelectsWord = true, multiline = true };
                    textField.RegisterValueChangedCallback(evt => SaveFile(path, evt.newValue));
                    root.Add(textField);
                    break;
            }
            return root;
        }

        [MenuItem("Assets/Create/Files/Json File", priority = 1)]
        public static void CreateJson()
        {
            string folderPath = GetSelectedFolderPath();
            string path = Path.Combine(folderPath, "NewJsonFile.json");
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);
            Texture2D icon = EditorGUIUtility.IconContent("TextAsset Icon").image as Texture2D;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                CreateInstance<CreateTextAssetAction>(),
                uniquePath,
                icon,
                "{\n\n}"
            );
        }
        [MenuItem("Assets/Create/Files/Text File", priority = 1)]
        public static void CreateTxt()
        {
            string folderPath = GetSelectedFolderPath();
            string path = Path.Combine(folderPath, "NewTextFile.txt");
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);
            Texture2D icon = EditorGUIUtility.IconContent("TextAsset Icon").image as Texture2D;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                CreateInstance<CreateTextAssetAction>(),
                uniquePath,
                icon,
                ""
            );
        }
        public class CreateTextAssetAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                File.WriteAllText(pathName, resourceFile);
                AssetDatabase.Refresh();
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
                ProjectWindowUtil.ShowCreatedAsset(asset);
            }
        }
        private static string GetSelectedFolderPath()
        {
            string path = "Assets";

            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);

                if (File.Exists(path))
                    path = Path.GetDirectoryName(path);

                break;
            }

            return path;
        }


        private VisualElement GetJsonUI(string path, string content)
        {
            JToken token = JsonConvert.DeserializeObject<JToken>(content);
            VisualElement root = new();
            root.style.paddingLeft = 10;
            DrawToken(root, token, () => SaveFile(path, token.ToString(Formatting.Indented)));
            return root;
        }

        private void DrawToken(VisualElement parent, JToken token, System.Action onChanged)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (JProperty prop in token.Children<JProperty>())
                    {
                        VisualElement container = new();
                        switch (prop.Value.Type)
                        {
                            case JTokenType.Integer:
                                IntegerField ifield = new(prop.Name) { value = prop.Value.Value<int>() };
                                ifield.RegisterValueChangedCallback(evt =>
                                {
                                    ((JValue)prop.Value).Value = evt.newValue;
                                    onChanged?.Invoke();
                                });
                                container.Add(ifield);
                                break;
                            case JTokenType.Float:
                                FloatField ffield = new(prop.Name) { value = prop.Value.Value<float>() };
                                ffield.RegisterValueChangedCallback(evt =>
                                {
                                    ((JValue)prop.Value).Value = evt.newValue;
                                    onChanged?.Invoke();
                                });
                                container.Add(ffield);
                                break;
                            case JTokenType.Boolean:
                                Toggle bfield = new(prop.Name) { value = prop.Value.Value<bool>() };
                                bfield.RegisterValueChangedCallback(evt =>
                                {
                                    ((JValue)prop.Value).Value = evt.newValue;
                                    onChanged?.Invoke();
                                });
                                container.Add(bfield);
                                break;
                            case JTokenType.String:
                                TextField sfield = new(prop.Name) { value = prop.Value.Value<string>() };
                                sfield.RegisterValueChangedCallback(evt =>
                                {
                                    ((JValue)prop.Value).Value = evt.newValue;
                                    onChanged?.Invoke();
                                });
                                container.Add(sfield);
                                break;
                            default:
                                Label label = new(prop.Name);
                                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                                container.Add(label);
                                DrawToken(container, prop.Value, onChanged);
                                break;
                        }

                        parent.Add(container);
                    }
                    break;

                case JTokenType.Array:
                    int index = 0;
                    foreach (JToken item in token.Children())
                    {
                        VisualElement container = new();
                        container.style.marginLeft = 15;

                        // Label label = new($"Element {index}");
                        // container.Add(label);

                        DrawToken(container, item, onChanged);

                        parent.Add(container);
                        index++;
                    }
                    break;

                case JTokenType.Integer:
                    IntegerField intField = new()
                    {
                        value = token.Value<int>()
                    };
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        ((JValue)token).Value = evt.newValue;
                        onChanged?.Invoke();
                    });
                    parent.Add(intField);
                    break;

                case JTokenType.Float:
                    FloatField floatField = new()
                    {
                        value = token.Value<float>()
                    };
                    floatField.RegisterValueChangedCallback(evt =>
                    {
                        ((JValue)token).Value = evt.newValue;
                        onChanged?.Invoke();
                    });
                    parent.Add(floatField);
                    break;

                case JTokenType.Boolean:
                    Toggle toggle = new()
                    {
                        value = token.Value<bool>()
                    };
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        ((JValue)token).Value = evt.newValue;
                        onChanged?.Invoke();
                    });
                    parent.Add(toggle);
                    break;

                default:
                    TextField textField = new()
                    {
                        value = token.ToString()
                    };
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        ((JValue)token).Value = evt.newValue;
                        onChanged?.Invoke();
                    });
                    parent.Add(textField);
                    break;
            }
        }


        private string LoadFile(string path)
        {
            using StreamReader r = new(path);
            return r.ReadToEnd();
        }
        private void SaveFile(string path, string content)
        {
            File.WriteAllText(path, content);
            EditorUtility.SetDirty(target);
        }
    }
}
