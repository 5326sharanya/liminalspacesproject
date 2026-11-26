#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiminalSpace.Editor
{
    /// <summary>
    /// Builds the 360 panorama scene to an installable Android APK.
    /// Menu: "Liminal Space" -> "Build 360 Panorama APK".
    /// Command line:
    ///   Unity.exe -batchmode -nographics -projectPath &lt;project&gt; -buildTarget Android
    ///             -executeMethod LiminalSpace.Editor.PanoramaAndroidBuild.BuildApk
    ///             -logFile &lt;log&gt;
    /// </summary>
    public static class PanoramaAndroidBuild
    {
        private const string ScenePath = "Assets/Scenes/PanoramaViewer.unity";
        private const string OutputDirectory = "Builds/Android";
        private const string ApkFileName = "PanoramaViewer.apk";
        private const string MobilePipelinePath = "Assets/Settings/Mobile_RPAsset.asset";
        private const string AndroidBundleId = "com.liminalspace.panoramaviewer";

        [MenuItem("Liminal Space/Build 360 Panorama APK", false, 21)]
        public static void BuildApk()
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
                Debug.LogError("[PanoramaAndroidBuild] " + ScenePath + " is missing.");
                return false;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("[PanoramaAndroidBuild] Switching build target to Android...");
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                {
                    Debug.LogError("[PanoramaAndroidBuild] Could not switch to Android. " +
                                   "Is the Android build support module installed?");
                    return false;
                }
            }

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidBundleId);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            // The PC pipeline is the project default; a phone should render with the cheaper
            // one. Restore the editor's own quality level once the APK is built.
            int editorQualityLevel = QualitySettings.GetQualityLevel();
            UseMobilePipelineIfAvailable();

            string directory = Path.GetFullPath(OutputDirectory);
            Directory.CreateDirectory(directory);
            string apkPath = Path.Combine(directory, ApkFileName).Replace('\\', '/');

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            Debug.Log("[PanoramaAndroidBuild] Building " + ScenePath + " -> " + apkPath +
                      " (quality: " + QualitySettings.names[QualitySettings.GetQualityLevel()] +
                      ", backend: " + PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) +
                      ", architectures: " + PlayerSettings.Android.targetArchitectures + ")");

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            QualitySettings.SetQualityLevel(editorQualityLevel, true);

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[PanoramaAndroidBuild] SUCCESS " + apkPath +
                          " size=" + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB" +
                          " time=" + summary.totalTime.TotalSeconds.ToString("0") + "s");
                return true;
            }

            Debug.LogError("[PanoramaAndroidBuild] FAILED result=" + summary.result +
                           " errors=" + summary.totalErrors);
            return false;
        }

        /// <summary>
        /// Switches to the quality level that uses the mobile pipeline, if one exists.
        /// </summary>
        private static void UseMobilePipelineIfAvailable()
        {
            RenderPipelineAsset mobile = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MobilePipelinePath);
            if (mobile == null)
            {
                Debug.LogWarning("[PanoramaAndroidBuild] " + MobilePipelinePath + " not found; " +
                                 "keeping the current render pipeline.");
                return;
            }

            int current = QualitySettings.GetQualityLevel();
            if (QualitySettings.GetRenderPipelineAssetAt(current) == mobile)
            {
                Debug.Log("[PanoramaAndroidBuild] Quality level '" + QualitySettings.names[current] +
                          "' already uses the mobile pipeline.");
                return;
            }

            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                if (QualitySettings.GetRenderPipelineAssetAt(i) != mobile) continue;

                QualitySettings.SetQualityLevel(i, true);
                Debug.Log("[PanoramaAndroidBuild] Switched active quality level to '" +
                          QualitySettings.names[i] + "' so the APK uses " + MobilePipelinePath + ".");
                return;
            }

            Debug.LogWarning("[PanoramaAndroidBuild] No quality level uses " + MobilePipelinePath +
                             "; the APK will use " + QualitySettings.GetRenderPipelineAssetAt(current).name + ".");
        }
    }
}
#endif
