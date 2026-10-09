using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    [FilePath("Assets/BuildPipelineConfig.asset", FilePathAttribute.Location.ProjectFolder)]
    public class BuildPipelineConfig : ScriptableSingleton<BuildPipelineConfig>
    {
        public string BuildPath = "Builds";
        [SerializeReference, UniqueReference] public List<UploadPlatformConfig> UploadPlatforms = new();
        public List<Optional<BuildProfile>> BuildProfiles  = new();

        public void Save() => Save(true);
    }
}
