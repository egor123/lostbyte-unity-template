using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public abstract class CustomGraphEditorWindow<TGraph, TAsset, TNodeView, TNodeBase> : EditorWindow
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
            saveChangesMessage = "You have unsaved changes in your Graph. Do you want to save them?";
            rootVisualElement.Clear();
            CreateGraphView();
            CreateToolbar();
            LoadLastSelectedDialogue();
        }

        protected virtual void OnDisable()
        {
            if (_graphView != null)
            {
                _graphView.OnGraphModified -= MarkAsDirty;
                _graphView.ClearGraph();
                rootVisualElement.Remove(_graphView);
            }
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            SaveAsset();
        }

        private void CreateGraphView()
        {
            _graphView = new TGraph { name = "Graph" };
            _graphView.Initialize(this);
            _graphView.StretchToParentSize();

            _graphView.OnGraphModified += MarkAsDirty;
            Undo.undoRedoPerformed += OnUndoRedo;

            rootVisualElement.Add(_graphView);
        }

        private void CreateToolbar()
        {
            var toolbar = new Toolbar();

            _assetField = new ObjectField("Asset")
            {
                objectType = typeof(TAsset),
                allowSceneObjects = false
            };

            _assetField.RegisterValueChangedCallback(evt =>
            {
                var newAsset = evt.newValue as TAsset;
                if (newAsset == CurrentAsset) return;
                if (hasUnsavedChanges)
                {
                    int choice = EditorUtility.DisplayDialogComplex(
                        "Unsaved Changes",
                        $"Save changes to {CurrentAsset?.name ?? "current graph"}?",
                        "Save", "Discard", "Cancel");

                    if (choice == 0)
                    {
                        SaveAsset();
                    }
                    else if (choice == 2)
                    {
                        _assetField.SetValueWithoutNotify(CurrentAsset);
                        return;
                    }
                }
                CurrentAsset = newAsset;
                LoadAsset();
            });

            toolbar.Add(_assetField);

            var saveButton = new Button(SaveAsset) { text = "Save Asset" };
            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

        private void LoadLastSelectedDialogue()
        {
            string prefKey = $"LastSelected{nameof(TAsset)}";
            if (EditorPrefs.HasKey(prefKey))
            {
                var path = EditorPrefs.GetString(prefKey);
                var asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);

                if (asset != null)
                {
                    CurrentAsset = asset;
                    _assetField.SetValueWithoutNotify(CurrentAsset);
                    LoadAsset();
                }
            }
        }

        private void LoadAsset()
        {
            hasUnsavedChanges = false;
            _graphView.Load(CurrentAsset);

            if (CurrentAsset != null)
                EditorPrefs.SetString($"LastSelected{nameof(TAsset)}", AssetDatabase.GetAssetPath(CurrentAsset));
        }

        private void SaveAsset()
        {
            if (CurrentAsset != null)
            {
                _graphView.Save(CurrentAsset);
                hasUnsavedChanges = false;
            }
        }

        private void MarkAsDirty()
        {
            if (CurrentAsset != null)
                hasUnsavedChanges = true;
        }

        private void OnUndoRedo()
        {
            if (CurrentAsset != null)
            {
                hasUnsavedChanges = true;
                _graphView.Load(CurrentAsset);
            }
        }
    }
}