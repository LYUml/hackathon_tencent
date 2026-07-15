using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TXGame.Editor
{
    public static class TxBuildUtility
    {
        [MenuItem("TX/Build Windows Fullscreen Test")]
        public static void BuildWindowsFullscreen()
        {
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsFullScreen = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;

            string outputDir = "Builds/WindowsFullscreen";
            Directory.CreateDirectory(outputDir);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                locationPathName = Path.Combine(outputDir, "Huapi_Test.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception($"Build failed: {report.summary.result}");

            Debug.Log($"Fullscreen test build created: {options.locationPathName}");
        }
    }
}
