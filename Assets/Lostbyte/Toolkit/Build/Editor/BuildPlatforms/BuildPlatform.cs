using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    [Serializable]
    public abstract class BuildPlatform
    {
        [SerializeReference, UniqueReference] public List<DepotConfig> Depots = new();
        public abstract string Postfix { get; }
        public abstract void Configure();
        public abstract BuildTargetGroup GetTargetGroup();
        public abstract BuildTarget GetTarget();
        public abstract string GetExecutablePath(string outputFolder, string executableName);
        public abstract void Run(string path);
    }
}
