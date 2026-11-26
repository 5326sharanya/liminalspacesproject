#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiminalSpace.Editor
{
    /// <summary>
    /// One-click importer for the mountain panorama photo: sets sane texture import
    /// settings, builds a skybox material with the band shader, and wires it into the
    /// PanoramaViewer scene. Menu: "Liminal Space" -> "Apply Mountain Panorama".
    /// Can also be run from the command line with -executeMethod.
    /// </summary>
    public static class PanoramaPhotoApplyEditor
    {
        private const string ScenePath = "Assets/Scenes/PanoramaViewer.unity";
        private const string PhotoPath = "Assets/LiminalSpace/Textures/MountainSummit.jpg";
        private const string MaterialPath = "Assets/Settings/MountainPanorama.mat";
        private const string BandShaderName = "LiminalSpace/Skybox Panorama Band";

        // How much of the sky the photo is assumed to cover vertically. Tweak this in the
        // material if the mountains look too tall or too flat.
        private const float BandVerticalFov = 55f;

        [MenuItem("Liminal Space/Apply Mountain Panorama", false, 4)]
        public static void ApplyMountainPanorama()
        {
            if (!ConfigureTexture(PhotoPath))
            {
                Finish(1);
                return;
            }

            if (!File.Exists(ScenePath))
            {
                Debug.LogError("[PanoramaPhoto] Scene not found at " + ScenePath);
                Finish(1);
                return;
            }

            Texture2D photo = AssetDatabase.LoadAssetAtPath<Texture2D>(PhotoPath);
            if (photo == null)
            {
                Debug.LogError("[PanoramaPhoto] Could not load " + PhotoPath);
                Finish(1);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PanoramaSkybox skybox = Object.FindFirstObjectByType<PanoramaSkybox>();
            if (skybox == null)
            {
                Debug.LogError("[PanoramaPhoto] No PanoramaSkybox in " + ScenePath);
                Finish(1);
                return;
            }

            Material material = BuildMaterial(photo);
            if (material == null)
            {
                Finish(1);
                return;
            }

            SerializedObject serializedSkybox = new SerializedObject(skybox);
            serializedSkybox.FindProperty("m_Panorama").objectReferenceValue = photo;
            serializedSkybox.FindProperty("m_SkyboxMaterial").objectReferenceValue = material;
            serializedSkybox.FindProperty("m_BandVerticalFov").floatValue = BandVerticalFov;
            serializedSkybox.ApplyModifiedPropertiesWithoutUndo();

            // Push it into RenderSettings right away so the saved scene is self-contained.
            skybox.Apply();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log(string.Format(
                "[PanoramaPhoto] Applied {0} ({1}x{2}) with material {3} (shader {4}) to {5}.",
                PhotoPath, photo.width, photo.height, MaterialPath, material.shader.name, ScenePath));

            Finish(0);
        }

        private static bool ConfigureTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("[PanoramaPhoto] Missing texture at " + path);
                return false;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            // Keep the full 4096 wide image and do not resize it to a power of two.
            importer.maxTextureSize = 4096;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
            return true;
        }

        private static Material BuildMaterial(Texture2D photo)
        {
            Shader shader = Shader.Find(BandShaderName);
            if (shader == null)
            {
                Debug.LogError("[PanoramaPhoto] Shader '" + BandShaderName + "' not found. " +
                               "Check Assets/LiminalSpace/Shaders/PanoramaBandSkybox.shader");
                return null;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets", "Settings");
                }

                material = new Material(shader) { name = "MountainPanorama" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture("_MainTex", photo);
            material.SetFloat("_Exposure", 1f);
            material.SetFloat("_Rotation", 0f);
            material.SetFloat("_VerticalFov", BandVerticalFov);
            material.SetFloat("_EdgeFade", 3f);
            material.SetFloat("_MirrorTiling", 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Finish(int exitCode)
        {
            // A batch-mode Unity never returns on its own once -executeMethod ends.
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
            else if (exitCode == 0)
            {
                EditorUtility.DisplayDialog("Mountain Panorama Applied",
                    "The summit photo is now the skybox in " + ScenePath + ".\n\n" +
                    "Press Play to look around. If the mountains look too tall or too flat, " +
                    "select Assets/Settings/MountainPanorama.mat and adjust " +
                    "\"Photo Vertical FOV\".", "Nice");
            }
        }
    }
}
#endif
