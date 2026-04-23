using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.FactSystem.Persistance;
using UnityEngine;

public class PersitantTransform : MonoBehaviour, IPersistent
{
    [Serializable]
    public struct Data
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Data(Transform transform) => (Position, Rotation, Scale) = (transform.localPosition, transform.localRotation, transform.localScale);
        public readonly void Apply(Transform transform)
        {
            transform.SetLocalPositionAndRotation(Position, Rotation);
            transform.localScale = Scale;
        }
    }

    private readonly SubscriptionGroup _subscriptions = new();
    private string Key => $"gameobjects/{name}";
    private void Start()
    {
        _subscriptions.Subscribe(GameFacts.Keys.Game, this);
    }

    private void OnDestroy()
    {
        _subscriptions.Dispose();
    }
    public void OnLoad(Store store)
    {
        Debug.Log("OnLoad!");
        store.GetData(Key, new Data(transform)).Apply(transform);
    }

    public void OnSave(Store store)
    {
        Debug.Log("OnSave!");
        store.SetData(Key, new Data(transform));
    }

}
