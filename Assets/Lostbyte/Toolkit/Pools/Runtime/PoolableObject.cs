using System;
using UnityEngine;

namespace Lostbyte.Toolkit.Pools
{
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
}
