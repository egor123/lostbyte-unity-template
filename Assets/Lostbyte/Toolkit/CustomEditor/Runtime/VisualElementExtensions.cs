using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor
{
    public static class VisualElementExtensions
    {
        #region Borders

        public static T SetBorder<T>(this T element, StyleFloat width, Color color) where T : VisualElement
        {
            return element.SetBorderWidth(width).SetBorderColor(color);
        }
        public static T SetBorder<T>(this T element, StyleFloat width, StyleLength radius, Color color) where T : VisualElement
        {
            return element.SetBorderWidth(width).SetBorderRadius(radius).SetBorderColor(color);
        }

        public static T SetBorderWidth<T>(this T element, StyleFloat width) where T : VisualElement
        {
            element.style.borderLeftWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderBottomWidth = width;
            return element;
        }

        public static T SetBorderWidth<T>(this T element, StyleFloat vertical, StyleFloat horizontal) where T : VisualElement
        {
            element.style.borderTopWidth = vertical;
            element.style.borderBottomWidth = vertical;
            element.style.borderLeftWidth = horizontal;
            element.style.borderRightWidth = horizontal;
            return element;
        }

        public static T SetBorderWidth<T>(this T element, StyleFloat top, StyleFloat right, StyleFloat bottom, StyleFloat left) where T : VisualElement
        {
            element.style.borderTopWidth = top;
            element.style.borderRightWidth = right;
            element.style.borderBottomWidth = bottom;
            element.style.borderLeftWidth = left;
            return element;
        }

        public static T SetColor<T>(this T element, StyleColor color) where T : VisualElement
        {
            element.style.color = color;
            return element;
        }


        public static T SetBorderColor<T>(this T element, StyleColor color) where T : VisualElement
        {
            element.style.borderTopColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            return element;
        }

        public static T SetBorderRadius<T>(this T element, StyleLength radius) where T : VisualElement
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
            return element;
        }

        public static T SetBorderRadius<T>(this T element, StyleLength topLeftBottomRight, StyleLength topRightBottomLeft) where T : VisualElement
        {
            element.style.borderTopLeftRadius = topLeftBottomRight;
            element.style.borderBottomRightRadius = topLeftBottomRight;
            element.style.borderTopRightRadius = topRightBottomLeft;
            element.style.borderBottomLeftRadius = topRightBottomLeft;
            return element;
        }

        public static T SetBorderRadius<T>(this T element, StyleLength topLeft, StyleLength topRight, StyleLength bottomRight, StyleLength bottomLeft) where T : VisualElement
        {
            element.style.borderTopLeftRadius = topLeft;
            element.style.borderTopRightRadius = topRight;
            element.style.borderBottomRightRadius = bottomRight;
            element.style.borderBottomLeftRadius = bottomLeft;
            return element;
        }

        #endregion

        #region Margin & Padding

        public static T SetMargin<T>(this T element, StyleLength margin) where T : VisualElement
        {
            element.style.marginTop = margin;
            element.style.marginBottom = margin;
            element.style.marginLeft = margin;
            element.style.marginRight = margin;
            return element;
        }

        public static T SetMargin<T>(this T element, StyleLength vertical, StyleLength horizontal) where T : VisualElement
        {
            element.style.marginTop = vertical;
            element.style.marginBottom = vertical;
            element.style.marginLeft = horizontal;
            element.style.marginRight = horizontal;
            return element;
        }

        public static T SetMargin<T>(this T element, StyleLength top, StyleLength horizontal, StyleLength bottom) where T : VisualElement
        {
            element.style.marginTop = top;
            element.style.marginLeft = horizontal;
            element.style.marginRight = horizontal;
            element.style.marginBottom = bottom;
            return element;
        }

        public static T SetMargin<T>(this T element, StyleLength top, StyleLength right, StyleLength bottom, StyleLength left) where T : VisualElement
        {
            element.style.marginTop = top;
            element.style.marginRight = right;
            element.style.marginBottom = bottom;
            element.style.marginLeft = left;
            return element;
        }

        public static T SetPadding<T>(this T element, StyleLength padding) where T : VisualElement
        {
            element.style.paddingTop = padding;
            element.style.paddingBottom = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
            return element;
        }

        public static T SetPadding<T>(this T element, StyleLength vertical, StyleLength horizontal) where T : VisualElement
        {
            element.style.paddingTop = vertical;
            element.style.paddingBottom = vertical;
            element.style.paddingLeft = horizontal;
            element.style.paddingRight = horizontal;
            return element;
        }

        public static T SetPadding<T>(this T element, StyleLength top, StyleLength horizontal, StyleLength bottom) where T : VisualElement
        {
            element.style.paddingTop = top;
            element.style.paddingLeft = horizontal;
            element.style.paddingRight = horizontal;
            element.style.paddingBottom = bottom;
            return element;
        }

        public static T SetPadding<T>(this T element, StyleLength top, StyleLength right, StyleLength bottom, StyleLength left) where T : VisualElement
        {
            element.style.paddingTop = top;
            element.style.paddingRight = right;
            element.style.paddingBottom = bottom;
            element.style.paddingLeft = left;
            return element;
        }

        public static T ClearPaddingAndMargin<T>(this T element) where T : VisualElement
        {
            return element.SetMargin(0).SetPadding(0);
        }

        #endregion

        #region Size & Position

        public static T SetSize<T>(this T element, StyleLength width, StyleLength height) where T : VisualElement
        {
            element.style.width = width;
            element.style.height = height;
            return element;
        }

        public static T SetMinSize<T>(this T element, StyleLength minWidth, StyleLength minHeight) where T : VisualElement
        {
            element.style.minWidth = minWidth;
            element.style.minHeight = minHeight;
            return element;
        }

        public static T SetMaxSize<T>(this T element, StyleLength maxWidth, StyleLength maxHeight) where T : VisualElement
        {
            element.style.maxWidth = maxWidth;
            element.style.maxHeight = maxHeight;
            return element;
        }

        public static T SetAbsolutePosition<T>(this T element, StyleLength top, StyleLength right, StyleLength bottom, StyleLength left) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.top = top;
            element.style.right = right;
            element.style.bottom = bottom;
            element.style.left = left;
            return element;
        }

        #endregion

        #region Flexbox

        public static T SetFlex<T>(this T element, StyleFloat grow, StyleFloat shrink) where T : VisualElement
        {
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
            return element;
        }

        public static T SetFlexDirection<T>(this T element, FlexDirection direction) where T : VisualElement
        {
            element.style.flexDirection = direction;
            return element;
        }

        public static T SetJustifyContent<T>(this T element, Justify justify) where T : VisualElement
        {
            element.style.justifyContent = justify;
            return element;
        }

        public static T SetAlignItems<T>(this T element, Align align) where T : VisualElement
        {
            element.style.alignItems = align;
            return element;
        }

        #endregion

        #region Appearance & Visibility

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

        public static T SetBackgroundColor<T>(this T element, Color color) where T : VisualElement
        {
            element.style.backgroundColor = color;
            return element;
        }

        public static T SetBackgroundImage<T>(this T element, Texture2D texture) where T : VisualElement
        {
            element.style.backgroundImage = new StyleBackground(texture);
            return element;
        }

        public static T SetOpacity<T>(this T element, StyleFloat opacity) where T : VisualElement
        {
            element.style.opacity = opacity;
            return element;
        }

        #endregion

        #region USS Classes

        public static T AddClass<T>(this T element, string className) where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }

        public static T RemoveClass<T>(this T element, string className) where T : VisualElement
        {
            element.RemoveFromClassList(className);
            return element;
        }

        public static T ToggleClass<T>(this T element, string className, bool enable) where T : VisualElement
        {
            element.EnableInClassList(className, enable);
            return element;
        }

        #endregion

        #region Interactions

        public static T SetEnabledState<T>(this T element, bool enabled) where T : VisualElement
        {
            element.SetEnabled(enabled);
            return element;
        }

        public static T SetTooltip<T>(this T element, string tooltip) where T : VisualElement
        {
            element.tooltip = tooltip;
            return element;
        }

        public static T AddContextualMenu<T>(this T element, Action<ContextualMenuPopulateEvent> buildMenu) where T : VisualElement
        {
            element.AddManipulator(new ContextualMenuManipulator(buildMenu));
            return element;
        }

        public static T SetFontStyle<T>(this T element, FontStyle style) where T : VisualElement
        {
            element.style.unityFontStyleAndWeight = style;
            return element;
        }

        public static T OnClick<T>(this T element, Action callback) where T : VisualElement
        {
            element.RegisterCallback<ClickEvent>(_ => callback?.Invoke());
            return element;
        }

        #endregion

        #region Flexbox Layout

        public static T MakeRow<T>(this T element) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Row;
            return element;
        }

        public static T MakeRow<T>(this T element, Align align) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Row;
            element.style.alignItems = align;
            return element;
        }

        public static T MakeRow<T>(this T element, Justify justify) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Row;
            element.style.justifyContent = justify;
            return element;
        }

        public static T MakeRow<T>(this T element, Align align, Justify justify) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Row;
            element.style.alignItems = align;
            element.style.justifyContent = justify;
            return element;
        }

        public static T MakeColumn<T>(this T element) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Column;
            return element;
        }

        public static T MakeColumn<T>(this T element, Align align) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Column;
            element.style.alignItems = align;
            return element;
        }

        public static T MakeColumn<T>(this T element, Justify justify) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Column;
            element.style.justifyContent = justify;
            return element;
        }
        public static T MakeColumn<T>(this T element, Align align, Justify justify) where T : VisualElement
        {
            element.style.flexDirection = FlexDirection.Column;
            element.style.alignItems = align;
            element.style.justifyContent = justify;

            return element;
        }

        public static T SetFlexWrap<T>(this T element, Wrap wrap) where T : VisualElement
        {
            element.style.flexWrap = wrap;
            return element;
        }

        public static T SetFlexBasis<T>(this T element, StyleLength basis) where T : VisualElement
        {
            element.style.flexBasis = basis;
            return element;
        }

        public static T SetAlignSelf<T>(this T element, Align align) where T : VisualElement
        {
            element.style.alignSelf = align;
            return element;
        }

        #endregion

        #region Element Creation

        public static T SetParent<T>(this T element, VisualElement parent) where T: VisualElement
        {
            parent.Add(element);
            return element;
        }

        public static VisualElement AddRow(this VisualElement parent, string name = "")
        {
            var container = new VisualElement { name = name }.MakeRow();
            parent.Add(container);
            return container;
        }

        public static VisualElement AddColumn(this VisualElement parent, string name = "")
        {
            var container = new VisualElement { name = name }.MakeColumn();
            parent.Add(container);
            return container;
        }

        public static Label AddLabel(this VisualElement parent, string text, string name = "")
        {
            var label = new Label(text) { name = name };
            parent.Add(label);
            return label;
        }

        public static Button AddButton(this VisualElement parent, string text, Action onClick = null, string name = "")
        {
            var button = new Button(onClick) { text = text, name = name };
            parent.Add(button);
            return button;
        }

        public static TextField AddTextField(this VisualElement parent, string label = "", string defaultValue = "", string name = "")
        {
            var textField = new TextField(label) { value = defaultValue, name = name };
            parent.Add(textField);
            return textField;
        }

        public static Toggle AddToggle(this VisualElement parent, string label = "", bool defaultValue = false, string name = "")
        {
            var toggle = new Toggle(label) { value = defaultValue, name = name };
            parent.Add(toggle);
            return toggle;
        }

        public static VisualElement AddSpace(this VisualElement parent, float size = 10f)
        {
            var spacer = new VisualElement();
            if (parent.resolvedStyle.flexDirection == FlexDirection.Row)
                spacer.style.width = size;
            else
                spacer.style.height = size;

            parent.Add(spacer);
            return spacer;
        }

        #endregion
    }
}