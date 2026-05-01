using UnityEditor;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using Lostbyte.Toolkit.Common;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
    public class AdvancedBuildWindow : EditorWindow
    {
        // Build Toggle Settings
        private bool buildWindows = true;
        private bool buildMac = false;
        private bool buildLinux = false;
        private bool buildWebGL = false;

        // Post-Build Settings
        private bool autoZip = true;
        private bool openFolder = true;


        // Upload Settings
        private bool uploadItch = false;
        private bool uploadSteam = false;

        // Configuration Strings
        private string itchUsername = "your_username";
        private string itchGameName = "your_game";
        private string butlerPath = "butler";
        private string steamCmdPath = "steamcmd";
        private string steamVdfPath = "C:/Path/To/app_build.vdf";

        [MenuItem("Window/Advanced Build Pipeline")]
        public static void ShowWindow()
        {
            GetWindow<AdvancedBuildWindow>("Build Pipeline");
        }

        private void OnGUI()
        {
            GUILayout.Label("Platform Selection", EditorStyles.boldLabel);
            buildWindows = EditorUtils.EditorPrefsToggle("Build Windows (x64)", "ABP_Windows", buildWindows);
            buildMac = EditorUtils.EditorPrefsToggle("Build MacOS (Universal)", "ABP_Mac", buildMac);
            buildLinux = EditorUtils.EditorPrefsToggle("Build Linux (x64)", "ABP_Linux", buildLinux);
            buildWebGL = EditorUtils.EditorPrefsToggle("Build WebGL", "ABP_WebGL", buildWebGL);

            EditorGUILayout.Space();

            GUILayout.Label("Post-Build Actions", EditorStyles.boldLabel);
            openFolder = EditorUtils.EditorPrefsToggle("Open Folder on Complete", "ABP_OpenFolder", openFolder);
            autoZip = EditorUtils.EditorPrefsToggle("Auto Compress (Zip)", "ABP_AutoZip", autoZip);

            EditorGUILayout.Space();

            if (autoZip)
            {
                GUILayout.Label("Store Uploads", EditorStyles.boldLabel);
                uploadItch = EditorUtils.EditorPrefsToggle("Upload to Itch.io", "ABP_UploadItch", uploadItch);
                if (uploadItch)
                {
                    EditorGUI.indentLevel++;
                    itchUsername = EditorUtils.EditorPrefsTextField("Itch Username", "ABP_ItchUser", itchUsername);
                    itchGameName = EditorUtils.EditorPrefsTextField("Itch Game Name", "ABP_ItchGame", itchGameName);
                    butlerPath = EditorUtils.EditorPrefsTextField("Butler Path/Cmd", "ABP_ButlerPath", butlerPath);
                    EditorGUI.indentLevel--;
                }
                uploadSteam = EditorUtils.EditorPrefsToggle("Upload to Steam", "ABP_UploadSteam", uploadSteam);
                if (uploadSteam)
                {
                    EditorGUI.indentLevel++;
                    steamCmdPath = EditorUtils.EditorPrefsTextField("SteamCMD Path/Cmd", "ABP_SteamCmdPath", steamCmdPath);
                    steamVdfPath = EditorUtils.EditorPrefsTextField("VDF Script Path", "ABP_SteamVdfPath", steamVdfPath);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Execute Build Pipeline", GUILayout.Height(40)))
            {
                RunPipeline();
            }

            EditorGUILayout.Space();
            DrawLocalBuildLaunchers();
        }

        private void DrawLocalBuildLaunchers()
        {
            string productName = PlayerSettings.productName;

            string winPath = Path.Combine("Builds", "Windows", productName + ".exe");
            string macPath = Path.Combine("Builds", "Mac", productName + ".app");
            string linuxPath = Path.Combine("Builds", "Linux", productName + ".x86_64");
            string webglPath = Path.Combine("Builds", "WebGL", "index.html");

            // Check if ANY builds exist to determine if we should draw the header
            bool hasWin = File.Exists(winPath);
            bool hasMac = Directory.Exists(macPath); // Mac .app is a folder, not a file!
            bool hasLinux = File.Exists(linuxPath);
            bool hasWebGl = File.Exists(webglPath);

            if (!hasWin && !hasMac && !hasLinux && !hasWebGl)
                return; // Hide section entirely if no builds are found

            GUILayout.Label("Launch Local Builds", EditorStyles.boldLabel);

            if (hasWin && GUILayout.Button("Run Windows Build"))
                LaunchApp(winPath);

            if (hasMac && GUILayout.Button("Run Mac Build"))
                LaunchApp(macPath);

            if (hasLinux && GUILayout.Button("Run Linux Build"))
                LaunchApp(linuxPath);

            if (hasWebGl && GUILayout.Button("Open WebGL Build"))
                LaunchApp(webglPath);

        }

        private void LaunchApp(string relativePath)
        {
            try
            {
                string absolutePath = Path.GetFullPath(relativePath);
                if (absolutePath.EndsWith(".html") || absolutePath.EndsWith(".htm"))
                {
                    string folderPath = Path.GetDirectoryName(absolutePath);
                    string serverScriptPath = Path.Combine(folderPath, "unity_server.py");
                    string pythonScript = @"import http.server
import socketserver

PORT = 8080

class UnityHandler(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path):
        if path.endswith('.wasm') or path.endswith('.wasm.gz') or path.endswith('.wasm.br'):
            return 'application/wasm'
        if path.endswith('.js.gz') or path.endswith('.js.br'):
            return 'application/javascript'
        if path.endswith('.data.gz') or path.endswith('.data.br'):
            return 'application/octet-stream'
        return super().guess_type(path)

    def end_headers(self):
        if self.path.endswith('.gz'):
            self.send_header('Content-Encoding', 'gzip')
        elif self.path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')
        super().end_headers()

with socketserver.TCPServer(('', PORT), UnityHandler) as httpd:
    print(f'Serving Unity WebGL with Compression Support on http://localhost:{PORT}')
    print('Close this terminal window to stop the server.')
    httpd.serve_forever()";

                    File.WriteAllText(serverScriptPath, pythonScript);
                    DebugLogger.Log("Starting local Python server for WebGL. Close the terminal window when done.");
                    ProcessStartInfo psi = new()
                    {
                        FileName = "python",
                        Arguments = "unity_server.py",
                        WorkingDirectory = folderPath,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Normal
                    };
                    Process.Start(psi);
                    System.Threading.Thread.Sleep(1000);
                    Application.OpenURL("http://localhost:8080");
                    return;
                }
                Process.Start(absolutePath);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"<b>[Pipeline]</b> Failed to launch application at {relativePath}. Error: {ex.Message}");
            }
        }
        private bool ConfirmPipelineDialog()
        {
            string summary = "Are you sure you want to start the build pipeline?\n\n";
            List<string> platforms = new();
            if (buildWindows) platforms.Add("Windows");
            if (buildMac) platforms.Add("Mac");
            if (buildLinux) platforms.Add("Linux");
            if (buildWebGL) platforms.Add("WebGL");
            if (platforms.Count == 0)
            {
                EditorUtility.DisplayDialog("Pipeline Error", "You must select at least one platform to build.", "OK");
                return false;
            }
            summary += "Platforms:\n- " + string.Join("\n- ", platforms) + "\n\n";
            summary += $"Auto-Zip: {(autoZip ? "Enabled" : "Disabled")}\n";
            if (autoZip && (uploadItch || uploadSteam))
            {
                summary += "\nSTORE UPLOADS ENABLED:\n";
                if (uploadItch) summary += $"- Itch.io ({itchUsername}/{itchGameName})\n";
                if (uploadSteam) summary += "- Steam (via VDF script)\n";
                summary += "\nWarning: Builds will be pushed live automatically if successful!";
            }
            return EditorUtility.DisplayDialog("Confirm Build Pipeline", summary, "Start Pipeline", "Cancel");
        }
        private void RunPipeline()
        {
            if (!ConfirmPipelineDialog())
            {
                DebugLogger.ManagerLog("Pipeline canceled by user.");
                return;
            }
            DebugLogger.ManagerLog("Starting Build Pipeline...");

            Dictionary<string, string> builds = new();
            int attempted = 0;

            if (buildWindows)
            {
                attempted++;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                if (!BuildTargetPlatform(BuildTarget.StandaloneWindows64, "Windows", ".exe", out var winPath)) { AbortPipeline("Windows"); return; }
                builds["windows"] = winPath;
            }
            if (buildMac)
            {
                attempted++;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                if (!BuildTargetPlatform(BuildTarget.StandaloneOSX, "Mac", ".app", out var macPath)) { AbortPipeline("Mac"); return; }
                builds["mac"] = macPath;
            }
            if (buildLinux)
            {
                attempted++;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
                if (!BuildTargetPlatform(BuildTarget.StandaloneLinux64, "Linux", ".x86_64", out var linuxPath)) { AbortPipeline("Linux"); return; }
                builds["linux"] = linuxPath;
            }
            if (buildWebGL)
            {
                attempted++;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
                if (!BuildTargetPlatform(BuildTarget.WebGL, "WebGL", "", out var webGLPath)) { AbortPipeline("WebGL"); return; }
                builds["webgl"] = webGLPath;
            }

            DebugLogger.ManagerLog($"Finished Successfully! {builds.Count}/{attempted} builds generated.");

            if (builds.Count > 0 && autoZip)
            {
                if (uploadItch) HandleItchUploads(builds);
                if (uploadSteam) UploadToSteam();
            }

            if (openFolder && builds.Count > 0)
            {
                string fullPath = Path.GetFullPath("Builds/");
                EditorUtility.RevealInFinder(fullPath);
            }
        }

        private void AbortPipeline(string failedPlatform)
        {
            DebugLogger.ManagerLogError($"ABORTED! {failedPlatform} build failed. Halting remaining jobs.");
        }

        private bool BuildTargetPlatform(BuildTarget target, string folderName, string ext, out string buildPath)
        {
            DebugLogger.ManagerLog($"Building {folderName}...");

            buildPath = Path.Combine("Builds", folderName);
            string outputPath;

            if (target == BuildTarget.WebGL) outputPath = buildPath;
            else outputPath = Path.Combine(buildPath, PlayerSettings.productName + ext);

            if (Directory.Exists(buildPath)) Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            BuildPlayerOptions options = new()
            {
                scenes = GetScenes(),
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                DebugLogger.ManagerLog($"{folderName} Build Succeeded.");
                if (autoZip) buildPath = ZipBuild(buildPath, folderName);
                return true;
            }
            return false;
        }

        private string ZipBuild(string source, string platform)
        {
            string zipPath = $"Builds/{PlayerSettings.productName}_{platform}_v{PlayerSettings.bundleVersion}.zip";
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(source, zipPath);
            DebugLogger.ManagerLog($"Compressed to: {zipPath}");
            return zipPath;
        }

        private void HandleItchUploads(Dictionary<string, string> builds)
        {
            string version = PlayerSettings.bundleVersion;
            foreach ((string build, string path) in builds)
                UploadToItch(path, build, version);
        }

        private void UploadToItch(string sourcePath, string channel, string version)
        {
            DebugLogger.ManagerLog($"Uploading {channel} build to itch.io...");
            string absolutePath = Path.GetFullPath(sourcePath);
            string args = $"push \"{absolutePath}\" {itchUsername}/{itchGameName}:{channel} --userversion {version}";
            ExecuteCommandLine(butlerPath, args);
        }

        private void UploadToSteam() //TODO GENERATE VDF
        {
            if (string.IsNullOrEmpty(steamVdfPath) || !File.Exists(steamVdfPath))
            {
                DebugLogger.ManagerLog("Steam upload failed: Invalid or missing VDF script path.");
                return;
            }
            DebugLogger.ManagerLog("Uploading to Steam via VDF script...");
            string absoluteVdfPath = Path.GetFullPath(steamVdfPath);
            string args = $"+run_app_build \"{absoluteVdfPath}\" +quit";
            ExecuteCommandLine(steamCmdPath, args);
        }

        private void ExecuteCommandLine(string fileName, string args)
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
                process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) DebugLogger.ManagerLog($"[{fileName}] {e.Data}"); };
                process.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) DebugLogger.ManagerLogError($"[{fileName}] {e.Data}"); };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (System.Exception ex)
            {
                DebugLogger.ManagerLogError($"Failed to start {fileName}. Ensure the path is correct or added to your system environment variables. Error: {ex.Message}");
            }
        }

        private string[] GetScenes()
        {
            List<string> scenes = new();
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled) scenes.Add(s.path);
            return scenes.ToArray();
        }
    }
}