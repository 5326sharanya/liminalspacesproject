using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FirstHandLab.Editor
{
    /// <summary>
    /// Editor utility for one-click setup of the First Hand Laboratory Experience.
    /// Accessible via menu 'First Hand Lab > Build Complete Lab Experience'.
    /// Creates the panoramic observation lab, physical workbench console,
    /// dynamic HUD, first-person player controller, and pinkish alien vista.
    /// </summary>
    public static class FirstHandSceneSetupEditor
    {
        private const string SceneDirectory = "Assets/FirstHandLab/Scenes";
        private const string ScenePath = "Assets/FirstHandLab/Scenes/FirstHandLabScene.unity";

        [MenuItem("First Hand Lab/Build Complete Lab Experience", false, 10)]
        public static void BuildCompleteLabExperience()
        {
            Debug.Log("[FirstHandLab] Starting automated scene generation...");

            // 1. Ensure target scenes folder exists
            if (!Directory.Exists(SceneDirectory))
            {
                Directory.CreateDirectory(SceneDirectory);
                AssetDatabase.Refresh();
            }

            // 2. Create clean new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Build Procedural Laboratory Hierarchy
            GameObject labRoot = LabEnvironmentBuilder.BuildLabHierarchy();
            Debug.Log("[FirstHandLab] Built procedural lab pod, tactile console workbench, and alien vista.");

            // 4. Construct Player Rig
            GameObject playerGo = new GameObject("FirstHand_Player");
            playerGo.transform.position = new Vector3(0f, 1.05f, 1.75f);
            playerGo.transform.rotation = Quaternion.identity;

            CharacterController cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Camera rig
            GameObject camGo = new GameObject("FirstPersonCamera");
            camGo.transform.SetParent(playerGo.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;

            camGo.AddComponent<AudioListener>();

            // Hold anchor for grabbable cores
            GameObject holdAnchor = new GameObject("CoreHoldAnchor");
            holdAnchor.transform.SetParent(camGo.transform, false);
            holdAnchor.transform.localPosition = new Vector3(0.28f, -0.22f, 0.65f);

            // Player Controller
            FirstHandPlayerController playerController = playerGo.AddComponent<FirstHandPlayerController>();

            // 5. HUD Setup
            GameObject hudGo = new GameObject("FirstHand_HUD");
            hudGo.transform.SetParent(playerGo.transform, false);
            hudGo.AddComponent<FirstHandHUD>();

            // 6. Atmospheric Lighting & Fog
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.18f, 0.28f); // Soft sci-fi violet ambient
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.65f, 0.42f, 0.58f); // Pinkish twilight vista haze
            RenderSettings.fogDensity = 0.008f;

            // 7. Save Scene to Assets
            EditorSceneManager.SaveScene(newScene, ScenePath);
            Debug.Log($"[FirstHandLab] Scene successfully saved to {ScenePath}");

            // 8. Register in EditorBuildSettings
            RegisterSceneInBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[FirstHandLab] Setup Complete! The First Hand lab experience is ready to play.");
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (var s in existing)
            {
                if (s.path.Equals(scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return; // Already present
                }
            }

            EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[existing.Length + 1];
            Array.Copy(existing, updated, existing.Length);
            updated[updated.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
