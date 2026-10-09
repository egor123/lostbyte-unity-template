using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    public enum SceneInclusionMode
    {
        BuildScenes,
        OverrideScenes
    }

    [CreateAssetMenu(fileName = "NewBuildProfile", menuName = "Build Pipeline/Build Profile")]
    public class BuildProfile : ScriptableObject
    {
        public string ProfileName = "";
        public BuildOptions Options = BuildOptions.None;
        public StandaloneBuildSubtarget Subtarget = StandaloneBuildSubtarget.Player;
        [SerializeReference, UniqueReference] public List<BuildPlatform> BuildPlatforms = new();
        public List<string> ScriptingDefines = new();
        public SceneInclusionMode SceneMode = SceneInclusionMode.BuildScenes;
        public List<SceneAsset> IncludedScenes = new();

        public string[] Scenes
        {
            get
            {
                if (SceneMode == SceneInclusionMode.OverrideScenes)
                    return IncludedScenes.WhereNotNull()
                        .Select(AssetDatabase.GetAssetPath)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
                return EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();
            }
        }
    }
}
