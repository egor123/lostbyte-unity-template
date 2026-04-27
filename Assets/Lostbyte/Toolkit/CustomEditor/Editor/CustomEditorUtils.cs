using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
    public static class CustomEditorUtils
    {
        public static Rect GetNextSegment(this Rect rect, float nextWidth, Rect parentRect)
        {
            rect.x += rect.width;
            rect.width = nextWidth;
            parentRect.xMax = Mathf.Min(parentRect.xMax);
            return rect;
        }
        public static Rect GetFirtstSegment(this Rect rect, float width)
        {
            rect.width = Mathf.Min(width, rect.width);
            return rect;
        }
        public static BindableElement CreatePropertyField(Type type, string label)
        {
            if (PropertyFieldRegistry.TryCreate(type, label, out var custom)) return custom;
            if (type == typeof(int)) return new IntegerField(label);
            if (type == typeof(float)) return new FloatField(label);
            if (type == typeof(string)) return new TextField(label);
            if (type == typeof(bool)) return new Toggle(label);
            if (type == typeof(Vector2)) return new Vector2Field(label);
            if (type == typeof(Vector3)) return new Vector3Field(label);
            if (type == typeof(Vector4)) return new Vector4Field(label);
            if (type == typeof(Color)) return new ColorField(label);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return new ObjectField(label) { objectType = type, allowSceneObjects = true };
            if (type.IsEnum) return new EnumField(label, (Enum)Activator.CreateInstance(type));
            return new Label($"{label} (Unsupported Type: {type.Name})");
        }
    }
}
