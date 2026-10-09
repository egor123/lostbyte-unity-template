using System;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    [Serializable]
    public abstract class DepotConfig
    {
        public abstract Type UploadPlatformType { get; }
        public abstract void Upload(object platformConfig, string buildName, string buildPath, string zipPath, string exePath, string version);
    }
}
