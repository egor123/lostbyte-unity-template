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
}
}