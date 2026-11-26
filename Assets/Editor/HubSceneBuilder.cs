using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace FirstHandLab.Editor
{
    /// <summary>
    /// Assembles the First Hand Subterranean Laboratory Hub Scene (HubScene.unity)
    /// exactly matching the reference image layout, scale, lighting, and holographic UI.
    /// </summary>
    [InitializeOnLoad]
    public static class HubSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/HubScene.unity";

        static HubSceneBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfMissing;
        }

        public static void AutoBuildIfMissing()
        {
            if (!File.Exists(ScenePath))
            {
                BuildHubScene();
            }
        }

        // Asset Store Prefab Paths
        private const string RobotSpherePrefabPath = "Assets/RobotSphere/Assets/Prefab/robotSphere.prefab";
        private const string Computer14PrefabPath = "Assets/Cosmic_Retro_Computer_1_FREE/Prefabs/Cosmic_Retro_Computer_14.prefab";
        private const string Computer15PrefabPath = "Assets/Cosmic_Retro_Computer_1_FREE/Prefabs/Cosmic_Retro_Computer_15.prefab";
        private const string Computer3PrefabPath = "Assets/Cosmic_Retro_Computer_1_FREE/Prefabs/Cosmic_Retro_Computer_3.prefab";
        private const string Computer7PrefabPath = "Assets/Cosmic_Retro_Computer_1_FREE/Prefabs/Cosmic_Retro_Computer_7.prefab";

        private const string AirlockDoorFramePath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Modules/Wall/DoorFrame/Wall_2x2_Door_Frame.prefab";
        private const string DoorLeftPath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Modules/Wall/Door/Wall_Door_Left.prefab";
        private const string DoorRightPath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Modules/Wall/Door/Wall_Door_Right.prefab";
        private const string FloorStandardPath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Modules/Floor/Standard/Floor_2x2.prefab";
        private const string FloorStripedPath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Modules/Floor/DualStriped/Floor_2x2_Striped_Straight.prefab";
        private const string WallStandardPath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Modules/Wall/Standard/Wall_2x2.prefab";
        private const string WallCornerPath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Modules/Wall/Standard/Wall_2x2_Corner.prefab";
        private const string BarrelToxicPath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Props/Barrel/Barrel_Toxic.prefab";
        private const string BarrelRadioactivePath = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/Props/Barrel/Barrel_Radioactive.prefab";

        [MenuItem("Tools/First Hand Lab/Build Reference Hub Scene", false, 10)]
        public static void BuildHubScene()
        {
            Debug.Log("[HubSceneBuilder] Starting construction of HubScene matching reference image...");

            // 1. Create or open clean HubScene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Camera
            SetupMainCamera();

            // 3. Setup Materials & Shared Assets
            SetupMaterials(out Material darkMetalMat, out Material warmRockMat, out Material neonCyanMat, 
                out Material neonBlueMat, out Material neonRedMat, out Material neonAmberMat, 
                out Material orbGlassMat, out Material conduitMat, out Material purpleGridMat);

            // 4. Build Environment Roots
            GameObject hubRoot = new GameObject("FirstHand_HubEnvironment");

            // A. Warm Amber Rocky Cavern Background
            BuildCavernBackground(hubRoot.transform, warmRockMat, neonAmberMat);

            // B. Central Circular Platform & Industrial Rotunda Framing
            BuildCentralRotunda(hubRoot.transform, darkMetalMat, neonCyanMat);

            // C. Right Elevated Catwalk, Airlock Door & Hover Drone
            BuildRightCatwalkAndDrone(hubRoot.transform, darkMetalMat);

            // D. Left Server Racks, Wall Panels, Cables & Robot Arms
            BuildLeftServerBay(hubRoot.transform, darkMetalMat, neonBlueMat, neonRedMat);

            // E. Semi-Circular Front Workstation Console
            BuildWorkstationConsole(hubRoot.transform, darkMetalMat, neonCyanMat, neonBlueMat, neonRedMat);

            // F. 4 Floating Glowing Portal Orbs
            BuildPortalOrbs(hubRoot.transform, orbGlassMat, neonCyanMat, conduitMat);

            // G. Tilted Holographic UI (Place the Orb)
            BuildHologramUI(hubRoot.transform, purpleGridMat);

            // 5. Lighting & Post-Processing (Bloom + ACES Tonemapping)
            SetupLightingAndPostProcessing(hubRoot.transform, neonAmberMat);

            // 6. Save Scene
            EditorSceneManager.SaveScene(newScene, ScenePath);
            RegisterSceneInBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[HubSceneBuilder] Successfully constructed and saved {ScenePath}!");
        }

        #region Camera Setup

        private static void SetupMainCamera()
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 1.6f, 0f);
            camGo.transform.rotation = Quaternion.identity;

            Camera cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 100f;
            cam.backgroundColor = new Color(0.04f, 0.03f, 0.05f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            camGo.AddComponent<AudioListener>();

            // Universal Additional Camera Data
            var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
        }

        #endregion

        #region Material Generation

        private static void SetupMaterials(
            out Material darkMetalMat, out Material warmRockMat, out Material neonCyanMat, 
            out Material neonBlueMat, out Material neonRedMat, out Material neonAmberMat, 
            out Material orbGlassMat, out Material conduitMat, out Material purpleGridMat)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");

            // Dark matte industrial metal for rotunda, trusses and consoles
            darkMetalMat = new Material(litShader);
            darkMetalMat.name = "M_Hub_DarkMetal";
            darkMetalMat.color = new Color(0.18f, 0.20f, 0.24f);
            if (darkMetalMat.HasProperty("_Metallic")) darkMetalMat.SetFloat("_Metallic", 0.75f);
            if (darkMetalMat.HasProperty("_Smoothness")) darkMetalMat.SetFloat("_Smoothness", 0.55f);

            // Warm amber subterranean cavern rock
            warmRockMat = new Material(litShader);
            warmRockMat.name = "M_Hub_CavernRock";
            warmRockMat.color = new Color(0.38f, 0.24f, 0.16f);
            if (warmRockMat.HasProperty("_Metallic")) warmRockMat.SetFloat("_Metallic", 0.05f);
            if (warmRockMat.HasProperty("_Smoothness")) warmRockMat.SetFloat("_Smoothness", 0.25f);

            // Emissive Neon Cyan (0.15, 0.9, 1.0)
            neonCyanMat = new Material(litShader);
            neonCyanMat.name = "M_Hub_NeonCyan";
            neonCyanMat.color = new Color(0.2f, 0.9f, 1.0f);
            neonCyanMat.EnableKeyword("_EMISSION");
            if (neonCyanMat.HasProperty("_EmissionColor"))
                neonCyanMat.SetColor("_EmissionColor", new Color(0.15f, 0.9f, 1.0f) * 3.5f);

            // Emissive Neon Electric Blue (0.1, 0.6, 1.0)
            neonBlueMat = new Material(litShader);
            neonBlueMat.name = "M_Hub_NeonBlue";
            neonBlueMat.color = new Color(0.1f, 0.6f, 1.0f);
            neonBlueMat.EnableKeyword("_EMISSION");
            if (neonBlueMat.HasProperty("_EmissionColor"))
                neonBlueMat.SetColor("_EmissionColor", new Color(0.1f, 0.6f, 1.0f) * 4.0f);

            // Emissive Warning Red (1.0, 0.15, 0.1)
            neonRedMat = new Material(litShader);
            neonRedMat.name = "M_Hub_NeonRed";
            neonRedMat.color = new Color(1.0f, 0.15f, 0.1f);
            neonRedMat.EnableKeyword("_EMISSION");
            if (neonRedMat.HasProperty("_EmissionColor"))
                neonRedMat.SetColor("_EmissionColor", new Color(1.0f, 0.15f, 0.1f) * 3.8f);

            // Warm Amber Glow for cavern illumination
            neonAmberMat = new Material(litShader);
            neonAmberMat.name = "M_Hub_WarmAmber";
            neonAmberMat.color = new Color(1.0f, 0.55f, 0.18f);
            neonAmberMat.EnableKeyword("_EMISSION");
            if (neonAmberMat.HasProperty("_EmissionColor"))
                neonAmberMat.SetColor("_EmissionColor", new Color(1.0f, 0.55f, 0.18f) * 2.5f);

            // Translucent glowing orb glass shader
            orbGlassMat = new Material(litShader);
            orbGlassMat.name = "M_Hub_OrbGlass";
            Color glassCol = new Color(0.35f, 0.85f, 1.0f, 0.45f);
            orbGlassMat.color = glassCol;
            if (orbGlassMat.HasProperty("_Surface")) orbGlassMat.SetFloat("_Surface", 1); // Transparent
            if (orbGlassMat.HasProperty("_Blend")) orbGlassMat.SetFloat("_Blend", 0);
            orbGlassMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            orbGlassMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            orbGlassMat.SetInt("_ZWrite", 0);
            orbGlassMat.renderQueue = (int)RenderQueue.Transparent;
            if (orbGlassMat.HasProperty("_Smoothness")) orbGlassMat.SetFloat("_Smoothness", 0.98f);
            if (orbGlassMat.HasProperty("_Metallic")) orbGlassMat.SetFloat("_Metallic", 0.1f);
            orbGlassMat.EnableKeyword("_EMISSION");
            if (orbGlassMat.HasProperty("_EmissionColor"))
                orbGlassMat.SetColor("_EmissionColor", new Color(0.2f, 0.85f, 1.0f) * 2.2f);

            // Dark ribbed flexible conduit cable material
            conduitMat = new Material(litShader);
            conduitMat.name = "M_Hub_RibbedConduit";
            conduitMat.color = new Color(0.08f, 0.09f, 0.11f);
            if (conduitMat.HasProperty("_Metallic")) conduitMat.SetFloat("_Metallic", 0.3f);
            if (conduitMat.HasProperty("_Smoothness")) conduitMat.SetFloat("_Smoothness", 0.35f);

            // Hologram UI translucent cyber grid
            purpleGridMat = new Material(unlitShader);
            purpleGridMat.name = "M_Hub_HoloGrid";
            Color gridCol = new Color(0.32f, 0.12f, 0.58f, 0.78f);
            purpleGridMat.color = gridCol;
            if (purpleGridMat.HasProperty("_Surface")) purpleGridMat.SetFloat("_Surface", 1);
            purpleGridMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            purpleGridMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            purpleGridMat.SetInt("_ZWrite", 0);
            purpleGridMat.renderQueue = (int)RenderQueue.Transparent + 50;
        }

        #endregion

        #region Environment Construction

        private static void BuildCavernBackground(Transform parent, Material rockMat, Material amberMat)
        {
            GameObject cavern = new GameObject("CavernRockWalls");
            cavern.transform.SetParent(parent, false);

            // Multi-faceted rocky cavern backdrop surrounding the rear and center opening
            int rockCount = 14;
            float radius = 9.5f;
            for (int i = 0; i < rockCount; i++)
            {
                float angle = -65f + (i * (130f / (rockCount - 1)));
                float rad = angle * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(rad) * radius, UnityEngine.Random.Range(1.0f, 3.5f), Mathf.Cos(rad) * radius);

                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = $"CavernSpire_{i}";
                rock.transform.SetParent(cavern.transform, false);
                rock.transform.position = pos;
                rock.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(-12f, 12f), angle + 180f, UnityEngine.Random.Range(-15f, 15f));
                rock.transform.localScale = new Vector3(UnityEngine.Random.Range(2.5f, 4.2f), UnityEngine.Random.Range(7.0f, 12.0f), UnityEngine.Random.Range(2.0f, 3.5f));
                rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            }

            // Central vertical structural rock column seen in the photo
            GameObject centerPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            centerPillar.name = "CavernCentralPillar";
            centerPillar.transform.SetParent(cavern.transform, false);
            centerPillar.transform.position = new Vector3(0.35f, 4.0f, 7.2f);
            centerPillar.transform.localScale = new Vector3(1.1f, 6.0f, 1.1f);
            centerPillar.GetComponent<Renderer>().sharedMaterial = rockMat;
        }

        private static void BuildCentralRotunda(Transform parent, Material metalMat, Material cyanMat)
        {
            GameObject rotunda = new GameObject("CentralRotunda");
            rotunda.transform.SetParent(parent, false);

            // 1. Central Circular Floor Platform at (0, 0, 0)
            GameObject floorDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floorDisk.name = "CentralPlatformFloor";
            floorDisk.transform.SetParent(rotunda.transform, false);
            floorDisk.transform.position = new Vector3(0f, -0.05f, 0f);
            floorDisk.transform.localScale = new Vector3(6.5f, 0.1f, 6.5f);
            floorDisk.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Outer ring collar
            GameObject outerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            outerRing.name = "FloorPerimeterRing";
            outerRing.transform.SetParent(rotunda.transform, false);
            outerRing.transform.position = new Vector3(0f, -0.08f, 0f);
            outerRing.transform.localScale = new Vector3(7.2f, 0.08f, 7.2f);
            outerRing.GetComponent<Renderer>().sharedMaterial = metalMat;

            // 2. Perimeter Guard Railings around the central platform opening
            int railPosts = 10;
            float railRadius = 3.1f;
            for (int i = 0; i <= railPosts; i++)
            {
                float angle = -65f + (i * (130f / railPosts));
                float rad = angle * Mathf.Deg2Rad;
                Vector3 postPos = new Vector3(Mathf.Sin(rad) * railRadius, 0.55f, Mathf.Cos(rad) * railRadius);

                // Vertical rail post
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"RailPost_{i}";
                post.transform.SetParent(rotunda.transform, false);
                post.transform.position = postPos;
                post.transform.localScale = new Vector3(0.06f, 0.55f, 0.06f);
                post.GetComponent<Renderer>().sharedMaterial = metalMat;
            }

            // Top curved horizontal railing beam
            for (int i = 0; i < railPosts; i++)
            {
                float a1 = (-65f + (i * (130f / railPosts))) * Mathf.Deg2Rad;
                float a2 = (-65f + ((i + 1) * (130f / railPosts))) * Mathf.Deg2Rad;
                Vector3 p1 = new Vector3(Mathf.Sin(a1) * railRadius, 1.1f, Mathf.Cos(a1) * railRadius);
                Vector3 p2 = new Vector3(Mathf.Sin(a2) * railRadius, 1.1f, Mathf.Cos(a2) * railRadius);

                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = $"RailSegment_{i}";
                bar.transform.SetParent(rotunda.transform, false);
                bar.transform.position = (p1 + p2) * 0.5f;
                bar.transform.LookAt(p2);
                bar.transform.localScale = new Vector3(0.06f, 0.05f, Vector3.Distance(p1, p2));
                bar.GetComponent<Renderer>().sharedMaterial = metalMat;
            }

            // 3. Overhead Trusses & Arch Ring Framing
            GameObject trussRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trussRing.name = "OverheadTrussArch";
            trussRing.transform.SetParent(rotunda.transform, false);
            trussRing.transform.position = new Vector3(0f, 4.2f, 1.2f);
            trussRing.transform.localScale = new Vector3(7.4f, 0.22f, 7.4f);
            trussRing.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Transverse overhead support beam
            GameObject trussBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trussBeam.name = "OverheadCrossTruss";
            trussBeam.transform.SetParent(rotunda.transform, false);
            trussBeam.transform.position = new Vector3(0f, 3.85f, 0.8f);
            trussBeam.transform.localScale = new Vector3(6.8f, 0.25f, 0.35f);
            trussBeam.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Recessed ceiling lamp fixture
            GameObject lampFixture = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lampFixture.name = "RotundaOverheadLamp";
            lampFixture.transform.SetParent(rotunda.transform, false);
            lampFixture.transform.position = new Vector3(2.4f, 3.9f, 1.8f);
            lampFixture.transform.rotation = Quaternion.Euler(0f, -35f, 18f);
            lampFixture.transform.localScale = new Vector3(0.55f, 0.35f, 0.2f);
            lampFixture.GetComponent<Renderer>().sharedMaterial = metalMat;

            GameObject lampBulb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lampBulb.name = "LampBulb_Warm";
            lampBulb.transform.SetParent(lampFixture.transform, false);
            lampBulb.transform.localPosition = new Vector3(0f, 0f, 0.11f);
            lampBulb.transform.localScale = new Vector3(0.8f, 0.7f, 0.1f);
            lampBulb.GetComponent<Renderer>().sharedMaterial = cyanMat;
        }

        private static void BuildRightCatwalkAndDrone(Transform parent, Material metalMat)
        {
            GameObject rightBay = new GameObject("RightCatwalkBay");
            rightBay.transform.SetParent(parent, false);

            // 1. Elevated Catwalk Deck (X = 2.4 to 4.8, Y = 0.95, Z = 0.5 to 5.2)
            var floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FloorStripedPath) 
                           ?? AssetDatabase.LoadAssetAtPath<GameObject>(FloorStandardPath);

            for (float z = 1.0f; z <= 4.5f; z += 1.8f)
            {
                if (floorPrefab != null)
                {
                    GameObject catTile = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab);
                    catTile.name = $"CatwalkFloor_Z_{z:F1}";
                    catTile.transform.SetParent(rightBay.transform, false);
                    catTile.transform.position = new Vector3(3.2f, 0.95f, z);
                    catTile.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                }
                else
                {
                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"CatwalkFloor_Z_{z:F1}";
                    tile.transform.SetParent(rightBay.transform, false);
                    tile.transform.position = new Vector3(3.2f, 0.95f, z);
                    tile.transform.localScale = new Vector3(1.8f, 0.15f, 1.8f);
                    tile.GetComponent<Renderer>().sharedMaterial = metalMat;
                }
            }

            // Catwalk Railings along the inner walkway
            for (float z = 0.8f; z <= 4.2f; z += 1.1f)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"CatwalkPost_Z_{z:F1}";
                post.transform.SetParent(rightBay.transform, false);
                post.transform.position = new Vector3(2.28f, 1.48f, z);
                post.transform.localScale = new Vector3(0.05f, 0.52f, 0.05f);
                post.GetComponent<Renderer>().sharedMaterial = metalMat;
            }

            // Horizontal handrail tube
            GameObject railTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            railTube.name = "CatwalkHandrail";
            railTube.transform.SetParent(rightBay.transform, false);
            railTube.transform.position = new Vector3(2.28f, 1.98f, 2.5f);
            railTube.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            railTube.transform.localScale = new Vector3(0.06f, 1.85f, 0.06f);
            railTube.GetComponent<Renderer>().sharedMaterial = metalMat;

            // 2. Sci-Fi Airlock Door at the end of the walkway (X = 3.6m, Y = 1.0m, Z = 4.6m)
            var doorFramePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AirlockDoorFramePath);
            var doorLeftPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DoorLeftPath);
            var doorRightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DoorRightPath);

            if (doorFramePrefab != null)
            {
                GameObject airlock = (GameObject)PrefabUtility.InstantiatePrefab(doorFramePrefab);
                airlock.name = "CatwalkAirlockDoorFrame";
                airlock.transform.SetParent(rightBay.transform, false);
                airlock.transform.position = new Vector3(3.6f, 1.0f, 4.6f);
                airlock.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                if (doorLeftPrefab != null)
                {
                    GameObject dl = (GameObject)PrefabUtility.InstantiatePrefab(doorLeftPrefab);
                    dl.transform.SetParent(airlock.transform, false);
                }
                if (doorRightPrefab != null)
                {
                    GameObject dr = (GameObject)PrefabUtility.InstantiatePrefab(doorRightPrefab);
                    dr.transform.SetParent(airlock.transform, false);
                }
            }

            // 3. Hover Robot / Drone perched on the catwalk railing!
            var dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RobotSpherePrefabPath);
            if (dronePrefab != null)
            {
                GameObject drone = (GameObject)PrefabUtility.InstantiatePrefab(dronePrefab);
                drone.name = "HoverDrone_RobotSphere";
                drone.transform.SetParent(rightBay.transform, false);
                // Exact location in reference image: perched right on the inner catwalk railing at (2.4, 1.85, 2.4)
                drone.transform.position = new Vector3(2.35f, 1.82f, 2.45f);
                drone.transform.rotation = Quaternion.Euler(6f, -38f, 0f); // Looking towards player
                drone.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            }

            // Background barrels near lower corridor
            var toxicBarrel = AssetDatabase.LoadAssetAtPath<GameObject>(BarrelToxicPath);
            if (toxicBarrel != null)
            {
                GameObject barrel = (GameObject)PrefabUtility.InstantiatePrefab(toxicBarrel);
                barrel.transform.SetParent(rightBay.transform, false);
                barrel.transform.position = new Vector3(4.5f, 1.0f, 3.2f);
            }
        }

        private static void BuildLeftServerBay(Transform parent, Material metalMat, Material blueMat, Material redMat)
        {
            GameObject leftBay = new GameObject("LeftServerBay");
            leftBay.transform.SetParent(parent, false);

            // 1. Server Racks placed on the left workstation
            var comp14 = AssetDatabase.LoadAssetAtPath<GameObject>(Computer14PrefabPath);
            var comp15 = AssetDatabase.LoadAssetAtPath<GameObject>(Computer15PrefabPath);
            var comp3 = AssetDatabase.LoadAssetAtPath<GameObject>(Computer3PrefabPath);
            var comp7 = AssetDatabase.LoadAssetAtPath<GameObject>(Computer7PrefabPath);

            Vector3[] rackPositions = {
                new Vector3(-2.6f, 1.05f, 1.2f),
                new Vector3(-2.95f, 1.05f, 1.7f),
                new Vector3(-2.55f, 1.05f, 2.2f)
            };

            GameObject[] rackPrefabs = { comp14, comp15, comp3 };

            for (int i = 0; i < 3; i++)
            {
                if (rackPrefabs[i] != null)
                {
                    GameObject rack = (GameObject)PrefabUtility.InstantiatePrefab(rackPrefabs[i]);
                    rack.name = $"ServerRack_{i + 1}";
                    rack.transform.SetParent(leftBay.transform, false);
                    rack.transform.position = rackPositions[i];
                    rack.transform.rotation = Quaternion.Euler(0f, 65f, 0f);
                    rack.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                }
            }

            // 2. Hanging Blue-Lit Overhead Cables
            int cableCount = 4;
            for (int i = 0; i < cableCount; i++)
            {
                GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cable.name = $"HangingPowerCable_{i}";
                cable.transform.SetParent(leftBay.transform, false);
                cable.transform.position = new Vector3(-2.8f + i * 0.15f, 3.2f - i * 0.1f, 1.5f + i * 0.25f);
                cable.transform.rotation = Quaternion.Euler(45f, 25f, -15f);
                cable.transform.localScale = new Vector3(0.035f, 1.8f, 0.035f);
                cable.GetComponent<Renderer>().sharedMaterial = blueMat;
            }

            // 3. Overhead Mechanical Robot Arms extending above the left desk
            GameObject armBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armBase.name = "MechanicalArmMount";
            armBase.transform.SetParent(leftBay.transform, false);
            armBase.transform.position = new Vector3(-2.1f, 3.4f, 1.4f);
            armBase.transform.localScale = new Vector3(0.35f, 0.15f, 0.45f);
            armBase.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Segment 1
            GameObject armSeg1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armSeg1.name = "ArmSegment_1";
            armSeg1.transform.SetParent(armBase.transform, false);
            armSeg1.transform.localPosition = new Vector3(0f, -0.6f, 0.1f);
            armSeg1.transform.localRotation = Quaternion.Euler(15f, 0f, -12f);
            armSeg1.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
            armSeg1.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Segment 2 with end effector claw
            GameObject armSeg2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armSeg2.name = "ArmSegment_2";
            armSeg2.transform.SetParent(armSeg1.transform, false);
            armSeg2.transform.localPosition = new Vector3(0f, -0.95f, 0.15f);
            armSeg2.transform.localRotation = Quaternion.Euler(-32f, 10f, 8f);
            armSeg2.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            armSeg2.GetComponent<Renderer>().sharedMaterial = metalMat;

            // 4. Wall Control Module with Glowing Blue Lightning Ring & Red Junction Hatch
            GameObject wallConsole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallConsole.name = "LeftWallControlUnit";
            wallConsole.transform.SetParent(leftBay.transform, false);
            wallConsole.transform.position = new Vector3(-1.85f, 1.75f, 2.6f);
            wallConsole.transform.rotation = Quaternion.Euler(0f, 40f, 0f);
            wallConsole.transform.localScale = new Vector3(0.55f, 0.75f, 0.12f);
            wallConsole.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Blue Lightning Ring Module
            GameObject blueRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            blueRing.name = "LightningRingModule";
            blueRing.transform.SetParent(wallConsole.transform, false);
            blueRing.transform.localPosition = new Vector3(-0.15f, 0.12f, 0.08f);
            blueRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            blueRing.transform.localScale = new Vector3(0.35f, 0.04f, 0.35f);
            blueRing.GetComponent<Renderer>().sharedMaterial = blueMat;

            // Red Circular Junction Hatch
            GameObject redHatch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            redHatch.name = "RedJunctionHatch";
            redHatch.transform.SetParent(wallConsole.transform, false);
            redHatch.transform.localPosition = new Vector3(0.18f, -0.1f, 0.08f);
            redHatch.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            redHatch.transform.localScale = new Vector3(0.25f, 0.05f, 0.25f);
            redHatch.GetComponent<Renderer>().sharedMaterial = redMat;

            // Small Mounted Surveillance Camera Sensor on left desk edge
            GameObject cameraSensor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cameraSensor.name = "MountedCameraSensor";
            cameraSensor.transform.SetParent(leftBay.transform, false);
            cameraSensor.transform.position = new Vector3(-1.35f, 1.15f, 1.6f);
            cameraSensor.transform.rotation = Quaternion.Euler(15f, 55f, 0f);
            cameraSensor.transform.localScale = new Vector3(0.14f, 0.14f, 0.22f);
            cameraSensor.GetComponent<Renderer>().sharedMaterial = metalMat;

            GameObject sensorLens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sensorLens.name = "SensorLens";
            sensorLens.transform.SetParent(cameraSensor.transform, false);
            sensorLens.transform.localPosition = new Vector3(0f, 0f, 0.55f);
            sensorLens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            sensorLens.transform.localScale = new Vector3(0.55f, 0.15f, 0.55f);
            sensorLens.GetComponent<Renderer>().sharedMaterial = blueMat;
        }

        private static void BuildWorkstationConsole(
            Transform parent, Material metalMat, Material cyanMat, Material blueMat, Material redMat)
        {
            GameObject consoleRoot = new GameObject("WorkstationConsole");
            consoleRoot.transform.SetParent(parent, false);
            consoleRoot.transform.position = new Vector3(0f, 0f, 1.8f);

            // 1. Semi-Circular Front Desk Surface
            GameObject deskSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deskSurface.name = "ConsoleDeskSurface";
            deskSurface.transform.SetParent(consoleRoot.transform, false);
            deskSurface.transform.position = new Vector3(0f, 0.88f, 0f);
            deskSurface.transform.localScale = new Vector3(2.4f, 0.08f, 0.85f);
            deskSurface.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Angled Left Wing of the Console
            GameObject leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWing.name = "ConsoleLeftWing";
            leftWing.transform.SetParent(consoleRoot.transform, false);
            leftWing.transform.position = new Vector3(-1.25f, 0.88f, -0.15f);
            leftWing.transform.rotation = Quaternion.Euler(0f, 32f, 0f);
            leftWing.transform.localScale = new Vector3(1.1f, 0.08f, 0.75f);
            leftWing.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Angled Right Wing of the Console
            GameObject rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWing.name = "ConsoleRightWing";
            rightWing.transform.SetParent(consoleRoot.transform, false);
            rightWing.transform.position = new Vector3(1.25f, 0.88f, -0.15f);
            rightWing.transform.rotation = Quaternion.Euler(0f, -32f, 0f);
            rightWing.transform.localScale = new Vector3(1.1f, 0.08f, 0.75f);
            rightWing.GetComponent<Renderer>().sharedMaterial = metalMat;

            // 2. Central Docking Pedestal & Tray
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "CentralDockingPedestal";
            pedestal.transform.SetParent(consoleRoot.transform, false);
            pedestal.transform.position = new Vector3(0f, 0.44f, 0f);
            pedestal.transform.localScale = new Vector3(0.75f, 0.44f, 0.75f);
            pedestal.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Top Square Receptacle Tray with rounded chamfer
            GameObject tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "OrbReceptacleTray";
            tray.transform.SetParent(consoleRoot.transform, false);
            tray.transform.position = new Vector3(0f, 0.94f, 0.02f);
            tray.transform.localScale = new Vector3(0.55f, 0.04f, 0.55f);
            tray.GetComponent<Renderer>().sharedMaterial = metalMat;

            // Tray Inner Indentation / Pad
            GameObject trayPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trayPad.name = "TrayGripPad";
            trayPad.transform.SetParent(tray.transform, false);
            trayPad.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            trayPad.transform.localScale = new Vector3(0.85f, 0.15f, 0.85f);
            trayPad.GetComponent<Renderer>().sharedMaterial = cyanMat;

            // 3. Vertical Glowing Blue Energy Tube behind the tray (Exact Hero Element in Reference)
            GameObject tubeBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tubeBase.name = "EnergyTubeBase";
            tubeBase.transform.SetParent(consoleRoot.transform, false);
            tubeBase.transform.position = new Vector3(0f, 1.25f, 0.38f);
            tubeBase.transform.localScale = new Vector3(0.22f, 0.35f, 0.22f);
            tubeBase.GetComponent<Renderer>().sharedMaterial = metalMat;

            GameObject glowingCore = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glowingCore.name = "VerticalGlowingEnergyCore";
            glowingCore.transform.SetParent(tubeBase.transform, false);
            glowingCore.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            glowingCore.transform.localScale = new Vector3(0.65f, 0.85f, 0.65f);
            glowingCore.GetComponent<Renderer>().sharedMaterial = blueMat;

            // Dedicated point light for the energy tube
            GameObject tubeLightGo = new GameObject("EnergyTubePointLight");
            tubeLightGo.transform.SetParent(glowingCore.transform, false);
            Light tl = tubeLightGo.AddComponent<Light>();
            tl.type = LightType.Point;
            tl.color = new Color(0.2f, 0.75f, 1.0f);
            tl.intensity = 2.5f;
            tl.range = 3.2f;
        }

        #endregion

        #region Portal Orbs

        private static void BuildPortalOrbs(Transform parent, Material glassMat, Material cyanMat, Material conduitMat)
        {
            GameObject orbsGroup = new GameObject("PortalOrbs_HorizontalArc");
            orbsGroup.transform.SetParent(parent, false);

            string[] orbNames = { "Orb_ClockTower", "Orb_CyberStreet", "Orb_SkyPeak", "Orb_HoloArena" };
            float[] xPositions = { -0.62f, -0.21f, 0.21f, 0.62f };
            float[] zPositions = { 1.45f, 1.55f, 1.55f, 1.45f };

            for (int i = 0; i < 4; i++)
            {
                Vector3 orbPos = new Vector3(xPositions[i], 1.06f, zPositions[i]);
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = orbNames[i];
                orb.transform.SetParent(orbsGroup.transform, false);
                orb.transform.position = orbPos;
                orb.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
                orb.GetComponent<Renderer>().sharedMaterial = glassMat;

                // Inner core sphere (swirling cyan energy)
                GameObject innerCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                innerCore.name = "InnerEnergyCore";
                innerCore.transform.SetParent(orb.transform, false);
                innerCore.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
                innerCore.GetComponent<Renderer>().sharedMaterial = cyanMat;

                // Subtle point light for each orb
                GameObject orbLight = new GameObject("OrbLight");
                orbLight.transform.SetParent(orb.transform, false);
                Light ol = orbLight.AddComponent<Light>();
                ol.type = LightType.Point;
                ol.color = new Color(0.25f, 0.85f, 1.0f);
                ol.intensity = 1.4f;
                ol.range = 1.5f;

                // Dark Ribbed Conduit Cable connecting orb underside to the console pedestal
                GameObject conduit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                conduit.name = $"RibbedConduit_{orbNames[i]}";
                conduit.transform.SetParent(orbsGroup.transform, false);
                Vector3 anchorPos = new Vector3(xPositions[i] * 0.45f, 0.85f, 1.72f);
                Vector3 orbBottom = orbPos + new Vector3(0f, -0.11f, 0f);
                conduit.transform.position = (orbBottom + anchorPos) * 0.5f;
                conduit.transform.LookAt(orbBottom);
                conduit.transform.localScale = new Vector3(0.025f, Vector3.Distance(orbBottom, anchorPos) * 0.5f, 0.025f);
                conduit.GetComponent<Renderer>().sharedMaterial = conduitMat;

                // Add gentle floating bob component
                var bob = orb.AddComponent<OrbFloatingBob>();
                bob.SetPhase(i * 0.65f);
            }
        }

        #endregion

        #region Holographic UI Construction

        private static void BuildHologramUI(Transform parent, Material gridMat)
        {
            GameObject uiRoot = new GameObject("Hologram_UI");
            uiRoot.transform.SetParent(parent, false);
            // Positioned right in front of player, tilted backwards at ~28 degrees matching photo
            uiRoot.transform.position = new Vector3(0f, 0.72f, 1.15f);
            uiRoot.transform.rotation = Quaternion.Euler(28f, 0f, 0f);

            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            uiRoot.AddComponent<CanvasScaler>();
            uiRoot.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = uiRoot.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(800f, 440f);
            canvasRect.localScale = new Vector3(0.0011f, 0.0011f, 0.0011f);

            // 1. Translucent Purple Cyber-Grid Background Panel
            GameObject panelGo = new GameObject("CyberGridPanel");
            panelGo.transform.SetParent(uiRoot.transform, false);
            RectTransform panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(760f, 380f);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.24f, 0.08f, 0.45f, 0.72f); // Vibrant cyber purple

            // Cyber grid lines overlay
            GameObject gridOverlay = new GameObject("GridLines");
            gridOverlay.transform.SetParent(panelGo.transform, false);
            RectTransform gridRect = gridOverlay.AddComponent<RectTransform>();
            gridRect.sizeDelta = new Vector2(750f, 370f);
            Image gridImg = gridOverlay.AddComponent<Image>();
            gridImg.color = new Color(0.35f, 0.15f, 0.65f, 0.45f);

            // 2. Header Bar ("PLACE THE ORB" and "0 / 25")
            GameObject headerGo = new GameObject("HeaderBar");
            headerGo.transform.SetParent(panelGo.transform, false);
            RectTransform headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(720f, 45f);
            headerRect.anchoredPosition = new Vector2(0f, 150f);

            Image headerBg = headerGo.AddComponent<Image>();
            headerBg.color = new Color(0.85f, 0.85f, 0.95f, 0.92f);

            // "PLACE THE ORB" Text
            GameObject titleTextGo = new GameObject("TitleText");
            titleTextGo.transform.SetParent(headerGo.transform, false);
            RectTransform titleRect = titleTextGo.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 0f);
            titleRect.sizeDelta = new Vector2(400f, 40f);
            Text titleTxt = titleTextGo.AddComponent<Text>();
            titleTxt.text = "PLACE THE ORB";
            titleTxt.fontSize = 26;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(0.22f, 0.14f, 0.35f);

            // "0 / 25" Progress Counter Text
            GameObject counterGo = new GameObject("CounterText");
            counterGo.transform.SetParent(headerGo.transform, false);
            RectTransform counterRect = counterGo.AddComponent<RectTransform>();
            counterRect.anchoredPosition = new Vector2(280f, 0f);
            counterRect.sizeDelta = new Vector2(120f, 40f);
            Text counterTxt = counterGo.AddComponent<Text>();
            counterTxt.text = "0 / 25";
            counterTxt.fontSize = 20;
            counterTxt.fontStyle = FontStyle.Bold;
            counterTxt.alignment = TextAnchor.MiddleRight;
            counterTxt.color = new Color(0.35f, 0.25f, 0.5f);

            // 3. Left Card: "Power Glove" [LOCKED]
            BuildUICard(panelGo.transform, new Vector2(-190f, 35f), "Power Glove", "LOCKED");

            // 4. Right Card: "Locomotion" [LOCKED]
            BuildUICard(panelGo.transform, new Vector2(190f, 35f), "Locomotion", "LOCKED");

            // 5. Center Blue Pill Tooltip ("Place an orb into the console to begin")
            GameObject tooltipGo = new GameObject("CenterTooltipPill");
            tooltipGo.transform.SetParent(panelGo.transform, false);
            RectTransform tooltipRect = tooltipGo.AddComponent<RectTransform>();
            tooltipRect.sizeDelta = new Vector2(340f, 36f);
            tooltipRect.anchoredPosition = new Vector2(0f, 50f);

            Image tooltipBg = tooltipGo.AddComponent<Image>();
            tooltipBg.color = new Color(0.08f, 0.48f, 0.95f, 0.95f); // Bright blue pill

            GameObject tooltipTextGo = new GameObject("TooltipText");
            tooltipTextGo.transform.SetParent(tooltipGo.transform, false);
            RectTransform ttTextRect = tooltipTextGo.AddComponent<RectTransform>();
            ttTextRect.sizeDelta = new Vector2(330f, 30f);
            Text ttTxt = tooltipTextGo.AddComponent<Text>();
            ttTxt.text = "Place an orb into the console to begin";
            ttTxt.fontSize = 15;
            ttTxt.fontStyle = FontStyle.Bold;
            ttTxt.alignment = TextAnchor.MiddleCenter;
            ttTxt.color = Color.white;

            // 6. Circular White Reticle Ring at the bottom
            GameObject reticleGo = new GameObject("BottomReticleRing");
            reticleGo.transform.SetParent(panelGo.transform, false);
            RectTransform reticleRect = reticleGo.AddComponent<RectTransform>();
            reticleRect.sizeDelta = new Vector2(85f, 85f);
            reticleRect.anchoredPosition = new Vector2(0f, -80f);

            Image reticleImg = reticleGo.AddComponent<Image>();
            reticleImg.color = new Color(0.9f, 0.95f, 1.0f, 0.92f);
        }

        private static void BuildUICard(Transform parent, Vector2 pos, string cardTitle, string badgeText)
        {
            GameObject cardGo = new GameObject($"Card_{cardTitle}");
            cardGo.transform.SetParent(parent, false);
            RectTransform cardRect = cardGo.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(240f, 140f);
            cardRect.anchoredPosition = pos;

            Image cardBg = cardGo.AddComponent<Image>();
            cardBg.color = new Color(0.88f, 0.85f, 0.95f, 0.92f); // Pale lavender card body

            // Title Text ("Power Glove" / "Locomotion")
            GameObject textGo = new GameObject("Title");
            textGo.transform.SetParent(cardGo.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0f, -42f);
            textRect.sizeDelta = new Vector2(220f, 32f);
            Text title = textGo.AddComponent<Text>();
            title.text = cardTitle;
            title.fontSize = 17;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(0.25f, 0.15f, 0.38f);

            // Lock Pill Badge
            GameObject badgeGo = new GameObject("LockBadge");
            badgeGo.transform.SetParent(cardGo.transform, false);
            RectTransform badgeRect = badgeGo.AddComponent<RectTransform>();
            badgeRect.sizeDelta = new Vector2(110f, 26f);
            badgeRect.anchoredPosition = new Vector2(0f, -22f);

            Image badgeBg = badgeGo.AddComponent<Image>();
            badgeBg.color = new Color(0.55f, 0.22f, 0.78f, 0.95f); // Deep purple lock pill

            GameObject badgeTextGo = new GameObject("BadgeText");
            badgeTextGo.transform.SetParent(badgeGo.transform, false);
            RectTransform bTextRect = badgeTextGo.AddComponent<RectTransform>();
            bTextRect.sizeDelta = new Vector2(100f, 22f);
            Text bTxt = badgeTextGo.AddComponent<Text>();
            bTxt.text = $"[ 🔒 {badgeText} ]";
            bTxt.fontSize = 12;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;
        }

        #endregion

        #region Lighting & Post-Processing

        private static void SetupLightingAndPostProcessing(Transform parent, Material amberMat)
        {
            GameObject lightingRoot = new GameObject("HubLightingAndFX");
            lightingRoot.transform.SetParent(parent, false);

            // 1. Warm Cavern Point Lights (Amber / Orange)
            Vector3[] amberPositions = {
                new Vector3(-4.5f, 2.5f, 6.5f),
                new Vector3(4.5f, 2.5f, 6.5f),
                new Vector3(0f, 3.8f, 7.5f)
            };

            foreach (var pos in amberPositions)
            {
                GameObject lightGo = new GameObject("CavernAmberLight");
                lightGo.transform.SetParent(lightingRoot.transform, false);
                lightGo.transform.position = pos;
                Light l = lightGo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1.0f, 0.58f, 0.18f); // Warm amber
                l.intensity = 3.2f;
                l.range = 8.5f;
            }

            // 2. Cool Console & Rotunda Point Lights (Electric Cyan)
            GameObject consoleLight = new GameObject("ConsoleCyanLight");
            consoleLight.transform.SetParent(lightingRoot.transform, false);
            consoleLight.transform.position = new Vector3(0f, 2.2f, 1.5f);
            Light cl = consoleLight.AddComponent<Light>();
            cl.type = LightType.Point;
            cl.color = new Color(0.2f, 0.85f, 1.0f);
            cl.intensity = 2.0f;
            cl.range = 4.5f;

            // 3. Subtle Ambient Directional Fill
            GameObject sunGo = new GameObject("AmbientFillLight");
            sunGo.transform.SetParent(lightingRoot.transform, false);
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.75f, 0.78f, 0.85f);
            sun.intensity = 0.65f;

            // 4. URP Global Volume with Bloom & ACES Tonemapping
            GameObject volumeGo = new GameObject("GlobalPostProcessingVolume");
            volumeGo.transform.SetParent(lightingRoot.transform, false);
            Volume volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "HubScene_VolumeProfile";

            // Add Bloom
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 0.85f;
            bloom.intensity.value = 1.35f;
            bloom.scatter.value = 0.7f;

            // Add Tonemapping
            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.ACES;

            volume.profile = profile;

            // Save VolumeProfile asset to Settings
            string profilePath = "Assets/Settings/HubScene_VolumeProfile.asset";
            AssetDatabase.CreateAsset(profile, profilePath);

            // Ambient Settings
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.10f, 0.16f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.28f, 0.14f, 0.08f); // Warm subterranean dust
            RenderSettings.fogDensity = 0.008f;
        }

        #endregion

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var existing = EditorBuildSettings.scenes;
            foreach (var s in existing)
            {
                if (s.path.Equals(scenePath, StringComparison.OrdinalIgnoreCase)) return;
            }

            var updated = new EditorBuildSettingsScene[existing.Length + 1];
            Array.Copy(existing, updated, existing.Length);
            updated[updated.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }

    public class HubScenePostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!File.Exists("Assets/Scenes/HubScene.unity"))
            {
                HubSceneBuilder.BuildHubScene();
            }
        }
    }
}
