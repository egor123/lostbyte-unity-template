using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public static class FactReactionMenuBuilder
    {
        public static void ShowAddReactionMenu(VisualElement anchor, FactDefinition fact, Action<Type> onReactionSelected)
        {
            var menu = new GenericMenu();
            Type factType = fact.GenericType;

            var reactionTypes = TypeCache.GetTypesDerivedFrom<FactReaction>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);

            foreach (var type in reactionTypes)
            {
                var supportedAttr = (SupportedFactTypesAttribute)Attribute.GetCustomAttribute(type, typeof(SupportedFactTypesAttribute));

                if (supportedAttr == null || supportedAttr.IsTypeSupported(factType))
                {
                    string baseName = ObjectNames.NicifyVariableName(type.Name.Replace("Reaction", ""));
                    var tagAttr = (TagAttribute)Attribute.GetCustomAttribute(type, typeof(TagAttribute));

                    string menuPath;
                    if (tagAttr != null && !string.IsNullOrWhiteSpace(tagAttr.Tag))
                    {
                        string cleanTag = tagAttr.Tag.TrimEnd('/');
                        menuPath = $"{cleanTag}/{baseName}";
                    }
                    else
                    {
                        menuPath = baseName;
                    }

                    menu.AddItem(new GUIContent(menuPath), false, () => onReactionSelected(type));
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No compatible reactions found for this type"));
            }

            menu.DropDown(anchor.worldBound);
        }
        public static void ShowAddReactionMenu(VisualElement anchor, EventDefinition fact, Action<Type> onReactionSelected)
        {
            var menu = new GenericMenu();
            var reactionTypes = TypeCache.GetTypesDerivedFrom<EventReaction>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);

            foreach (var type in reactionTypes)
            {
                string baseName = ObjectNames.NicifyVariableName(type.Name.Replace("Reaction", ""));
                var tagAttr = (TagAttribute)Attribute.GetCustomAttribute(type, typeof(TagAttribute));
                string menuPath;
                if (tagAttr != null && !string.IsNullOrWhiteSpace(tagAttr.Tag))
                {
                    string cleanTag = tagAttr.Tag.TrimEnd('/');
                    menuPath = $"{cleanTag}/{baseName}";
                }
                else
                {
                    menuPath = baseName;
                }
                menu.AddItem(new GUIContent(menuPath), false, () => onReactionSelected(type));
            }
            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No compatible reactions found for this type"));
            }
            menu.DropDown(anchor.worldBound);
        }
    }
}