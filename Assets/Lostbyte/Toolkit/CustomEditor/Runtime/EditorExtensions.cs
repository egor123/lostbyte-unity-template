using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public static class EditorExtensions
    {
#if UNITY_EDITOR

        public const BindingFlags FIELD_FLAGS = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;        // public const BindingFlags FIELD_FLAGS = (BindingFlags)(-1);


        public static FieldInfo GetTargetField(this SerializedProperty property) => property.GetTargetDetails().fieldInfo;
        public static Type GetTargetType(this SerializedProperty property) => property.GetTargetDetails().type;
        public static object GetTargetObject(this SerializedProperty property) => property.GetTargetDetails().obj; 
        public static void SetTargetObject(this SerializedProperty property, object value)
        {
            var targetData = property.GetTargetDetails();
            if (targetData.index == -1)
            {
                if (targetData.fieldInfo != null)
                    targetData.fieldInfo.SetValue(targetData.target, value);
                else
                    targetData.propertyInfo?.SetValue(targetData.target, value);
            }
            else
            {
                if (targetData.target is IList list)
                    list[targetData.index] = value;
                else if (targetData.target is Array array)
                        array.SetValue(value, targetData.index);
            }
        }

        private static TargetData GetTargetDetails(this SerializedProperty property)
        {
            if (property == null) return new(null, null);

            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            Type type = obj.GetType();
            TargetData data = new(obj, null, type);
            string[] elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    string elementName = element[..element.IndexOf("[")];
                    int index = Convert.ToInt32(element[element.IndexOf("[")..].Replace("[", "").Replace("]", ""));
                    data = GetValue_Imp(data.obj, elementName, index);
                }
                else
                {
                    data = GetValue_Imp(data.obj, element);
                }
            }

            return data;
        }

        private static TargetData GetValue_Imp(object source, string name)
        {
            if (source == null) return new();
            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return new(f.GetValue(source), source, f.FieldType, fieldInfo: f);
                PropertyInfo p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null) return new(p.GetValue(source, null), source, p.PropertyType, propertyInfo: p);
                type = type.BaseType;
            }
            return new();
        }

        private static TargetData GetValue_Imp(object source, string name, int index)
        {
            TargetData data = GetValue_Imp(source, name);
            if (data.obj is not IEnumerable enumerable) return new();
            IEnumerator enumerator = enumerable.GetEnumerator();
            int i = 0;
            for (i = 0; i <= index; i++)
                if (!enumerator.MoveNext()) return new();
            Type elementType = data.type.IsArray ? data.type.GetElementType() : data.type.GetGenericArguments()[0];
            return new(enumerator.Current, data.obj, elementType, data.propertyInfo, data.fieldInfo, i); //FIXME ??? enumerator.GetType().GetGenericArguments()[0]
        }


        private struct TargetData
        {
            public object obj;
            public object target;
            public Type type;
            public PropertyInfo propertyInfo;
            public FieldInfo fieldInfo;
            public int index;
            public TargetData(object obj = null, object target = null, Type type = null, PropertyInfo propertyInfo = null, FieldInfo fieldInfo = null, int index = -1)
            {
                this.obj = obj;
                this.target = target;
                this.type = type;
                this.propertyInfo = propertyInfo;
                this.fieldInfo = fieldInfo;
                this.index = index;
            }
        }
        public static object InvokeMethod(this SerializedProperty property, string methodName, params object[] parameters)
        {
            var info = property.GetTargetDetails();
            return info.type.GetMethod(methodName).Invoke(info.obj, parameters);
        }
        public static Vector3 GetCastedVector3Value(this SerializedProperty property)
        {
            var type = property.GetTargetType();
            if (type == typeof(Vector2))
                return property.vector2Value;
            else if (type == typeof(Vector2Int))
                return (Vector3Int)property.vector2IntValue;
            else if (type == typeof(Vector3))
                return property.vector3Value;
            else if (type == typeof(Vector3Int))
                return property.vector3IntValue;
            return Vector3.zero;
        }
        public static void SetVectorValue(this SerializedProperty property, Vector3 value)
        {
            var type = property.GetTargetType();
            if (type == typeof(Vector2))
                property.vector2Value = value;
            else if (type == typeof(Vector2Int))
                property.vector2IntValue = new Vector2Int((int)value.x, (int)value.y);
            else if (type == typeof(Vector3))
                property.vector3Value = value;
            else if (type == typeof(Vector3Int))
                property.vector3IntValue = new Vector3Int((int)value.x, (int)value.y, (int)value.z);
        }

        public static bool IsArrayElement(this SerializedProperty property)
        {
            var path = property.propertyPath.Split(".");
            return path.Length > 1 && path[^2] == "Array";
        }
#endif
    }
}