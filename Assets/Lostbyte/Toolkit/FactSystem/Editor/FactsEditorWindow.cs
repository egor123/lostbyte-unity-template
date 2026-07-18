using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class FactsEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField] private StyleSheet m_StyleSheet = default;

        private TreeView _treeView;
        private int _lastAssetHash = 0;
        private string _filter = "";
        private int _view = 0;
        private Vector2 _lastMousePosition;

        private class RowContext
        {
            public object Item;
            public int Index;
        }

        [MenuItem("Window/Facts/Facts Editor")]
        public static void ShowFactsEditorWindow()
        {
            var wnd = GetWindow<FactsEditorWindow>();
            wnd.titleContent = new GUIContent("Facts Editor");
        }

        public void CreateGUI()
        {
            LoadAssetsDynamically();

            VisualElement root = rootVisualElement;
            m_VisualTreeAsset.CloneTree(root);
            root.styleSheets.Add(m_StyleSheet);

            _treeView = root.Q<TreeView>("tree-view");

            // Event bindings
            root.Q<Button>("add-btn").clicked += OnAddButtonClicked;
            root.Q<Button>("compile-btn").clicked += OnCompileButtonClicked;

            root.Q<ToolbarSearchField>("search-bar").RegisterValueChangedCallback(evt =>
            {
                _filter = evt.newValue;
                SaveTreeViewState();
                BuildTreeView();
            });

            var inspectorPanel = root.Q<ScrollView>("inspector");
            _treeView.selectionChanged += (selectedItems) => UpdateInspectorPanel(inspectorPanel, selectedItems);

            var dropdown = root.Q<DropdownField>("tab-selector");
            _view = dropdown.index;
            dropdown.RegisterValueChangedCallback(evt =>
            {
                _view = dropdown.index;
                SaveTreeViewState();
                BuildTreeView();
            });

            BuildTreeView();

            EditorApplication.update += CheckForDatabaseChanges;
            EditorApplication.playModeStateChanged += OnGameStateChange;
        }

        private void LoadAssetsDynamically()
        {
            if (m_VisualTreeAsset == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:VisualTreeAsset FactsEditorWindow");
                if (guids.Length > 0)
                    m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            if (m_StyleSheet == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:StyleSheet FactsEditorWindow");
                if (guids.Length > 0)
                    m_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        private void OnCompileButtonClicked()
        {
            if (Application.isPlaying)
            {
                Print.Warn("Cannot compile when playing!");
                return;
            }

            AssetDatabase.SaveAssets();
            FactCodeGenerator.Generate(FactEditorUtils.Database);
        }

        private void OnAddButtonClicked()
        {
            switch (_view)
            {
                case 0: FactEditorUtils.ShowAddNewKeyPopup(null, "", _lastMousePosition); break;
                case 1: FactEditorUtils.ShowAddNewFactPopup(null, "", true, _lastMousePosition); break;
                case 2: FactEditorUtils.ShowAddNewEventPopup(null, "", true, _lastMousePosition); break;
                default: Print.Warn("Unknown view"); break;
            }
        }

        private void UpdateInspectorPanel(ScrollView inspectorPanel, IEnumerable<object> selectedItems)
        {
            inspectorPanel.Clear();
            var selectedItem = selectedItems.FirstOrDefault();
            if (selectedItem == null || !(selectedItem is ScriptableObject item)) return;

            var serializedObject = new SerializedObject(item);
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script") continue;

                var field = new PropertyField(iterator.Copy());
                field.Bind(serializedObject);
                inspectorPanel.Add(field);
            }
            if (item is FactDefinition fact)
            {
                DrawRegistrationInspector(inspectorPanel, fact, nameof(KeyContainer.FactRegistrations), nameof(FactRegistration.Fact));
            }
            else if (item is EventDefinition evt)
            {
                DrawRegistrationInspector(inspectorPanel, evt, nameof(KeyContainer.EventRegistrations), nameof(EventRegistration.Event));
            }
        }

        private void DrawRegistrationInspector(ScrollView inspectorPanel, ScriptableObject item, string listPropertyName, string elementPropertyName)
        {
            var parentKey = GetParentKey(_treeView.selectedIndex);
            if (parentKey == null) return;

            var keySO = new SerializedObject(parentKey);

            var regsProp = keySO.FindProperty(listPropertyName) ??
                           keySO.FindProperty($"<{listPropertyName}>k__BackingField");

            if (regsProp == null) return;

            for (int i = 0; i < regsProp.arraySize; i++)
            {
                var regProp = regsProp.GetArrayElementAtIndex(i);
                var elementProp = regProp.FindPropertyRelative(elementPropertyName);

                if (elementProp != null && elementProp.objectReferenceValue == item)
                {
                    inspectorPanel.Add(new VisualElement()
                        .SetBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                        .SetMargin(10, 0));

                    var regField = new PropertyField(regProp);
                    regField.BindProperty(regProp);
                    inspectorPanel.Add(regField);
                    break;
                }
            }
        }

        private void BuildTreeView()
        {
            var db = FactEditorUtils.Database;
            if (db == null || _treeView == null) return;

            EnsureGuidsExist(db.FactStorage);
            EnsureGuidsExist(FactEditorUtils.GetAllKeys());
            EnsureGuidsExist(db.EventStorage);

            var treeItems = new List<TreeViewItemData<object>>();
            int id = 0;

            switch (_view)
            {
                case 0:
                    foreach (var rootKey in db.RootKeys)
                    {
                        var item = FilterKeyView(rootKey, _filter, ref id);
                        if (item.HasValue) treeItems.Add(item.Value);
                    }
                    break;
                case 1:
                    FilterFactView(db, _filter, ref id).ForEach(treeItems.Add);
                    break;
                case 2:
                    FilterEventView(db, _filter, ref id).ForEach(treeItems.Add);
                    break;
                default:
                    Print.Warn("Unknown view");
                    break;
            }

            _treeView.makeItem = () =>
            {
                var container = new VisualElement();
                container.AddManipulator(new ContextualMenuManipulator(evt => BuildContextMenu(evt, container)));
                return container;
            };

            _treeView.bindItem = (element, i) =>
            {
                element.Clear(); 
                var item = _treeView.GetItemDataForIndex<object>(i);
                element.userData = new RowContext { Item = item, Index = i };
                if (item is ScriptableObject obj)
                {
                    element.name = obj.name;
                    VisualElement row = item switch
                    {
                        KeyContainer key => new KeyRow(key),
                        FactDefinition fact => new FactRow(fact, GetParentKey(i)),
                        EventDefinition @event => new EventRow(@event, GetParentKey(i)),
                        _ => null
                    };

                    if (row != null)
                    {
                        element.focusable = false;
                        element.pickingMode = PickingMode.Ignore;
                        element.Add(row);
                    }
                }
            };
            _treeView.SetRootItems(treeItems);
            _treeView.Rebuild();
            LoadTreeViewState();
        }

        private void BuildContextMenu(ContextualMenuPopulateEvent evt, VisualElement container)
        {
            if (container.userData is not RowContext context) return;

            _lastMousePosition = Event.current.mousePosition;
            var item = context.Item;
            var index = context.Index;
            var parentKey = GetParentKey(index);

            if (item is KeyContainer key)
            {
                evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(parentKey, item as ScriptableObject));
                evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(parentKey, item as ScriptableObject));

                if (!Application.isPlaying)
                {
                    evt.menu.AppendAction("Add New Key", (e) => FactEditorUtils.ShowAddNewKeyPopup(key, "", _lastMousePosition));
                    evt.menu.AppendAction("Add New Fact", (e) => FactEditorUtils.ShowAddNewFactPopup(key, "", true, _lastMousePosition));
                }

                evt.menu.AppendAction("Add Existing Fact", (e) => FactEditorUtils.ShowAddExistingFactPopup(key, "", _lastMousePosition));

                if (!Application.isPlaying)
                    evt.menu.AppendAction("Add New Event", (e) => FactEditorUtils.ShowAddNewEventPopup(key, "", true, _lastMousePosition));

                evt.menu.AppendAction("Add Existing Event", (e) => FactEditorUtils.ShowAddExistingEventPopup(key, "", _lastMousePosition));

                if (!Application.isPlaying)
                    evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteKeyModal(key));
            }
            else if (item is FactDefinition fact)
            {
                if (!Application.isPlaying || _view == 1)
                {
                    evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(parentKey, item as ScriptableObject));
                    evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(parentKey, item as ScriptableObject));
                }
                if (!Application.isPlaying)
                {
                    if (_view == 0) evt.menu.AppendAction("Remove", (e) => FactEditorUtils.RemoveFact(parentKey, fact));
                    evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteFactModal(fact));
                }
            }
            else if (item is EventDefinition @event)
            {
                if (!Application.isPlaying || _view == 2)
                {
                    evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(parentKey, item as ScriptableObject));
                    evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(parentKey, item as ScriptableObject));
                }
                if (!Application.isPlaying)
                {
                    if (_view == 0) evt.menu.AppendAction("Remove", (e) => FactEditorUtils.RemoveEvent(parentKey, @event));
                    evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteEventModal(@event));
                }
            }
        }

        private void EnsureGuidsExist<T>(IEnumerable<T> items) where T : ScriptableObject
        {
            foreach (var item in items)
            {
                var so = new SerializedObject(item);
                var guidProp = so.FindProperty("Guid") ?? so.FindProperty("<Guid>k__BackingField");

                if (guidProp != null && string.IsNullOrWhiteSpace(guidProp.stringValue))
                {
                    guidProp.stringValue = FactEditorUtils.GenerateGuid(item.name);
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private bool MatchesFilter(string value, Type type, string filter)
        {
            if (string.IsNullOrEmpty(filter)) return true;
            value = value.ToLowerInvariant();

            foreach (var f in filter.Split('+', StringSplitOptions.RemoveEmptyEntries))
            {
                var token = f.Trim().ToLowerInvariant();
                if (token.Length < 2 || token[1] != ':')
                {
                    if (value.Contains(token)) return true;
                    continue;
                }

                var prefix = token[0];
                var search = token[2..];

                if (value.Contains(search) &&
                    ((prefix == 'f' && type == typeof(FactDefinition)) ||
                     (prefix == 'k' && type == typeof(KeyContainer)) ||
                     (prefix == 'e' && type == typeof(EventDefinition))))
                    return true;
            }
            return false;
        }

        private TreeViewItemData<object>? FilterKeyView(KeyContainer key, string filter, ref int id)
        {
            if (key == null) return null;

            var children = new List<TreeViewItemData<object>>();
            bool addAll = string.IsNullOrEmpty(filter);
            bool matchesSelf = MatchesFilter(key.name, typeof(KeyContainer), filter);
            bool matchesChild = false;

            if (addAll || matchesSelf) filter = null;

            foreach (var fact in key.DefinedFacts)
            {
                if (fact == null)
                {
                    Print.MWarn($"{key.name} contains null fact");
                    continue;
                }
                if (MatchesFilter(fact.name, typeof(FactDefinition), filter))
                {
                    children.Add(new TreeViewItemData<object>(id++, fact));
                    matchesChild = true;
                }
            }

            foreach (var @event in key.DefinedEvents)
            {
                if (@event == null) continue;
                if (MatchesFilter(@event.name, typeof(EventDefinition), filter))
                {
                    children.Add(new TreeViewItemData<object>(id++, @event));
                    matchesChild = true;
                }
            }

            foreach (var childKey in key.Children)
            {
                var childItem = FilterKeyView(childKey, filter, ref id);
                if (childItem.HasValue)
                {
                    children.Add(childItem.Value);
                    matchesChild = true;
                }
            }

            return (addAll || matchesSelf || matchesChild) ? new TreeViewItemData<object>(id++, key, children) : null;
        }

        private List<TreeViewItemData<object>> FilterFactView(FactDatabase db, string filter, ref int id)
        {
            int nextId = id;
            var result = db.FactStorage
                .Where(fact => fact != null && MatchesFilter(fact.name, typeof(FactDefinition), filter))
                .Select(fact => new TreeViewItemData<object>(nextId++, fact))
                .ToList();

            id = nextId;
            return result;
        }

        private List<TreeViewItemData<object>> FilterEventView(FactDatabase db, string filter, ref int id)
        {
            int nextId = id;
            var result = db.EventStorage
                .Where(evt => evt != null && MatchesFilter(evt.name, typeof(EventDefinition), filter))
                .Select(evt => new TreeViewItemData<object>(nextId++, evt))
                .ToList();
            id = nextId;
            return result;

        }

        private void SaveTreeViewState()
        {
            if (_treeView != null && string.IsNullOrWhiteSpace(_filter))
            {
                var collapsed = new List<string>();
                var controller = _treeView.viewController;
                if (controller == null) return;

                bool shouldSave = false;
                foreach (var id in controller.GetAllItemIds())
                {
                    if (_treeView.GetItemDataForId<object>(id) is ScriptableObject obj && obj is KeyContainer)
                    {
                        shouldSave = true;
                        if (!controller.IsExpanded(id))
                        {
                            var so = new SerializedObject(obj);
                            var guidProp = so.FindProperty("Guid") ?? so.FindProperty("<Guid>k__BackingField");
                            string guid = guidProp != null ? guidProp.stringValue : obj.GetInstanceID().ToString();
                            collapsed.Add(guid);
                        }
                    }
                }

                if (shouldSave)
                {
                    string newState = string.Join(",", collapsed);
                    string prefKey = $"{nameof(FactsEditorWindow)}.TreeViewState";
                    if (EditorPrefs.GetString(prefKey, string.Empty) != newState)
                    {
                        EditorPrefs.SetString(prefKey, newState);
                    }
                }
            }
        }

        private void LoadTreeViewState()
        {
            if (_treeView == null) return;

            if (string.IsNullOrWhiteSpace(_filter) && _view == 0)
            {
                string str = EditorPrefs.GetString($"{nameof(FactsEditorWindow)}.TreeViewState", string.Empty);
                if (string.IsNullOrWhiteSpace(str))
                {
                    _treeView.ExpandAll();
                    return;
                }

                var collapsed = str.Split(",").ToList();
                var controller = _treeView.viewController;
                if (controller == null) return;

                foreach (var id in controller.GetAllItemIds())
                {
                    var index = controller.GetIndexForId(id);
                    if (controller.GetItemForIndex(index) is ScriptableObject obj)
                    {
                        var so = new SerializedObject(obj);
                        var guidProp = so.FindProperty("Guid") ?? so.FindProperty("<Guid>k__BackingField");
                        string guid = guidProp != null ? guidProp.stringValue : obj.GetInstanceID().ToString();

                        if (collapsed.Contains(guid)) _treeView.CollapseItem(id, false);
                        else _treeView.ExpandItem(id, false);
                    }
                    else
                    {
                        _treeView.ExpandItem(id, false);
                    }
                }
            }
            else
            {
                _treeView.ExpandAll();
            }
        }

        private KeyContainer GetParentKey(int index)
        {
            if (_treeView == null || index < 0) return null;

            var item = _treeView.GetItemDataForIndex<object>(index);
            if (item == null) return null;

            for (int i = index - 1; i > -1; i--)
            {
                if (_treeView.GetItemDataForIndex<object>(i) is KeyContainer parent)
                {
                    if (item is KeyContainer key && parent.Children.Contains(key)) return parent;
                    if (item is FactDefinition fact && parent.DefinedFacts.Contains(fact)) return parent;
                    if (item is EventDefinition @event && parent.DefinedEvents.Contains(@event)) return parent;
                }
            }
            return null;
        }

        private void OnGameStateChange(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    SaveTreeViewState();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.EnteredEditMode:
                    BuildTreeView();
                    break;
            }
        }

        private void CheckForDatabaseChanges()
        {
            var db = FactEditorUtils.Database;
            if (db == null) return;

            string path = AssetDatabase.GetAssetPath(db);
            int currentHash = AssetDatabase.GetAssetDependencyHash(path).GetHashCode();

            if (currentHash != _lastAssetHash)
            {
                _lastAssetHash = currentHash;
                BuildTreeView();
            }
        }

        private void OnDisable()
        {
            SaveTreeViewState();
            EditorApplication.update -= CheckForDatabaseChanges;
            EditorApplication.playModeStateChanged -= OnGameStateChange;
        }
    }
}