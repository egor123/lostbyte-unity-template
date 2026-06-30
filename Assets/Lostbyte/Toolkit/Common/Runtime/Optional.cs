using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    [Serializable]
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        [SerializeField] private bool m_hasValue;
        [SerializeField] private T m_value;

        public readonly bool HasValue => m_hasValue;
        public readonly T Value => m_value;

        public Optional(T value)
        {
            m_hasValue = true;
            m_value = value;
        }

        public Optional(T value, bool initialHasValue)
        {
            m_hasValue = initialHasValue;
            m_value = value;
        }
        public static implicit operator Optional<T>(T value) => new(value);
        public static explicit operator T(Optional<T> optional) => optional.Value;

        public T GetValueOrDefault(T defaultValue = default)
        {
            return m_hasValue ? m_value : defaultValue;
        }
        public override readonly bool Equals(object obj)
        {
            if (obj is Optional<T> other)
                return Equals(other);
            return false;
        }

        public readonly bool Equals(Optional<T> other)
        {
            if (!m_hasValue && !other.m_hasValue) return true;
            if (m_hasValue != other.m_hasValue) return false;
            return EqualityComparer<T>.Default.Equals(m_value, other.m_value);
        }

        public override readonly int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + m_hasValue.GetHashCode();
                if (m_hasValue && m_value != null)
                    hash = hash * 23 + m_value.GetHashCode();
                return hash;
            }
        }

        public override readonly string ToString()
        {
            return m_hasValue ? m_value?.ToString() ?? "null" : "No Value";
        }
    }
}