using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace Lostbyte.Toolkit.Common
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }
        public static IEnumerable<int> ToStream(this int end)
        {
            for (int i = 0; i < end; i++)
                yield return i;
        }
        public static IEnumerable<int> ToStream(this int start, int end)
        {
            for (int i = start; i < end; i++)
                yield return i;
        }
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            if (source == null || !source.Any()) return default;
            return source.ElementAt(UnityEngine.Random.Range(0, source.Count()));
        }
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => UnityEngine.Random.value);
        }
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(x => x != null);
        }
    }
}