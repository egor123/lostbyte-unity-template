using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Build.Editor
{
    [Serializable]
    public class WebBuildPlatform : BuildPlatform
    {
        public WebGLCompressionFormat CompressionFormat = WebGLCompressionFormat.Brotli;
        [Tooltip("WebGL relies heavily on IL2CPP Optimization. Use Release for final builds.")]
        public Il2CppCompilerConfiguration CompilerOptimization = Il2CppCompilerConfiguration.Release;

        public override string Postfix => "WebGL";

        public override void Configure()
        {
            PlayerSettings.SetScriptingBackend(GetTargetGroup(), ScriptingImplementation.IL2CPP);
            EditorUserBuildSettings.SwitchActiveBuildTarget(GetTargetGroup(), GetTarget());
            PlayerSettings.SetIl2CppCompilerConfiguration(GetTargetGroup(), CompilerOptimization);
            PlayerSettings.WebGL.compressionFormat = CompressionFormat;
        }
        public override BuildTarget GetTarget() => BuildTarget.WebGL;
        public override BuildTargetGroup GetTargetGroup() => BuildTargetGroup.WebGL;
        public override string GetExecutablePath(string outputFolder, string executableName) =>
            Path.Combine(outputFolder, "index.html");

        public override void Run(string path)
        {
            string absolutePath = Path.GetFullPath(path);
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
            Print.Log("Starting local Python server for WebGL. Close the terminal window when done.");
            ProcessStartInfo info = new()
            {
                FileName = "python",
                Arguments = "unity_server.py",
                WorkingDirectory = folderPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(info);
            System.Threading.Thread.Sleep(1000);
            Application.OpenURL("http://localhost:8080");
        }
    }
}