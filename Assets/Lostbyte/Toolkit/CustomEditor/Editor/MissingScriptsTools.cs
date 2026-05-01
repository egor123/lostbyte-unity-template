using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
    public static class MissingScriptsTools
    {
        private const string k_menuFolder = "Tools/Missing Scripts/";
        [MenuItem(k_menuFolder + "Find", priority = 0)]
        public static int FindMissingScripts()
        {
            int count = 0;
            Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(o => o.GetComponents<Component>().Any(c => c == null))
                .ForEach(o =>
                {
                    bool isHidden = o.hideFlags.HasFlag(HideFlags.HideInHierarchy);
                    DebugLogger.ManagerLogWarning($"GameObject {o.name} has missing script! {(isHidden ? " [HideInHierarchy]" : "")}", o);
                    count++;
                });
            DebugLogger.ManagerLog($"Found {count} objects with missing scripts");
            return count;
        }
        [MenuItem(k_menuFolder + "Delete", priority = 1)]
        public static int DeleteMissingScripts()
        {
            int count = 0;
            Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(o => o.GetComponents<Component>().Any(c => c == null))
                .ForEach(o =>
                {
                    Undo.RegisterFullObjectHierarchyUndo(o, "Remove Missing Scripts");
                    count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(o);
                    DebugLogger.ManagerLog($"Removed missing scripts from GameObject {o.name}", o);
                });
            DebugLogger.ManagerLog($"Removed {count} missing scripts");
            return count;
        }
    }
}
