using System;
namespace Lostbyte.Toolkit.Build.Editor
{
    [Serializable]
    public class SteamDepotConfig : DepotConfig
    {
        public uint AppId;
        public uint DepotId;

        public override Type UploadPlatformType => typeof(SteamConfig);

        public override void Upload(object platformConfig, string buildName, string buildPath, string zipPath, string exePath, string version)
        {
            throw new NotImplementedException();
        }
    }
}
