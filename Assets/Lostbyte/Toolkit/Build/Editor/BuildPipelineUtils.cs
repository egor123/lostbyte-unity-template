using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    public class BuildTaskInfo
    {
        public string TaskName;
        public float Progress;
        public string TimeText;
        public BuildResult Result = BuildResult.Unknown;
    }
    public static class BuildPipelineUtils
    {
        public static event Action TasksUpdated;
        public static readonly List<BuildTaskInfo> BuildTasks = new();

        [MenuItem("Tools/Build Pipeline/Open Player Log")]
        public static void OpenPlayerLog()
        {
            string logPath = GetPlayerLogPath();

            if (string.IsNullOrEmpty(logPath)) return;

            if (File.Exists(logPath))
            {
                EditorUtility.OpenWithDefaultApp(logPath);
                Print.MLog($"Opening Player.log at: {logPath}");
            }
            else
            {
                Print.MWarn($"Could not find Player.log at: {logPath}\nMake sure you have run the built game at least once on this machine.");
            }
        }

        private static string GetPlayerLogPath()
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

#if UNITY_EDITOR_WIN
            // Windows: %USERPROFILE%\AppData\LocalLow\CompanyName\ProductName\Player.log
            string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string localLow = localData.Replace("Local", "LocalLow");
            return Path.Combine(localLow, companyName, productName, "Player.log");

#elif UNITY_EDITOR_OSX
            // macOS: ~/Library/Logs/CompanyName/ProductName/Player.log
            return Path.Combine(homeDir, "Library", "Logs", companyName, productName, "Player.log");

#elif UNITY_EDITOR_LINUX
            // Linux: ~/.config/unity3d/CompanyName/ProductName/Player.log
            return Path.Combine(homeDir, ".config", "unity3d", companyName, productName, "Player.log");

#else
            Debug.LogError("Unsupported OS for dynamically finding Player.log.");
            return null;
#endif
        }

        [MenuItem("Tools/Build Pipeline/Build")]
        public static void Build()
        {
            if (!GetPipelineConf(out var conf)) return;
            PreprocessVersion();
            BuildTasks.Clear();
            TasksUpdated?.Invoke();
            foreach (var opt in conf.BuildProfiles)
            {
                if (!opt.TryGetValue(out var profile) || profile == null) continue;
                foreach (var platform in profile.BuildPlatforms)
                {
                    if (platform == null) continue;
                    Build(conf, profile, platform);
                }
            }

        }
        [MenuItem("Tools/Build Pipeline/Build And Upload")]
        public static void BuildAndUpload()
        {
            Build();
            Upload();
        }
        [MenuItem("Tools/Build Pipeline/Run")]
        public static void Run()
        {
            if (!GetPipelineConf(out var conf)) return;
            GenericMenu menu = new();
            bool hasBuilds = false;

            foreach (var opt in conf.BuildProfiles)
            {
                var profile = opt.Value;
                foreach (var platform in profile.BuildPlatforms)
                {
                    if (platform == null) continue;
                    GetBuildDirectories(conf, profile, platform, out var buildName, out var buildPath, out var zipPath, out var exePath);
                    if (File.Exists(exePath) || Directory.Exists(exePath))
                    {
                        hasBuilds = true;
                        string menuLabel = string.IsNullOrWhiteSpace(profile.ProfileName)
                            ? $"{platform.Postfix}"
                            : $"{profile.ProfileName}/{platform.Postfix}";
                        menu.AddItem(new GUIContent(menuLabel), false, () =>
                        {
                            Print.MLog($"Launching '{menuLabel}' at path: {exePath}");
                            platform.Run(exePath);
                        });
                    }
                }
            }
            if (!hasBuilds)
            {
                EditorUtility.DisplayDialog("Run Build", "No existing builds were found for the current version.", "OK");
                return;
            }
            menu.ShowAsContext();
        }
        [MenuItem("Tools/Build Pipeline/Upload")]
        public static void Upload()
        {
            if (!GetPipelineConf(out var conf)) return;
            Dictionary<Type, object> uploadConfs = new();
            foreach (var platform in conf.UploadPlatforms)
            {
                uploadConfs[platform.GetType()] = platform;
            }
            foreach (var opt in conf.BuildProfiles)
            {
                var profile = opt.Value;
                foreach (var platform in profile.BuildPlatforms)
                {
                    if (platform == null) continue;
                    GetBuildDirectories(conf, profile, platform, out var buildName, out var buildPath, out var zipPath, out var exePath);
                    if (File.Exists(exePath) || Directory.Exists(exePath))
                    {
                        string version = $"V{PlayerSettings.bundleVersion}";
                        foreach (var depot in platform.Depots)
                        {
                            if(uploadConfs.TryGetValue(depot.UploadPlatformType, out var platformConf))
                            {
                                depot.Upload(platformConf, buildName, buildPath, zipPath, exePath, version);
                            }
                            else
                            {
                                Print.MError($"{depot.UploadPlatformType.Name} is not configured!");
                            }
                        }
                    }
                }
            }
        }
        [MenuItem("Tools/Build Pipeline/Open Folder")]
        public static void OpenFolder()
        {
            if (!GetPipelineConf(out var conf)) return;
            string version = $"V{PlayerSettings.bundleVersion}";
            string fullPath = Path.GetFullPath(Path.Join(conf.BuildPath, version));
            if (Directory.Exists(fullPath)) EditorUtility.RevealInFinder(fullPath);
        }

        public static bool GetPipelineConf(out BuildPipelineConfig conf)
        {
            conf = BuildPipelineConfig.instance;
            return true;
        }


        public static void PreprocessVersion()
        {
            string bundleVersion = PlayerSettings.bundleVersion;
            if (!Version.TryParse(bundleVersion, out Version version))
            {
                version = new Version(0, 0, 0, 0);
            }
            int choice = EditorUtility.DisplayDialogComplex(
                "Increment Version",
                $"Current Version: {version}\n\nHow would you like to increment?",
                "Major", // 0
                "Other",
                "Minor" // 2
            );
            if (choice == 1)
            {
                choice = 4 + EditorUtility.DisplayDialogComplex(
                    "Increment Version",
                    $"Current Version: {version}\n\nSelect increment type:",
                    "Patch", // 4
                    "Skip",
                    "Revision" // 6
                );
            }
            switch (choice)
            {
                case 0: // Major
                    version = new(version.Major + 1, 0, 0, 0);
                    PlayerSettings.bundleVersion = version.ToString();
                    return;
                case 2: // Minor
                    version = new(version.Major, version.Minor + 1, 0, 0);
                    PlayerSettings.bundleVersion = version.ToString();
                    return;
                case 4: // Patch
                    version = new(version.Major, version.Minor, version.Build + 1, 0);
                    PlayerSettings.bundleVersion = version.ToString();
                    return;
                case 6: // Revision
                    version = new(version.Major, version.Minor, version.Build, version.Revision + 1);
                    PlayerSettings.bundleVersion = version.ToString();
                    return;
            }
        }

        public static void ExecuteCommandLine(string fileName, string args)
        {
            ProcessStartInfo processInfo = new()
            {
                FileName = fileName,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            try
            {
                using Process process = Process.Start(processInfo);
                process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) Print.MLog($"[{fileName}] {e.Data}"); };
                process.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) Print.MError($"[{fileName}] {e.Data}"); };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Print.MError($"Failed to start {fileName}. Ensure the path is correct or added to your system environment variables. Error: {ex.Message}");
            }
        }

        public static void Build(BuildPipelineConfig conf, BuildProfile profile, BuildPlatform platform)
        {
            var taskInfo = new BuildTaskInfo
            {
                TaskName = $"Building {profile.ProfileName}_{platform.Postfix}",
                Progress = 0f,
                TimeText = "In Progress...",
                Result = BuildResult.Unknown,
            };
            BuildTasks.Add(taskInfo);
            TasksUpdated?.Invoke();

            GetBuildDirectories(conf, profile, platform, out var buildName, out var buildPath, out var zipPath, out var exePath);

            platform.Configure();

            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();

            BuildPlayerOptions options = new()
            {
                scenes = profile.Scenes,
                locationPathName = exePath,
                targetGroup = platform.GetTargetGroup(),
                target = platform.GetTarget(),
                options = profile.Options,
                subtarget = (int)profile.Subtarget,
                extraScriptingDefines = profile.ScriptingDefines.ToArray()
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            var time = report.summary.totalTime.ToString(@"m\:ss");
            taskInfo.Progress = 1f; // TODO REALTIME PROGRESS????
            taskInfo.TimeText = time;
            taskInfo.Result = report.summary.result;
            TasksUpdated?.Invoke();

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Build Failed for '{buildName}'. Result: {report.summary.result} ({report.summary.totalErrors} errors)");
            }

            Print.MLog($"Build succeeded at '{exePath}', {report.summary.totalSize / 1024 / 1024} MB in {time}");

            if (!options.options.HasFlag(BuildOptions.Development)) RemoveDoNotShipFiles(buildPath);
            ZipBuild(buildPath, zipPath);
        }

        public static void RemoveDoNotShipFiles(string buildPath)
        {
            string buildDirectory = Path.GetDirectoryName(buildPath);
            string backupDirectory = Path.Combine(buildDirectory, "_CrashBackups");

            if (!Directory.Exists(backupDirectory)) Directory.CreateDirectory(backupDirectory);
            string[] directories = Directory.GetDirectories(buildDirectory);
            foreach (string dir in directories)
            {
                if (dir.EndsWith("_DoNotShip") || dir.EndsWith("_ButDontShipItWithYourGame"))
                {
                    string folderName = Path.GetFileName(dir);
                    string destination = Path.Combine(backupDirectory, folderName);
                    if (Directory.Exists(destination)) Directory.Delete(destination, true);
                    Directory.Move(dir, destination);
                    Print.MLog($"Moved {folderName} to Crash Backups");
                }
            }
            string[] files = Directory.GetFiles(buildDirectory, "*.pdb", SearchOption.AllDirectories);
            foreach (string file in files) File.Delete(file);
        }

        private static void GetBuildDirectories(BuildPipelineConfig conf, BuildProfile profile, BuildPlatform platform, out string buildName, out string buildPath, out string zipPath, out string exePath)
        {
            string version = $"V{PlayerSettings.bundleVersion}";
            buildName = PlayerSettings.productName;
            if (!string.IsNullOrWhiteSpace(profile.ProfileName))
                buildName += $"_{profile.ProfileName}";
            buildName += $"_{platform.Postfix}";
            buildName += $"_{version}";

            buildPath = CreateDirectory(conf.BuildPath, version, buildName);
            zipPath = Path.Combine(conf.BuildPath, version, $"{buildName}.zip");
            exePath = platform.GetExecutablePath(buildPath, PlayerSettings.productName);
        }

        private static string CreateDirectory(params string[] paths)
        {
            string path = "";
            foreach (var part in paths)
            {
                path = Path.Combine(path, part);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            return path;
        }

        private static void ZipBuild(string sourceDirectory, string zipPath)
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            var destinationDir = Path.GetDirectoryName(zipPath);
            if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            ZipFile.CreateFromDirectory(sourceDirectory, zipPath);
            Print.MLog($"Compressed build package to: {zipPath}");
        }
    }
}