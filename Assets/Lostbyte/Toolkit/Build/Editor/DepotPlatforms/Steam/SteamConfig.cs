using System;

namespace Lostbyte.Toolkit.Build.Editor
{
    [Serializable]
    public class SteamConfig : UploadPlatformConfig
    {
        public string SteamUsername;
        public string SteamCmdPath;
    }
}
