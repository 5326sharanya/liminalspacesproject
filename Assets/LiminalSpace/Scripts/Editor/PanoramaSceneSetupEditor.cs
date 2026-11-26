#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace LiminalSpace.Editor
{
    /// <summary>
    /// Editor tool that generates the 360 panorama skeleton scene: a viewer rig, a URP
    /// camera with an audio listener, and a panorama skybox driven by a placeholder image.
    /// Accessible via top menu: "Liminal Space" -> "Build 360 Panorama Scene".
    /// </summary>
    public static class PanoramaSceneSetupEditor
    {
        private const string ScenePath = "Assets/Scenes/PanoramaViewer.unity";
        private const string MaterialPath = "Assets/Settings/PanoramaSkybox.mat";

        [MenuItem("Liminal Space/Build 360 Panorama Scene", false, 2)]
        public static void BuildPanoramaScene()
        {
            if (EditorApplication.isPlaying)
            {
                Notify("Currently in Play Mode",
                    "Stop Play Mode before generating the panorama scene.", "Got it!");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // A panorama lights itself, so the scene only needs soft flat ambient fill for
            // any interaction props that get added later.
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.21f, 0.20f);

            GameObject rig = new GameObject("PanoramaViewer");
            rig.AddComponent<PanoramaViewer>();
            PanoramaSkybox skybox = rig.AddComponent<PanoramaSkybox>();

            GameObject cameraObject = new GameObject("PanoramaCamera");
            cameraObject.transform.SetParent(rig.transform, false);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 75f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();

            UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = false;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;

            SerializedObject serializedViewer = new SerializedObject(rig.GetComponent<PanoramaViewer>());
            serializedViewer.FindProperty("m_CameraPivot").objectReferenceValue = cameraObject.transform;
            serializedViewer.FindProperty("m_Camera").objectReferenceValue = camera;
            serializedViewer.ApplyModifiedPropertiesWithoutUndo();

            // Persist the generated skybox material as an asset so the scene does not carry
            // an unsaved in-memory material.
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            Material skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (skyboxMaterial == null)
            {
                Shader shader = Shader.Find("Skybox/Panoramic");
                if (shader != null)
                {
                    skyboxMaterial = new Material(shader) { name = "PanoramaSkybox" };
                    AssetDatabase.CreateAsset(skyboxMaterial, MaterialPath);
                }
            }

            if (skyboxMaterial != null)
            {
                SerializedObject serializedSkybox = new SerializedObject(skybox);
                serializedSkybox.FindProperty("m_SkyboxMaterial").objectReferenceValue = skyboxMaterial;
                serializedSkybox.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(skybox);
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

            Notify("360 Panorama Scene Ready",
                "Created " + ScenePath + ".\n\n" +
                "Press Play to look around: drag with the mouse on a laptop, drag with a " +
                "finger on a phone, and move the phone to look around once the gyro kicks in.\n\n" +
                "Double-tap, right-click or press [R] to recenter.\n\n" +
                "To use your own 360 photo, select PanoramaViewer and drop an " +
                "equirectangular image into the Panorama slot.", "Nice");
        }

        [MenuItem("Liminal Space/Open 360 Panorama Scene", false, 3)]
        public static void OpenPanoramaScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (sceneAsset == null)
            {
                Notify("Scene not found",
                    "Run \"Liminal Space > Build 360 Panorama Scene\" first.", "OK");
                return;
            }

            EditorSceneManager.OpenScene(ScenePath);
        }

        private static void Notify(string title, string message, string ok)
        {
            // Batch mode has no UI; a modal dialog there would block an automated build.
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(title, message, ok);
            }
        }

        private static void AddSceneToBuildSettings(string path)
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene buildScene in existing)
            {
                if (buildScene.path == path)
                {
                    return;
                }
            }

            EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[existing.Length + 1];
            existing.CopyTo(updated, 0);
            updated[existing.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif
