using System;
using Lostbyte.Toolkit.Common;

namespace Lostbyte.Toolkit.Build.Editor
{
    [Serializable]
    public class ItchDepotConfig : DepotConfig
    {
        public string AppName;
        public string ChannelName;
        public override Type UploadPlatformType => typeof(ItchConfig);
        public override void Upload(object platformConfig, string buildName, string buildPath, string zipPath, string exePath, string version)
        {
            Print.MLog($"Uploading to {AppName}/{ChannelName} ({version}) build to itch.io...");
            if (platformConfig is not ItchConfig itchConfig) throw new Exception("Unknown platform config!");
            string args = $"push \"{buildPath}\" {itchConfig.ItchUsername}/{AppName}:{ChannelName} --userversion {version}";
            BuildPipelineUtils.ExecuteCommandLine(itchConfig.ButlerPath, args);
        }
    }
}
