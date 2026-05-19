using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    public static class Print // TODO all needed methods for custom logger
    {
        [System.ThreadStatic] private static StringBuilder _sb;
        private static readonly ConcurrentDictionary<string, string> _fileNameCache = new();
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
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object message, UnityEngine.Object context = null) => Debug.Log(message, context);
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warn(object message, UnityEngine.Object context = null) => Debug.LogWarning(message, context);
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(object message, UnityEngine.Object context = null) => Debug.LogError(message, context);
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exception(Exception exception, UnityEngine.Object context = null) => Debug.LogException(exception, context);
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Assert(bool condition, string message, UnityEngine.Object context = null)
        {
            if (!condition) Debug.LogError($"<b>[ASSERT FAILED]</b> {message}", context);
        }
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MLog(object message, UnityEngine.Object context = null, [CallerFilePath] string file = "") => Debug.Log(FormatManagerMessage(message, file), context);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MWarn(object message, UnityEngine.Object context = null, [CallerFilePath] string file = "") => Debug.LogWarning(FormatManagerMessage(message, file), context);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MError(object message, UnityEngine.Object context = null, [CallerFilePath] string file = "") => Debug.LogError(FormatManagerMessage(message, file), context);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MAssert(bool condition, object message, UnityEngine.Object context = null, [CallerFilePath] string file = "")
        {
            if (!condition) Debug.LogError(FormatManagerMessage($"<b>[ASSERT FAILED]</b> {message}", file), context);
        }
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatManagerMessage(object message, string filePath)
        {
#if UNITY_EDITOR
            if (!_fileNameCache.TryGetValue(filePath, out string headerText))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string spacedName = Regex.Replace(fileName, @"(\B[A-Z])", " $1");
                headerText = $"<size=14><b>[{spacedName}]</b></size> ";
                _fileNameCache[filePath] = headerText;
            }
            var sb = GetBuilder();
            sb.Append(headerText);
            sb.Append(message);
            return sb.ToString();
#else
            return message?.ToString() ?? "null";
#endif
        }
    }
}