using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    public static class DebugLogger // TODO all needed methods for custom logger
    {
        [System.ThreadStatic] private static StringBuilder _sb;

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitLoggerConfig()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        }
#endif

        private static StringBuilder GetBuilder()
        {
            _sb ??= new StringBuilder(256);
            _sb.Clear();
            return _sb;
        }

        // [Conditional("UNITY_EDITOR")]
        // [Conditional("DEVELOPMENT_BUILD")]
        // [Conditional("ENABLE_LOGS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object message, UnityEngine.Object context = null) => Debug.Log(message, context);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning(object message, UnityEngine.Object context = null) => Debug.LogWarning(message, context);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(object message, UnityEngine.Object context = null) => Debug.LogError(message, context);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(Exception exception, UnityEngine.Object context = null) => Debug.LogException(exception, context);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ManagerLog(object message, UnityEngine.Object context = null, [CallerFilePath] string file = "") => Debug.Log(FormatManagerMessage(message, file), context);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ManagerLogWarning(object message, UnityEngine.Object context = null, [CallerFilePath] string file = "") => Debug.LogWarning(FormatManagerMessage(message, file), context);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ManagerLogError(object message, UnityEngine.Object context = null, [CallerFilePath] string file = "") => Debug.LogError(FormatManagerMessage(message, file), context);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatManagerMessage(object message, string filePath)
        {
            var sb = GetBuilder();
            sb.Append("<b>[");
            sb.Append(Path.GetFileNameWithoutExtension(filePath));
            sb.Append("]</b> ");
            sb.Append(message);
            return sb.ToString();
        }
    }
}