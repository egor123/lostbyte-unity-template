using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Lostbyte.Toolkit.TimelineExtensions.Editor
{
    [CustomPropertyDrawer(typeof(BaseTimelineAction), true)]
    public class BaseTimelineActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect popupRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            string currentType = string.IsNullOrEmpty(property.managedReferenceFullTypename)
                ? "Select Action Type..."
                : property.managedReferenceFullTypename.Split('.')[^1];
            if (EditorGUI.DropdownButton(popupRect, new GUIContent(currentType), FocusType.Keyboard))
                ShowFilteredMenu(property, label);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var child = property.Copy();
            var end = child.GetEndProperty();
            if (child.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                    position.height = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(position, child, true);
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                } while (child.NextVisible(false));
            }
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var child = property.Copy();
            var end = child.GetEndProperty();
            if (child.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                    height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                } while (child.NextVisible(false));
            }
            return height;
        }
        private void ShowFilteredMenu(SerializedProperty property, GUIContent label)
        {
            GenericMenu menu = new();
            UnityEngine.Object boundObj = GetBoundGameObject(property);
            foreach (var type in TypeCache.GetTypesDerivedFrom<BaseTimelineAction>().Where(t => !t.IsAbstract))
            {
                var attr = type.GetCustomAttribute<TimelineExtensionAttribute>();
                Type reqType = attr?.BindingType ?? typeof(GameObject);
                if (IsAssignable(boundObj, reqType))
                {
                    string displayName = attr?.Name ?? type.Name;
                    menu.AddItem(new GUIContent(displayName), false, () =>
                    {
                        property.serializedObject.Update();
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        var timelineClip = GetOwningClip(property);
                        if (timelineClip != null)
                        {
                            timelineClip.displayName = displayName;
                            TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
                        }
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }
            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("No Valid Actions (Check Track Binding)"));
            menu.ShowAsContext();
        }

        private bool IsAssignable(UnityEngine.Object boundObj, Type reqType)
        {
            if (boundObj == null) return false;
            Type type = boundObj.GetType();
            if (type == reqType || type.IsAssignableFrom(reqType)) return true;
            if (typeof(Component).IsAssignableFrom(reqType))
            {
                if (boundObj is Component c && c.GetComponent(reqType) != null) return true;
                if (boundObj is GameObject g && g.GetComponent(reqType) != null) return true;
            }
            return false;
        }

        private TimelineClip GetOwningClip(SerializedProperty property)
        {
            if (TimelineEditor.inspectedDirector?.playableAsset is not TimelineAsset timeline ||
                property.serializedObject.targetObject is not UniversalClip asset) return null;
            foreach (var track in timeline.GetOutputTracks())
            {
                var clip = track.GetClips().FirstOrDefault(c => c.asset == asset);
                if (clip != null) return clip;
            }
            return null;
        }

        private UnityEngine.Object GetBoundGameObject(SerializedProperty property)
        {
            var clip = GetOwningClip(property);
            var track = clip?.GetParentTrack();
            if (track == null) return null;
            return TimelineEditor.inspectedDirector.GetGenericBinding(track);

        }
    }
}