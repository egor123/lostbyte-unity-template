using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public readonly struct FieldPath : IEquatable<FieldPath>
    {
        private readonly Tuple<FieldInfo, int>[] _steps;
        public readonly string Name;
        public readonly Type FieldType;
        public FieldPath(FieldInfo field, int idx = -1)
        {
            _steps = new Tuple<FieldInfo, int>[] { new(field, idx) };
            Name = field.Name;
            FieldType = field.FieldType;
        }
        private FieldPath(Tuple<FieldInfo, int>[] steps)
        {
            _steps = steps;
            Name = steps[^1].Item1.Name;
            FieldType = steps[^1].Item1.FieldType;

        }
        public int Depth => _steps?.Length ?? 0;

        public FieldPath SetIndexAtDepth(int depth, int newIndex)
        {
            var newSteps = (Tuple<FieldInfo, int>[])_steps.Clone();
            newSteps[depth] = new Tuple<FieldInfo, int>(newSteps[depth].Item1, newIndex);
            return new FieldPath(newSteps);
        }
        public FieldPath Append(FieldInfo field, int index = -1)
        {
            var newSteps = new Tuple<FieldInfo, int>[_steps.Length + 1];
            Array.Copy(_steps, 0, newSteps, 0, _steps.Length);
            newSteps[^1] = new Tuple<FieldInfo, int>(field, index);
            return new FieldPath(newSteps);
        }
        public FieldPath WithIndex(int index)
        {
            var newSteps = (Tuple<FieldInfo, int>[])_steps.Clone();
            newSteps[^1] = new Tuple<FieldInfo, int>(_steps[^1].Item1, index);
            return new FieldPath(newSteps);
        }

        public object GetValue(object rootNode)
        {
            if (_steps == null || _steps.Length == 0) return null;

            object current = rootNode;

            foreach (var (field, idx) in _steps)
            {
                if (current == null) return null;

                current = field.GetValue(current);

                if (current == null) return null;

                if (idx >= 0)
                {
                    if (current is Array arr)
                    {
                        if (idx >= arr.Length) return null;
                        current = arr.GetValue(idx);
                    }
                    else if (current is IList list)
                    {
                        if (idx >= list.Count) return null;
                        current = list[idx];
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Field {field.Name} is not indexable but index {idx} was provided.");
                    }
                }
            }

            return current;
        }

        public void SetValue(object rootNode, object value)
        {
            if (_steps == null || _steps.Length == 0) return;

            object[] chain = new object[_steps.Length + 1];
            chain[0] = rootNode;

            for (int i = 0; i < _steps.Length; i++)
            {
                var (field, idx) = _steps[i];
                object parent = chain[i];

                if (parent == null) return;

                object current = field.GetValue(parent);

                if (idx >= 0)
                {
                    Type elementType = field.FieldType.IsArray
                        ? field.FieldType.GetElementType()
                        : (field.FieldType.IsGenericType ? field.FieldType.GetGenericArguments()[0] : typeof(object));

                    if (current == null)
                    {
                        if (field.FieldType.IsArray)
                            current = Array.CreateInstance(elementType, idx + 1);
                        else
                            current = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

                        field.SetValue(parent, current); // Save it to the parent immediately
                    }

                    if (current is Array arr)
                    {
                        if (idx >= arr.Length)
                        {
                            Array newArr = Array.CreateInstance(elementType, idx + 1);
                            Array.Copy(arr, newArr, arr.Length);
                            current = newArr;
                            field.SetValue(parent, current); // Save the larger array to the parent
                        }

                        object element = ((Array)current).GetValue(idx);

                        if (element == null && i < _steps.Length - 1)
                        {
                            element = elementType == typeof(string) ? "" : Activator.CreateInstance(elementType);
                            ((Array)current).SetValue(element, idx);
                        }
                        current = element;
                    }
                    else if (current is IList list)
                    {
                        while (idx >= list.Count)
                        {
                            list.Add(elementType.IsValueType ? Activator.CreateInstance(elementType) : null);
                        }

                        object element = list[idx];
                        if (element == null && i < _steps.Length - 1)
                        {
                            element = elementType == typeof(string) ? "" : Activator.CreateInstance(elementType);
                            list[idx] = element;
                        }
                        current = element;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Field {field.Name} is not indexable but index {idx} was provided.");
                    }
                }
                else
                {
                    if (current == null && i < _steps.Length - 1)
                    {
                        current = field.FieldType == typeof(string) ? "" : Activator.CreateInstance(field.FieldType);
                        field.SetValue(parent, current);
                    }
                }

                chain[i + 1] = current;
            }

            var (lastField, lastIdx) = _steps[^1];
            if (lastIdx < 0)
            {
                lastField.SetValue(chain[^2], value);
                chain[^1] = value;
            }
            else
            {
                object container = lastField.GetValue(chain[^2]);
                if (container is Array arr) arr.SetValue(value, lastIdx);
                else if (container is IList list) list[lastIdx] = value;
                chain[^1] = value;
            }

            for (int i = _steps.Length - 1; i >= 0; i--)
            {
                var (field, idx) = _steps[i];
                object parent = chain[i];
                object child = chain[i + 1];

                if (idx >= 0)
                {
                    object container = field.GetValue(parent);
                    if (container is Array arr) arr.SetValue(child, idx);
                    else if (container is IList list) list[idx] = child;
                }
                else
                {
                    field.SetValue(parent, child);
                }
                chain[i] = parent;
            }
        }

        public bool Equals(FieldPath other)
        {
            if (_steps.Length != other._steps.Length)
                return false;
            for (int i = 0; i < _steps.Length; i++)
            {
                if (!Equals(_steps[i].Item1, other._steps[i].Item1) ||
                    _steps[i].Item2 != other._steps[i].Item2)
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var (field, index) in _steps)
            {
                hash.Add(field);
                hash.Add(index);
            }
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            if (_steps == null || _steps.Length == 0)
                return "<empty>";

            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < _steps.Length; i++)
            {
                var (field, index) = _steps[i];

                if (i > 0)
                    sb.Append('.');

                sb.Append(field.Name);

                if (index >= 0)
                {
                    sb.Append('[');
                    sb.Append(index);
                    sb.Append(']');
                }
            }

            return sb.ToString();
        }
    }
}
