using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Lostbyte.Toolkit.Injection
{
    public class Injector
    {
        internal readonly Dictionary<Type, object> m_container = new();
        private readonly Injector m_parentInjector;
        private readonly GameObject m_rootObject;

        public Injector(GameObject rootObject)
        {
            m_rootObject = rootObject;
            m_container[typeof(Injector)] = this;
        }

        public Injector(Injector injector)
        {
            m_parentInjector = injector;
            m_container[typeof(Injector)] = this;
        }

        public Injector CreateContainer()
        {
            return new Injector(this);
        }

        public object Initialize(object obj)
        {
            // if (obj is INode node) node.injector = this;
            Inject(obj);
            return obj;
        }

        public T Initialize<T>(T obj)
        {
            // if (obj is INode node) node.injector = this;
            Inject(obj);
            return obj;
        }

        public T Create<T>()
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
            Inject(obj);
            return obj;
        }

        public object Create(Type type)
        {
            object obj = Activator.CreateInstance(type);
            Inject(obj);
            return obj;
        }

        public void Set<T>(T value)
        {
            m_container[typeof(T)] = value;
        }
        public void Set(Type type, object value)
        {
            m_container[type] = value;
        }
        public void SetDeep(object value)
        {
            Type type = value.GetType();
            TryToSet(type, value);
            foreach (Type i in type.GetInterfaces())
                TryToSet(i, value);
            foreach (var nestedType in type.GetNestedTypes())
            {
                TryToSet(nestedType, value);
                foreach (Type i in nestedType.GetInterfaces())
                    TryToSet(i, value);
            }
        }

        public bool TryToSet(Type key, object value)
        {
            if (m_container.ContainsKey(key)) return false;
            m_container[key] = value;
            return true;
        }

        private void Inject(object obj) //TODO caching ???
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance; //FIXME????
            Type type = obj.GetType();


            foreach (var field in type.GetFields(flags))
            {
                if (field.GetCustomAttribute<Inject>() != null)
                {
                    var value = GetInstance(field.FieldType);
                    if (value != null)
                        field.SetValue(obj, value);
                }
                else if (field.GetCustomAttribute<Initialize>() != null)
                {
                    object val = field.GetValue(obj);
                    if (val == null)
                        field.SetValue(obj, Initialize(Create(field.FieldType)));
                    else
                        Initialize(obj);
                }
            }
            foreach (var prop in type.GetProperties(flags))
            {
                if (prop.GetCustomAttribute<Inject>() != null)
                {
                    var value = GetInstance(prop.PropertyType);
                    if (value != null)
                        prop.SetValue(obj, value);
                }
                else if (prop.GetCustomAttribute<Initialize>() != null)
                {
                    object val = prop.GetValue(obj);
                    if (val == null)
                        prop.SetValue(obj, Initialize(Create(prop.PropertyType)));
                    else
                        Initialize(obj);
                }
            }

            foreach (var method in type.GetMethods(flags))
                if (method.GetCustomAttribute<Inject>() != null)
                    method.Invoke(obj, method.GetParameters().Select(p => GetInstance(p.GetType())).ToArray());

        }

        public T GetInstance<T>() => (T)GetInstance(typeof(T));
        public object GetInstance(Type type)
        {
            if (m_container.ContainsKey(type))
                return m_container[type];
            if (m_rootObject != null && type.IsAssignableFrom(typeof(MonoBehaviour)))
            {
                object obj = m_rootObject.GetComponentInChildren(type);
                if (obj != null)
                {
                    m_container[type] = obj;
                    return obj;
                }
            }
            return m_parentInjector?.GetInstance(type);
        }
    }
}
