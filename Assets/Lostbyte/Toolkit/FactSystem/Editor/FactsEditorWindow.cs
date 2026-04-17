using System.Collections.Generic;
using System.Linq;
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

        [MenuItem("Window/Facts/FactsEditorWindow")]
        public static void ShowFactsEditorWindow()
        {
            FactsEditorWindow wnd = GetWindow<FactsEditorWindow>();
            wnd.titleContent = new GUIContent("FactsEditorWindow");
        }
        public void CreateGUI()
        {
            if (m_VisualTreeAsset == null)
                m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.lostbyte.toolkit/FactSystem/Editor/FactsEditorWindow.uxml");
            if (m_StyleSheet == null)
                m_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.lostbyte.toolkit/FactSystem/Editor/FactsEditorWindow.uss");

            VisualElement root = rootVisualElement;
            m_VisualTreeAsset.CloneTree(root);
            root.styleSheets.Add(m_StyleSheet);

            _treeView = root.Q<TreeView>("tree-view");

            Button addBtn = root.Q<Button>("add-btn");
            addBtn.clicked += () =>
            {
                switch (_view)
                {
                    case 0:
                        FactEditorUtils.ShowAddNewKeyPopup(null, "", _lastMousePosition);
                        break;
                    case 1:
                        FactEditorUtils.ShowAddNewFactPopup(null, "", true, _lastMousePosition);
                        break;
                    case 2:
                        FactEditorUtils.ShowAddNewEventPopup(null, "", true, _lastMousePosition);
                        break;
                    default:
                        Debug.LogWarning("Unkown view");
                        break;
                }
            };
            Button compileBtn = root.Q<Button>("compile-btn");
            compileBtn.clicked += () =>
            {
                if (Application.isPlaying) Debug.LogWarning("Cannot compile when playing!");
                else FactCodeGenerator.Generate(FactEditorUtils.Database);
            };
            ToolbarSearchField searchBar = root.Q<ToolbarSearchField>("search-bar");
            searchBar.RegisterValueChangedCallback(evt =>
            {
                _filter = evt.newValue;
                SaveTreeViewState();
                BuildTreeView();
            });
            VisualElement inspectorPanel = root.Q<VisualElement>("inspector");
            _treeView.selectionChanged += (selectedItems) =>
            {
                inspectorPanel.Clear();

                if (selectedItems.Count() == 0) return;

                if (selectedItems.First() is ScriptableObject item)
                {
                    SerializedObject serializedObject = new(item);
                    var iterator = serializedObject.GetIterator();
                    iterator.NextVisible(true);

                    while (iterator.NextVisible(false))
                    {
                        PropertyField field = new(iterator.Copy());
                        field.Bind(serializedObject);
                        inspectorPanel.Add(field);
                    }
                }
            };
            DropdownField dropdown = root.Q<DropdownField>("tab-selector");
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

        private void BuildTreeView()
        {
            if (FactEditorUtils.Database == null || _treeView == null) return;

            foreach (var f in FactEditorUtils.Database.FactStorage)
            {
                if (string.IsNullOrWhiteSpace(f.Guid))
                {
                    f.Guid = FactEditorUtils.GenerateGuid(f.name);
                    EditorUtility.SetDirty(f);
                }
            }
            foreach (var f in FactEditorUtils.GetAllKeys())
            {
                if (string.IsNullOrWhiteSpace(f.Guid))
                {
                    f.Guid = FactEditorUtils.GenerateGuid(f.name);
                    EditorUtility.SetDirty(f);
                }
            }
            foreach (var f in FactEditorUtils.Database.EventStorage)
            {
                if (string.IsNullOrWhiteSpace(f.Guid))
                {
                    f.Guid = FactEditorUtils.GenerateGuid(f.name);
                    EditorUtility.SetDirty(f);
                }
            }

            var treeItems = new List<TreeViewItemData<object>>();
            int id = 0;


            switch (_view)
            {
                case 0:
                    foreach (var rootKey in FactEditorUtils.Database.RootKeys)
                    {

                        var item = FilterKeyView(rootKey, _filter, ref id);
                        if (item.HasValue) treeItems.Add(item.Value);
                    }
                    break;
                case 1:
                    FilterFactView(FactEditorUtils.Database, _filter, ref id).ForEach(treeItems.Add);
                    break;
                case 2:
                    FilterEventView(FactEditorUtils.Database, _filter, ref id).ForEach(treeItems.Add);
                    break;
                default:
                    Debug.LogWarning("Unknown view");
                    break;
            }



            _treeView.makeItem = () => new VisualElement();
            _treeView.bindItem = (element, i) =>
            {

                element.Clear();
                var item = _treeView.GetItemDataForIndex<object>(i);
                if (item is ScriptableObject obj)
                {
                    element.name = obj.name;

                    VisualElement row = null;
                    if (item is KeyContainer key)
                    {
                        row = new KeyRow(key);
                        row.AddManipulator(new ContextualMenuManipulator(evt =>
                        {
                            _lastMousePosition = Event.current.mousePosition;
                            evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(GetParentKey(i), GetItemByIndex(i)));
                            evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(GetParentKey(i), GetItemByIndex(i)));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Add New Key", (e) => FactEditorUtils.ShowAddNewKeyPopup(key, "", _lastMousePosition));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Add New Fact", (e) => FactEditorUtils.ShowAddNewFactPopup(key, "", true, _lastMousePosition));
                            evt.menu.AppendAction("Add Existing Fact", (e) => FactEditorUtils.ShowAddExistingFactPopup(key, "", _lastMousePosition));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Add New Event", (e) => FactEditorUtils.ShowAddNewEventPopup(key, "", true, _lastMousePosition));
                            evt.menu.AppendAction("Add Existing Event", (e) => FactEditorUtils.ShowAddExistingEventPopup(key, "", _lastMousePosition));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteKeyModal(key));
                        }));
                    }
                    else if (item is FactDefinition fact)
                    {
                        row = new FactRow(fact, GetParentKey(i));
                        row.AddManipulator(new ContextualMenuManipulator(evt =>
                        {
                            _lastMousePosition = Event.current.mousePosition;
                            if (!Application.isPlaying || _view == 1)
                                evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(GetParentKey(i), GetItemByIndex(i)));
                            if (!Application.isPlaying || _view == 1)
                                evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(GetParentKey(i), GetItemByIndex(i)));
                            if (!Application.isPlaying && _view == 0)
                                evt.menu.AppendAction("Remove", (e) => FactEditorUtils.RemoveFact(GetParentKey(i), fact));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteFactModal(fact));
                        }));
                    }
                    else if (item is EventDefinition @event)
                    {
                        row = new EventRow(@event, GetParentKey(i));
                        row.AddManipulator(new ContextualMenuManipulator(evt =>
                        {
                            _lastMousePosition = Event.current.mousePosition;
                            if (!Application.isPlaying || _view == 2)
                                evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(GetParentKey(i), GetItemByIndex(i)));
                            if (!Application.isPlaying || _view == 2)
                                evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(GetParentKey(i), GetItemByIndex(i)));
                            if (!Application.isPlaying && _view == 0)
                                evt.menu.AppendAction("Remove", (e) => FactEditorUtils.RemoveEvent(GetParentKey(i), @event));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteEventModal(@event));
                        }));
                    }
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

        private bool MatchesFilter(string value, string filter) => string.IsNullOrEmpty(filter) || value.ToLower().Contains(filter);

        private ScriptableObject GetItemByIndex(int index) => _treeView.GetItemDataForIndex<object>(index) as ScriptableObject;

        private TreeViewItemData<object>? FilterKeyView(KeyContainer key, string filter, ref int id)
        {
            var children = new List<TreeViewItemData<object>>();
            bool addAll = string.IsNullOrEmpty(filter);
            bool matchesSelf = MatchesFilter(key.name, filter);
            bool matchesChild = false;
            if (addAll || matchesSelf) filter = null;
            foreach (var fact in key.DefinedFacts)
            {
                if (MatchesFilter(fact.name, filter))
                {
                    children.Add(new TreeViewItemData<object>(id++, fact));
                    matchesChild = true;
                }
            }
            foreach (var @event in key.DefinedEvents)
            {
                if (MatchesFilter(@event.name, filter))
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
            List<TreeViewItemData<object>> items = new();
            foreach (var fact in db.FactStorage)
                if (MatchesFilter(fact.name, filter))
                    items.Add(new TreeViewItemData<object>(id++, fact));
            return items;
        }
        private List<TreeViewItemData<object>> FilterEventView(FactDatabase db, string filter, ref int id)
        {
            List<TreeViewItemData<object>> items = new();
            foreach (var @event in db.EventStorage)
                if (MatchesFilter(@event.name, filter))
                    items.Add(new TreeViewItemData<object>(id++, @event));
            return items;
        }

        private void SaveTreeViewState()
        {
            if (_treeView != null && string.IsNullOrWhiteSpace(_filter))
            {
                var collapsed = new List<string>();
                var controller = _treeView.viewController;
                bool save = false;
                if (controller == null) return;
                foreach (var id in controller.GetAllItemIds())
                {
                    if (_treeView.GetItemDataForId<object>(id) is ScriptableObject obj)
                    {
                        if (obj is KeyContainer)
                        {
                            save = true;
                            if (!controller.IsExpanded(id))
                            {
                                string guid = obj.GetInstanceID().ToString();
                                if (obj is KeyContainer k) guid = k.Guid;
                                else if (obj is Definition d) guid = d.Guid;
                                collapsed.Add(guid);
                            }
                        }
                    }
                }
                if (save)
                {
                    EditorPrefs.SetString($"{nameof(FactsEditorWindow)}.TreeViewState", string.Join(",", collapsed));
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
                List<string> collapsed = str.Split(",").ToList();
                var controller = _treeView.viewController;
                if (controller == null) return;
                foreach (var id in controller.GetAllItemIds())
                {
                    var index = controller.GetIndexForId(id);
                    if (controller.GetItemForIndex(index) is ScriptableObject obj)
                    {
                        string guid = obj.GetInstanceID().ToString();
                        if (obj is KeyContainer k) guid = k.Guid;
                        else if (obj is Definition d) guid = d.Guid;
                        if (collapsed.Contains(guid)) _treeView.CollapseItem(id, false);
                        else _treeView.ExpandItem(id, false);
                    }
                    else _treeView.ExpandItem(id, false);
                }
            }
            else
            {
                _treeView.ExpandAll();
            }
        }

        private Vector2 _lastMousePosition;


        private KeyContainer GetParentKey(int index)
        {
            if (_treeView == null) return null;
            var item = GetItemByIndex(index);
            if (item is KeyContainer key)
            {
                for (int i = index - 1; i > -1; i--)
                    if (GetItemByIndex(i) is KeyContainer parent && parent.Children.Contains(key)) return parent;
            }
            else if (item is FactDefinition fact)
            {
                for (int i = index - 1; i > -1; i--)
                    if (GetItemByIndex(i) is KeyContainer parent && parent.DefinedFacts.Contains(fact)) return parent;
            }
            else if (item is EventDefinition @event)
            {
                for (int i = index - 1; i > -1; i--)
                    if (GetItemByIndex(i) is KeyContainer parent && parent.DefinedEvents.Contains(@event)) return parent;
            }
            return null;
        }

        private void OnGameStateChange(PlayModeStateChange stateChange)
        {
            SaveTreeViewState();
            BuildTreeView();
        }
        private void CheckForDatabaseChanges()
        {
            var db = FactEditorUtils.Database;
            if (FactEditorUtils.Database == null) return;

            string path = AssetDatabase.GetAssetPath(db);
            int currentHash = AssetDatabase.GetAssetDependencyHash(path).GetHashCode();
            if (currentHash != _lastAssetHash)
            {
                _lastAssetHash = currentHash;
                SaveTreeViewState();
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