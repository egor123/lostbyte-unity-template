using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Pools
{
    public abstract class Pool<T> : ScriptableObject where T : PollableObject
    {
        [SerializeField]
        private T m_prefab;
        [SerializeField]
        private int m_maxPoolSize = 10;
        private readonly Queue<T> _pool = new();

        //TODO some kinda dynamic size magic  

        public T GetFromPool()
        {

            if (_pool.TryDequeue(out var item))
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item = Instantiate(m_prefab);
                DontDestroyOnLoad(item.gameObject);
                item.OnCreate();
                item.ReleaseToPoolAction = () => ReleaseToPool(item);
            }
            item.gameObject.hideFlags = HideFlags.None;
            return item;
        }

        private void ReleaseToPool(T item)
        {
            item.transform.parent = null;
            item.gameObject.SetActive(false);
            item.gameObject.hideFlags = HideFlags.HideInHierarchy;

            if (_pool.Count < m_maxPoolSize) _pool.Enqueue(item);
            else Destroy(item.gameObject);
        }
    }
}

public abstract class PollableObject : MonoBehaviour
{
    internal Action ReleaseToPoolAction { private get; set; }
    public virtual void OnCreate() { }
    public virtual void ReleaseToPool()
    {
        ReleaseToPoolAction.Invoke();
        foreach (var obj in GetComponentsInChildren<PollableObject>())
            if (obj != this)
                obj.ReleaseToPool();
    }

}