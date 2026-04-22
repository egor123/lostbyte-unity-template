using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    public static class TaskExtensions
    {
        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        public static async void Then(this Task task, Action callback)
        {
            try
            {
                await task;
                callback?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        public static async void Then<T>(this Task<T> task, Action<T> callback)
        {
            try
            {
                callback?.Invoke(await task);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}