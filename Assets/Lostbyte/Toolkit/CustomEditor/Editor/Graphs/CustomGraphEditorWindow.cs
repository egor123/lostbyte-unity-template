using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public class CustomGraphEditorWindow<TGraph, TAsset, TNodeView, TNodeBase> : EditorWindow
        where TGraph : CustomGraphView<TGraph, TAsset, TNodeView, TNodeBase>, new()
        where TAsset : ScriptableObject
        where TNodeView : CustomGraphNode<TGraph, TAsset, TNodeView, TNodeBase>
        where TNodeBase : ScriptableObject
    {
        public static TAsset CurrentAsset;
        private TGraph _graphView;
        private ObjectField _assetField;


        protected virtual void OnEnable()
        {
            CreateGraphView();
            CreateToolbar();
            LoadLastSelectedDialogue();
        }
        private void CreateGraphView()
        {
            _graphView = new TGraph { name = "Graph" };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
        private void LoadLastSelectedDialogue()
        {
            if (EditorPrefs.HasKey($"LastSelected{nameof(TAsset)}"))
            {
                var path = EditorPrefs.GetString($"LastSelected{nameof(TAsset)}");
                CurrentAsset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
                _assetField.value = CurrentAsset;
                LoadAsset();
            }
        }
        protected virtual void OnDisable()
        {
            if (_graphView != null)
                rootVisualElement.Remove(_graphView);
        }

        protected virtual void CreateToolbar()
        {
            var toolbar = new Toolbar();

            _assetField = new ObjectField("Asset")
            {
                objectType = typeof(TAsset),
                allowSceneObjects = false
            };
            _assetField.RegisterValueChangedCallback(evt =>
            {
                // SaveDialogue(); //TODO FIXME!!!!!!!
                CurrentAsset = evt.newValue as TAsset;
                LoadAsset();
            });
            toolbar.Add(_assetField);
            var saveButton = new Button(SaveAsset) { text = "Save Asset" };
            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }


        private void LoadAsset()
        {
            _graphView.Load(CurrentAsset);
        }

        private void SaveAsset()
        {
            if (CurrentAsset != null)
            {
                _graphView.Save(CurrentAsset);
                EditorUtility.SetDirty(CurrentAsset);
                AssetDatabase.SaveAssets();
                EditorPrefs.SetString($"LastSelected{nameof(TAsset)}", AssetDatabase.GetAssetPath(CurrentAsset));
            }
        }
    }
}
