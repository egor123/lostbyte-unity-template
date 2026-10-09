using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    public enum WindowsArchitecture
    {
        x86, // 32-bit (Deprecated)
        x64  // 64-bit (Standard)
    }
    [Serializable]
    public class WindowsBuildPlatform : BuildPlatform
    {
        public WindowsArchitecture Architecture = WindowsArchitecture.x64;
        public ScriptingImplementation ScriptingBackend = ScriptingImplementation.Mono2x;

        public override string Postfix => $"Win_{Architecture}";

        public override void Configure()
        {
            PlayerSettings.SetScriptingBackend(GetTargetGroup(), ScriptingBackend);
            EditorUserBuildSettings.SwitchActiveBuildTarget(GetTargetGroup(), GetTarget());
            if (ScriptingBackend == ScriptingImplementation.IL2CPP)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(GetTargetGroup(), Il2CppCompilerConfiguration.Release);
            }
        }
        public override BuildTargetGroup GetTargetGroup() => BuildTargetGroup.Standalone;
        public override BuildTarget GetTarget() => (Architecture == WindowsArchitecture.x86) ? BuildTarget.StandaloneWindows : BuildTarget.StandaloneWindows64;
        public override string GetExecutablePath(string outputFolder, string executableName) =>
                    Path.Combine(outputFolder, $"{executableName}.exe");

        public override void Run(string path)
        {
            string absolutePath = Path.GetFullPath(path);
            Process.Start(absolutePath);
        }
    }
}
