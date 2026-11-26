#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LiminalSpace.Editor
{
    /// <summary>
    /// Builds the 360 panorama scene as a Windows executable, for showing the viewer on a
    /// laptop without a headset or a phone.
    /// Menu: "Liminal Space" -> "Build 360 Panorama for Windows".
    /// Command line:
    ///   Unity.exe -batchmode -nographics -projectPath &lt;project&gt;
    ///             -executeMethod LiminalSpace.Editor.PanoramaWindowsBuild.BuildWindows
    ///             -logFile &lt;log&gt;
    /// </summary>
    public static class PanoramaWindowsBuild
    {
        private const string ScenePath = "Assets/Scenes/PanoramaViewer.unity";
        private const string OutputDirectory = "Builds/Windows";
        private const string ExeName = "PanoramaViewer.exe";

        [MenuItem("Liminal Space/Build 360 Panorama for Windows", false, 22)]
        public static void BuildWindows()
        {
            bool success = false;
            try
            {
                success = RunBuild();
            }
            finally
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(success ? 0 : 1);
                }
            }
        }

        private static bool RunBuild()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogError("[PanoramaWindowsBuild] " + ScenePath + " is missing.");
                return false;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                Debug.Log("[PanoramaWindowsBuild] Switching build target to Windows...");
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                        BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                {
                    Debug.LogError("[PanoramaWindowsBuild] Could not switch to Windows. " +
                                   "Is the Windows build support module installed?");
                    return false;
                }
            }

            string directory = Path.GetFullPath(OutputDirectory);
            Directory.CreateDirectory(directory);
            string exePath = Path.Combine(directory, ExeName).Replace('\\', '/');

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[PanoramaWindowsBuild] SUCCESS " + exePath +
                          " size=" + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB" +
                          " time=" + summary.totalTime.TotalSeconds.ToString("0") + "s");
                return true;
            }

            Debug.LogError("[PanoramaWindowsBuild] FAILED result=" + summary.result +
                           " errors=" + summary.totalErrors);
            return false;
        }
    }
}
#endif
