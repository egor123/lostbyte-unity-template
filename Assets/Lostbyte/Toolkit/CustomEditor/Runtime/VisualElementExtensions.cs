using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor
{
    public static class VisualElementExtensions
    {
        public static T SetBorderColor<T>(this T element, Color color) where T : VisualElement
        {
            StyleColor styleColor = new(color);
            element.style.borderTopColor = styleColor;
            element.style.borderBottomColor = styleColor;
            element.style.borderLeftColor = styleColor;
            element.style.borderRightColor = styleColor;
            return element;
        }
        public static T SetBorderWidth<T>(this T element, float width) where T : VisualElement
        {
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            return element;
        }
        public static T SetBorderRadius<T>(this T element, float radius) where T : VisualElement
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
            return element;
        }
        public static T SetMargin<T>(this T element, float margin) where T : VisualElement
        {
            element.style.marginTop = margin;
            element.style.marginBottom = margin;
            element.style.marginLeft = margin;
            element.style.marginRight = margin;
            return element;
        }

        public static T SetPadding<T>(this T element, float padding) where T : VisualElement
        {
            element.style.paddingTop = padding;
            element.style.paddingBottom = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
            return element;
        }
        public static T Show<T>(this T element) where T : VisualElement
        {
            element.style.display = DisplayStyle.Flex;
            return element;
        }
        public static T Hide<T>(this T element) where T : VisualElement
        {
            element.style.display = DisplayStyle.None;
            return element;
        }
        public static T SetVisible<T>(this T element, bool isVisible) where T : VisualElement
        {
            element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            return element;
        }
        public static T SetAbsolutePosition<T>(this T element, float top, float right, float bottom, float left) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.top = top;
            element.style.right = right;
            element.style.bottom = bottom;
            element.style.left = left;
            return element;
        }
        public static T SetSize<T>(this T element, float width, float height) where T : VisualElement
        {
            element.style.width = width;
            element.style.height = height;
            return element;
        }

        public static T SetFlex<T>(this T element, float grow, float shrink) where T : VisualElement
        {
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
            return element;
        }
    }
}