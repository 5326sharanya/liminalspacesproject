#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiminalSpace.Editor
{
    /// <summary>
    /// Editor tool to build, populate, and configure the entire Liminal Space experience with 1 click.
    /// Accessible via top menu: "Liminal Space" -> "Build Complete Liminal Experience".
    /// </summary>
    public static class LiminalSceneSetupEditor
    {
        [MenuItem("Liminal Space/Build Complete Liminal Experience", false, 1)]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Currently in Play Mode",
                    "You are currently running in Play Mode!\n\n" +
                    "To view and play the game right now:\n" +
                    "Click the 'Game' tab (next to the 'Scene' tab in the top-left).\n\n" +
                    "To generate/bake the world into the Scene editor permanently:\n" +
                    "1. Click the Play button at the top to stop Play Mode.\n" +
                    "2. Click 'Liminal Space > Build Complete Liminal Experience' again.", "Got it!");
                return;
            }

            Camera oldCam = Camera.main;
            if (oldCam != null && oldCam.transform.parent == null)
            {
                Object.DestroyImmediate(oldCam.gameObject);
            }

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional && l.transform.parent == null)
                {
                    Object.DestroyImmediate(l.gameObject);
                }
            }

            // Setup Hub atmospheric lighting for the Scene editor preview
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.17f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.12f, 0.11f, 0.08f);
            RenderSettings.fogDensity = 0.018f;

            // Find or create LiminalExperienceManager
            LiminalGameManager manager = Object.FindFirstObjectByType<LiminalGameManager>();
            if (manager == null)
            {
                GameObject mgrObj = new GameObject("LiminalExperienceManager");
                manager = mgrObj.AddComponent<LiminalGameManager>();
            }

            if (Object.FindFirstObjectByType<LiminalVisualEffects>() == null)
            {
                manager.gameObject.AddComponent<LiminalVisualEffects>();
            }
            if (Object.FindFirstObjectByType<LiminalHUD>() == null)
            {
                manager.gameObject.AddComponent<LiminalHUD>();
            }
            if (Object.FindFirstObjectByType<CinematicSubtitles>() == null)
            {
                manager.gameObject.AddComponent<CinematicSubtitles>();
            }

            // Find or create LiminalWorldBuilder
            LiminalWorldBuilder builder = Object.FindFirstObjectByType<LiminalWorldBuilder>();
            if (builder == null)
            {
                builder = manager.gameObject.AddComponent<LiminalWorldBuilder>();
            }

            LiminalRunState previewRun = new LiminalRunState();
            builder.BuildCompleteWorld(previewRun);

            // Spawn Player if not present
            LiminalPlayerController player = Object.FindFirstObjectByType<LiminalPlayerController>();
            if (player == null)
            {
                manager.SpawnPlayer();
            }

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(new Vector3(0, 1.6f, 8f), Quaternion.Euler(5f, 0f, 0f), 10f);
                SceneView.lastActiveSceneView.Repaint();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#44ff88><b>[Liminal Space]</b> 2-Door Experience built with The Celestial Horizon (Door 1) and The Subterranean Poolrooms (Door 2)!</color>");
            EditorUtility.DisplayDialog("Liminal Space Experience Built!",
                "The 2-Door Liminal Experience is ready!\n\n" +
                "✓ Hub Corridor with 2 Doors: Door 1 & Door 2\n" +
                "✓ Door 1: Ethereal Cosmic Wonder with Gigantic Ringed Planet, Luminous Moon, Celestial Arch, Floating Islands, Cascading Waterfall & Bioluminescent Flora\n" +
                "✓ Door 2: Realistic Creepy Subterranean Poolrooms with Wet Reflective Tiles and Echoing Drips\n" +
                "✓ VR-Ready with 1:1 Metric Scale, 3D Spatial Audio, and Comfort Controls\n\n" +
                "Press PLAY (and check the 'Game' tab) to enter the experience!", "Enter Experience");
        }
    }
}
#endif
