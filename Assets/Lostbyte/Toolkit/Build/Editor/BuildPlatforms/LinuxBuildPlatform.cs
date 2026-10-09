using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    [Serializable]
    public class LinuxBuildPlatform : BuildPlatform
    {
        public ScriptingImplementation ScriptingBackend = ScriptingImplementation.Mono2x;

        public override string Postfix => "Linux";

        public override void Configure()
        {
            PlayerSettings.SetScriptingBackend(GetTargetGroup(), ScriptingBackend);
            EditorUserBuildSettings.SwitchActiveBuildTarget(GetTargetGroup(), GetTarget());
            if (ScriptingBackend == ScriptingImplementation.IL2CPP)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(GetTargetGroup(), Il2CppCompilerConfiguration.Release);
            }
        }
        public override BuildTarget GetTarget() => BuildTarget.StandaloneLinux64;
        public override BuildTargetGroup GetTargetGroup() => BuildTargetGroup.Standalone;
        public override string GetExecutablePath(string outputFolder, string executableName) =>
             Path.Combine(outputFolder, $"{executableName}.x86_64");


        public override void Run(string path)
        {
            string absolutePath = Path.GetFullPath(path);
            Process.Start(absolutePath);
        }
    }
}