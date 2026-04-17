using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    [Serializable]
    public class SerializedTuple<T1, T2>
    {
        [field: SerializeField] public T1 Item1 { get; private set; }
        [field: SerializeField] public T2 Item2 { get; private set; }
        public SerializedTuple(T1 item1, T2 item2) => (Item1, Item2) = (item1, item2);
        public override string ToString() => $"({Item1}, {Item2})";
        public override bool Equals(object obj)
        {
            if (obj is KeyValuePair<T1, T2> pair) return pair.Key.Equals(Item1) && pair.Value.Equals(Item2);
            if (obj is Tuple<T1, T2> tuple) return tuple.Item1.Equals(Item1) && tuple.Item2.Equals(Item2);
            if (obj is SerializedTuple<T1, T2> sTuple) return sTuple.Item1.Equals(Item1) && sTuple.Item2.Equals(Item2);
            return false;
        }
        public override int GetHashCode() => HashCode.Combine(Item1, Item2);
        public static implicit operator KeyValuePair<T1, T2>(SerializedTuple<T1, T2> tuple) => new(tuple.Item1, tuple.Item2);
        public static implicit operator Tuple<T1, T2>(SerializedTuple<T1, T2> tuple) => new(tuple.Item1, tuple.Item2);
        public void Deconstruct(out T1 item1, out T2 item2)
        {
            item1 = Item1;
            item2 = Item2;
        }
    }
    [Serializable]
    public class SerializedTuple<T1, T2, T3>
    {
        [field: SerializeField] public T1 Item1 { get; private set; }
        [field: SerializeField] public T2 Item2 { get; private set; }
        [field: SerializeField] public T3 Item3 { get; private set; }

        public SerializedTuple(T1 item1, T2 item2, T3 item3) => (Item1, Item2, Item3) = (item1, item2, item3);
        public override string ToString() => $"({Item1}, {Item2}, {Item3})";
        public override bool Equals(object obj)
        {
            if (obj is Tuple<T1, T2, T3> tuple) return tuple.Item1.Equals(Item1) && tuple.Item2.Equals(Item2) && tuple.Item3.Equals(Item3);
            if (obj is SerializedTuple<T1, T2, T3> sTuple) return sTuple.Item1.Equals(Item1) && sTuple.Item2.Equals(Item2) && sTuple.Item3.Equals(Item3);
            return false;
        }
        public override int GetHashCode() => HashCode.Combine(Item1, Item2, Item3);
        public static implicit operator Tuple<T1, T2, T3>(SerializedTuple<T1, T2, T3> tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3);
        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
        }
    }
    [Serializable]
    public class SerializedTuple<T1, T2, T3, T4>
    {
        [field: SerializeField] public T1 Item1 { get; private set; }
        [field: SerializeField] public T2 Item2 { get; private set; }
        [field: SerializeField] public T3 Item3 { get; private set; }
        [field: SerializeField] public T4 Item4 { get; private set; }

        public SerializedTuple(T1 item1, T2 item2, T3 item3, T4 item4) => (Item1, Item2, Item3, Item4) = (item1, item2, item3, item4);
        public override string ToString() => $"({Item1}, {Item2}, {Item3}, {Item4})";
        public override bool Equals(object obj)
        {
            if (obj is Tuple<T1, T2, T3, T4> tuple) return tuple.Item1.Equals(Item1) && tuple.Item2.Equals(Item2) && tuple.Item3.Equals(Item3) && tuple.Item4.Equals(Item4);
            if (obj is SerializedTuple<T1, T2, T3, T4> sTuple) return sTuple.Item1.Equals(Item1) && sTuple.Item2.Equals(Item2) && sTuple.Item3.Equals(Item3) && sTuple.Item4.Equals(Item4);
            return false;
        }
        public override int GetHashCode() => HashCode.Combine(Item1, Item2, Item3, Item4);
        public static implicit operator Tuple<T1, T2, T3, T4>(SerializedTuple<T1, T2, T3, T4> tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
        }
    }
    [Serializable]
    public class SerializedTuple<T1, T2, T3, T4, T5>
    {
        [field: SerializeField] public T1 Item1 { get; private set; }
        [field: SerializeField] public T2 Item2 { get; private set; }
        [field: SerializeField] public T3 Item3 { get; private set; }
        [field: SerializeField] public T4 Item4 { get; private set; }
        [field: SerializeField] public T5 Item5 { get; private set; }

        public SerializedTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) => (Item1, Item2, Item3, Item4, Item5) = (item1, item2, item3, item4, item5);
        public override string ToString() => $"({Item1}, {Item2}, {Item3}, {Item5})";
        public override bool Equals(object obj)
        {
            if (obj is Tuple<T1, T2, T3, T4, T5> tuple) return tuple.Item1.Equals(Item1) && tuple.Item2.Equals(Item2) && tuple.Item3.Equals(Item3) && tuple.Item4.Equals(Item4) && tuple.Item5.Equals(Item5);
            if (obj is SerializedTuple<T1, T2, T3, T4, T5> sTuple) return sTuple.Item1.Equals(Item1) && sTuple.Item2.Equals(Item2) && sTuple.Item3.Equals(Item3) && sTuple.Item4.Equals(Item4) && sTuple.Item5.Equals(Item5);
            return false;
        }
        public override int GetHashCode() => HashCode.Combine(Item1, Item2, Item3, Item4, Item5);
        public static implicit operator Tuple<T1, T2, T3, T4, T5>(SerializedTuple<T1, T2, T3, T4, T5> tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
        }
    }
}
