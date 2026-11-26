using System;
using System.IO;
using FirstHandLab;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace FirstHandLab.Editor
{
    public static class FirstHandHubBuilder
    {
        private const string ScenePath = "Assets/Scenes/HubScene.unity";
        private const string PrefabDirCosmic = "Assets/Cosmic_Retro_Computer_1_FREE/Prefabs/";
        private const string PrefabDirTyuris = "Assets/Tyuris/Modular_SciFi_Interior_Pack_Lite/Prefabs/";
        private const string RobotPrefabPath = "Assets/RobotSphere/Assets/Prefab/robotSphere.prefab";

        [MenuItem("Tools/Build First Hand Hub Scene")]
        public static void BuildHubSceneMenu()
        {
            BuildHubScene();
        }

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EditorApplication.delayCall += () =>
            {
                Debug.Log("[FirstHandHubBuilder] Auto-building First Hand Hub Scene...");
                BuildHubScene();
            };
        }

        public static void BuildHubScene()
        {
            Debug.Log("[FirstHandHubBuilder] Starting generation of realistic First Hand Hub scene...");

            // Ensure folder exists
            string dir = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Setup root containers
            GameObject envRoot = new GameObject("Environment_Modular");
            GameObject catwalkRoot = new GameObject("Right_Elevated_Catwalk");
            GameObject serverDeckRoot = new GameObject("Left_Server_Deck");
            GameObject consoleRoot = new GameObject("Center_Console_Pedestal");
            GameObject orbsRoot = new GameObject("Glowing_Portal_Orbs");
            GameObject lightingRoot = new GameObject("Lighting_And_Atmosphere");

            // Setup Player & Camera
            GameObject playerGo = new GameObject("Player_Desktop");
            playerGo.transform.position = new Vector3(0f, 0f, 0f);

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(playerGo.transform);
            camGo.transform.position = new Vector3(0f, 1.6f, 0f);
            camGo.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

            Camera cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 100f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();

            FirstHandDesktopController desktopCtrl = playerGo.AddComponent<FirstHandDesktopController>();

            // --- 1. MODULAR ROOM ARCHITECTURE (Using real Tyuris prefabs) ---
            string floorPrefabPath = PrefabDirTyuris + "Modules/Floor/Standard/Floor_2x2.prefab";
            string floorStripedPath = PrefabDirTyuris + "Modules/Floor/DualStriped/Floor_2x2_Striped_Straight.prefab";
            string wallPrefabPath = PrefabDirTyuris + "Modules/Wall/Standard/Wall_2x2.prefab";
            string wallCornerPath = PrefabDirTyuris + "Modules/Wall/Standard/Wall_2x2_Corner.prefab";
            string doorFramePath = PrefabDirTyuris + "Modules/Wall/DoorFrame/Wall_2x2_Door_Frame.prefab";
            string doorLeftPath = PrefabDirTyuris + "Modules/Wall/Door/Wall_Door_Left.prefab";
            string doorRightPath = PrefabDirTyuris + "Modules/Wall/Door/Wall_Door_Right.prefab";
            string ceilingLampPath = PrefabDirTyuris + "Modules/Ceiling/Ceiling_2x2_Lamp.prefab";

            // Main central floor platform (3x3 grid around player)
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 2; z++)
                {
                    string pPath = (x == 0 && z >= 0) ? floorStripedPath : floorPrefabPath;
                    InstantiatePrefabSafe(pPath, new Vector3(x * 2f, 0f, z * 2f), Quaternion.identity, envRoot.transform);
                }
            }

            // Rear and side walls with corners
            InstantiatePrefabSafe(wallCornerPath, new Vector3(-4f, 0f, 4f), Quaternion.Euler(0f, 0f, 0f), envRoot.transform);
            InstantiatePrefabSafe(wallPrefabPath, new Vector3(-2f, 0f, 4f), Quaternion.Euler(0f, 0f, 0f), envRoot.transform);
            InstantiatePrefabSafe(wallPrefabPath, new Vector3(0f, 0f, 4f), Quaternion.Euler(0f, 0f, 0f), envRoot.transform);
            InstantiatePrefabSafe(wallPrefabPath, new Vector3(2f, 0f, 4f), Quaternion.Euler(0f, 0f, 0f), envRoot.transform);
            InstantiatePrefabSafe(wallPrefabPath, new Vector3(-4f, 0f, 2f), Quaternion.Euler(0f, 90f, 0f), envRoot.transform);
            InstantiatePrefabSafe(wallPrefabPath, new Vector3(-4f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), envRoot.transform);

            // Ceiling with illuminated industrial lamps
            InstantiatePrefabSafe(ceilingLampPath, new Vector3(0f, 3.2f, 2f), Quaternion.identity, envRoot.transform);
            InstantiatePrefabSafe(ceilingLampPath, new Vector3(-2f, 3.2f, 2f), Quaternion.identity, envRoot.transform);
            InstantiatePrefabSafe(ceilingLampPath, new Vector3(2f, 3.2f, 2f), Quaternion.identity, envRoot.transform);

            // --- 2. RIGHT ELEVATED CATWALK & AIRLOCK DOOR ---
            // Catwalk deck
            InstantiatePrefabSafe(floorStripedPath, new Vector3(2.8f, 0.5f, 0.5f), Quaternion.identity, catwalkRoot.transform);
            InstantiatePrefabSafe(floorStripedPath, new Vector3(2.8f, 0.5f, 2.5f), Quaternion.identity, catwalkRoot.transform);

            // Bulkhead Airlock Door at the end of the catwalk
            InstantiatePrefabSafe(doorFramePath, new Vector3(2.8f, 0.5f, 3.8f), Quaternion.Euler(0f, 0f, 0f), catwalkRoot.transform);
            InstantiatePrefabSafe(doorLeftPath, new Vector3(2.8f, 0.5f, 3.8f), Quaternion.Euler(0f, 0f, 0f), catwalkRoot.transform);
            InstantiatePrefabSafe(doorRightPath, new Vector3(2.8f, 0.5f, 3.8f), Quaternion.Euler(0f, 0f, 0f), catwalkRoot.transform);

            // Railing on catwalk edge
            GameObject railing = CreateRailing(catwalkRoot.transform, new Vector3(1.8f, 0.9f, 1.5f), 2.8f);

            // --- 3. ROBOT SPHERE DRONE (Perched on the right catwalk railing) ---
            GameObject robotGo = InstantiatePrefabSafe(RobotPrefabPath, new Vector3(1.85f, 1.25f, 1.8f), Quaternion.Euler(0f, -125f, 0f), catwalkRoot.transform);
            if (robotGo != null)
            {
                robotGo.name = "Hover_Drone_Companion";
                DroneCompanion droneComp = robotGo.AddComponent<DroneCompanion>();

                // Add glowing eye light
                GameObject droneLightGo = new GameObject("Drone_Visor_Light");
                droneLightGo.transform.SetParent(robotGo.transform);
                droneLightGo.transform.localPosition = new Vector3(0f, 0.25f, 0.35f);
                Light droneLight = droneLightGo.AddComponent<Light>();
                droneLight.type = LightType.Point;
                droneLight.color = new Color(0f, 0.9f, 1f);
                droneLight.intensity = 2.0f;
                droneLight.range = 3.5f;

                var field = typeof(DroneCompanion).GetField("eyeLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(droneComp, droneLight);
            }

            // --- 4. LEFT SERVER RACKS & COMPUTERS (Real 3D models) ---
            string comp14 = PrefabDirCosmic + "Cosmic_Retro_Computer_14.prefab";
            string comp15 = PrefabDirCosmic + "Cosmic_Retro_Computer_15.prefab";
            string comp3 = PrefabDirCosmic + "Cosmic_Retro_Computer_3.prefab";
            string comp7 = PrefabDirCosmic + "Cosmic_Retro_Computer_7.prefab";

            InstantiatePrefabSafe(comp14, new Vector3(-2.2f, 0f, 1.8f), Quaternion.Euler(0f, 40f, 0f), serverDeckRoot.transform);
            InstantiatePrefabSafe(comp15, new Vector3(-2.6f, 0f, 0.8f), Quaternion.Euler(0f, 55f, 0f), serverDeckRoot.transform);
            InstantiatePrefabSafe(comp3, new Vector3(-1.8f, 0f, 2.6f), Quaternion.Euler(0f, 25f, 0f), serverDeckRoot.transform);
            InstantiatePrefabSafe(comp7, new Vector3(-1.4f, 0f, 0.3f), Quaternion.Euler(0f, 75f, 0f), serverDeckRoot.transform);

            // Overhead blue conduits along left wall
            CreateOverheadCables(serverDeckRoot.transform);

            // --- 5. CENTER CONSOLE PEDESTAL ---
            consoleRoot.transform.position = new Vector3(0f, 0f, 1.4f);
            ConsoleWorkbench workbench = consoleRoot.AddComponent<ConsoleWorkbench>();

            // Pedestal table structure
            GameObject tableMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tableMesh.name = "Console_Desk";
            tableMesh.transform.SetParent(consoleRoot.transform);
            tableMesh.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            tableMesh.transform.localScale = new Vector3(1.4f, 0.84f, 0.7f);
            SetMaterialColor(tableMesh, new Color(0.12f, 0.14f, 0.18f), 0.85f, 0.2f);

            // Central Vertical Glowing Energy Column
            GameObject tubeCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tubeCylinder.name = "Energy_Core_Tube";
            tubeCylinder.transform.SetParent(consoleRoot.transform);
            tubeCylinder.transform.localPosition = new Vector3(0f, 1.15f, 0.1f);
            tubeCylinder.transform.localScale = new Vector3(0.12f, 0.4f, 0.12f);
            Material tubeMat = CreateEmissiveMaterial("Mat_EnergyTube", new Color(0f, 0.95f, 1f) * 2.5f);
            tubeCylinder.GetComponent<Renderer>().sharedMaterial = tubeMat;

            // Glass casing around tube
            GameObject glassCasing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glassCasing.name = "Tube_Glass_Casing";
            glassCasing.transform.SetParent(consoleRoot.transform);
            glassCasing.transform.localPosition = new Vector3(0f, 1.15f, 0.1f);
            glassCasing.transform.localScale = new Vector3(0.18f, 0.42f, 0.18f);
            Collider glassCol = glassCasing.GetComponent<Collider>();
            if (glassCol != null) UnityEngine.Object.DestroyImmediate(glassCol);
            Material glassMat = CreateGlassMaterial("Mat_TubeGlass", new Color(0.2f, 0.8f, 1f, 0.35f));
            glassCasing.GetComponent<Renderer>().sharedMaterial = glassMat;

            // Tube glow light
            GameObject tubeLightGo = new GameObject("Tube_Point_Light");
            tubeLightGo.transform.SetParent(consoleRoot.transform);
            tubeLightGo.transform.localPosition = new Vector3(0f, 1.15f, 0.1f);
            Light tubeLight = tubeLightGo.AddComponent<Light>();
            tubeLight.type = LightType.Point;
            tubeLight.color = new Color(0f, 0.95f, 1f);
            tubeLight.intensity = 2.5f;
            tubeLight.range = 4f;

            // Socket receptacle in front of tube
            GameObject socketGo = new GameObject("Orb_Socket_Point");
            socketGo.transform.SetParent(consoleRoot.transform);
            socketGo.transform.localPosition = new Vector3(0f, 0.92f, -0.15f);

            GameObject socketRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            socketRing.name = "Socket_Rim";
            socketRing.transform.SetParent(socketGo.transform);
            socketRing.transform.localPosition = Vector3.zero;
            socketRing.transform.localScale = new Vector3(0.28f, 0.04f, 0.28f);
            SetMaterialColor(socketRing, new Color(0.08f, 0.09f, 0.12f), 0.9f, 0.1f);

            // Small mounted camera sensor on left of console
            GameObject sensorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sensorGo.name = "Camera_Sensor_Mount";
            sensorGo.transform.SetParent(consoleRoot.transform);
            sensorGo.transform.localPosition = new Vector3(-0.55f, 0.9f, -0.05f);
            sensorGo.transform.localScale = new Vector3(0.1f, 0.08f, 0.12f);
            SetMaterialColor(sensorGo, new Color(0.15f, 0.18f, 0.22f), 0.8f, 0.3f);

            // Red circular junction hatch
            GameObject redHatch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            redHatch.name = "Red_Junction_Hatch";
            redHatch.transform.SetParent(consoleRoot.transform);
            redHatch.transform.localPosition = new Vector3(-0.35f, 0.86f, -0.1f);
            redHatch.transform.localScale = new Vector3(0.12f, 0.03f, 0.12f);
            SetMaterialColor(redHatch, new Color(0.9f, 0.15f, 0.15f), 0.6f, 0.4f);

            // Glowing blue ring module
            GameObject blueRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            blueRing.name = "Glowing_Blue_Ring_Module";
            blueRing.transform.SetParent(consoleRoot.transform);
            blueRing.transform.localPosition = new Vector3(-0.52f, 0.86f, -0.18f);
            blueRing.transform.localScale = new Vector3(0.14f, 0.03f, 0.14f);
            blueRing.GetComponent<Renderer>().sharedMaterial = CreateEmissiveMaterial("Mat_BlueRing", new Color(0f, 0.7f, 1f) * 2f);

            // Link ConsoleWorkbench fields via reflection
            SetPrivateField(workbench, "socketPoint", socketGo.transform);
            SetPrivateField(workbench, "energyTubeRenderer", tubeCylinder.GetComponent<Renderer>());
            SetPrivateField(workbench, "energyTubeLight", tubeLight);

            // --- 6. 4 GLOWING PORTAL ORBS FLOATING IN AN ARC ---
            var orbDefs = new[]
            {
                new { Id = "Orb_ClockTower", Name = "CLOCK TOWER", Color = new Color(0f, 0.9f, 1f), X = -0.48f, Z = 1.25f },
                new { Id = "Orb_CyberStreet", Name = "CYBER STREET", Color = new Color(0f, 1f, 0.8f), X = -0.16f, Z = 1.36f },
                new { Id = "Orb_SkyPeak", Name = "SKY PEAK", Color = new Color(0.3f, 0.75f, 1f), X = 0.16f, Z = 1.36f },
                new { Id = "Orb_HoloArena", Name = "HOLO ARENA", Color = new Color(0.5f, 0.6f, 1f), X = 0.48f, Z = 1.25f }
            };

            for (int i = 0; i < orbDefs.Length; i++)
            {
                var def = orbDefs[i];
                GameObject orbGo = new GameObject(def.Id);
                orbGo.transform.SetParent(orbsRoot.transform);
                orbGo.transform.position = new Vector3(def.X, 1.08f, def.Z);

                SphereCollider sc = orbGo.AddComponent<SphereCollider>();
                sc.radius = 0.12f;

                // Outer iridescent glass shell
                GameObject outerShell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                outerShell.name = "Glass_Shell";
                outerShell.transform.SetParent(orbGo.transform);
                outerShell.transform.localPosition = Vector3.zero;
                outerShell.transform.localScale = Vector3.one * 0.18f;
                Collider c = outerShell.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
                Material oMat = CreateGlassMaterial("Mat_" + def.Id + "_Glass", new Color(def.Color.r, def.Color.g, def.Color.b, 0.4f));
                outerShell.GetComponent<Renderer>().sharedMaterial = oMat;

                // Inner radiant core
                GameObject innerCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                innerCore.name = "Glowing_Core";
                innerCore.transform.SetParent(orbGo.transform);
                innerCore.transform.localPosition = Vector3.zero;
                innerCore.transform.localScale = Vector3.one * 0.10f;
                Collider ic = innerCore.GetComponent<Collider>();
                if (ic != null) UnityEngine.Object.DestroyImmediate(ic);
                Material cMat = CreateEmissiveMaterial("Mat_" + def.Id + "_Core", def.Color * 2.5f);
                innerCore.GetComponent<Renderer>().sharedMaterial = cMat;

                // Point Light
                GameObject lightGo = new GameObject("Orb_Point_Light");
                lightGo.transform.SetParent(orbGo.transform);
                lightGo.transform.localPosition = Vector3.zero;
                Light oLight = lightGo.AddComponent<Light>();
                oLight.type = LightType.Point;
                oLight.color = def.Color;
                oLight.intensity = 1.6f;
                oLight.range = 2.5f;

                // Interactable script
                OrbInteractable interactable = orbGo.AddComponent<OrbInteractable>();
                interactable.Initialize(def.Id, def.Name, def.Color, i * 0.8f);
                SetPrivateField(interactable, "outerRenderer", outerShell.GetComponent<Renderer>());
                SetPrivateField(interactable, "coreRenderer", innerCore.GetComponent<Renderer>());
                SetPrivateField(interactable, "orbLight", oLight);

                // Dark ribbed conduit connecting bottom of orb to console base
                CreateOrbConduit(orbGo.transform, new Vector3(def.X, 0.45f, def.Z));
            }

            // --- 7. HOLOGRAPHIC WORLD-SPACE CANVAS UI ---
            GameObject canvasGo = new GameObject("Hologram_UI_Canvas");
            canvasGo.transform.SetParent(consoleRoot.transform);
            canvasGo.transform.localPosition = new Vector3(0f, 0.88f, -0.32f);
            canvasGo.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            canvasGo.transform.localScale = Vector3.one * 0.0016f;

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(720f, 380f);

            HologramUIController uiCtrl = canvasGo.AddComponent<HologramUIController>();
            BuildHologramUIGeometry(canvasGo.transform, uiCtrl);

            // Connect UI and ConsoleWorkbench references
            SetPrivateField(workbench, "uiController", uiCtrl);
            if (robotGo != null)
            {
                SetPrivateField(workbench, "droneCompanion", robotGo.GetComponent<DroneCompanion>());
            }
            SetPrivateField(desktopCtrl, "consoleWorkbench", workbench);
            SetPrivateField(desktopCtrl, "uiController", uiCtrl);

            // --- 8. LIGHTING & URP GLOBAL VOLUME ---
            // Main warm ambient light (cavern glow)
            GameObject dirLightGo = new GameObject("Warm_Cavern_Directional_Light");
            dirLightGo.transform.SetParent(lightingRoot.transform);
            dirLightGo.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            Light dirLight = dirLightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1.0f, 0.72f, 0.45f); // Warm amber
            dirLight.intensity = 0.85f;

            // Background orange cavern fill point light
            GameObject cavernLight = new GameObject("Cavern_Amber_Point_Light");
            cavernLight.transform.SetParent(lightingRoot.transform);
            cavernLight.transform.position = new Vector3(0f, 2.8f, 5.0f);
            Light cavL = cavernLight.AddComponent<Light>();
            cavL.type = LightType.Point;
            cavL.color = new Color(1.0f, 0.55f, 0.15f);
            cavL.intensity = 3.5f;
            cavL.range = 12f;

            // URP Global Volume (Bloom + ACES Tonemapping + Vignette)
            GameObject volumeGo = new GameObject("Global_PostProcessing_Volume");
            volumeGo.transform.SetParent(lightingRoot.transform);
            Volume volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Hub_PostProcessing_Profile";

            // Bloom
            Bloom bloom;
            if (profile.TryGet(out bloom) == false) bloom = profile.Add<Bloom>();
            bloom.threshold.value = 0.85f;
            bloom.intensity.value = 1.35f;
            bloom.scatter.value = 0.7f;
            bloom.active = true;

            // Tonemapping
            Tonemapping tonemapping;
            if (profile.TryGet(out tonemapping) == false) tonemapping = profile.Add<Tonemapping>();
            tonemapping.mode.value = TonemappingMode.ACES;
            tonemapping.active = true;

            // Vignette
            Vignette vignette;
            if (profile.TryGet(out vignette) == false) vignette = profile.Add<Vignette>();
            vignette.intensity.value = 0.28f;
            vignette.smoothness.value = 0.45f;
            vignette.active = true;

            volume.profile = profile;

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[FirstHandHubBuilder] Scene saved to {ScenePath}");

            // Set in Build Settings
            AddSceneToBuildSettings(ScenePath);

            // Open scene in editor
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("[FirstHandHubBuilder] Scene build complete and loaded in Editor!");
        }

        private static GameObject InstantiatePrefabSafe(string path, Vector3 pos, Quaternion rot, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[FirstHandHubBuilder] Prefab not found at path: {path}");
                return null;
            }
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = pos;
            go.transform.rotation = rot;
            return go;
        }

        private static GameObject CreateRailing(Transform parent, Vector3 center, float length)
        {
            GameObject railingRoot = new GameObject("Catwalk_Railing");
            railingRoot.transform.SetParent(parent);
            railingRoot.transform.position = center;

            // Horizontal top bar
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bar.name = "Top_Bar";
            bar.transform.SetParent(railingRoot.transform);
            bar.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            bar.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            bar.transform.localScale = new Vector3(0.04f, length * 0.5f, 0.04f);
            SetMaterialColor(bar, new Color(0.2f, 0.22f, 0.25f), 0.9f, 0.1f);

            // Vertical posts
            for (float z = -length * 0.45f; z <= length * 0.45f; z += length * 0.45f)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Railing_Post";
                post.transform.SetParent(railingRoot.transform);
                post.transform.localPosition = new Vector3(0f, 0f, z);
                post.transform.localScale = new Vector3(0.04f, 0.25f, 0.04f);
                SetMaterialColor(post, new Color(0.2f, 0.22f, 0.25f), 0.9f, 0.1f);
            }

            return railingRoot;
        }

        private static void CreateOverheadCables(Transform parent)
        {
            GameObject cables = new GameObject("Overhead_Blue_Cables");
            cables.transform.SetParent(parent);

            Material blueGlowMat = CreateEmissiveMaterial("Mat_CableGlow", new Color(0f, 0.6f, 1f) * 1.5f);

            for (int i = 0; i < 3; i++)
            {
                GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cable.transform.SetParent(cables.transform);
                cable.transform.localPosition = new Vector3(-2.8f + (i * 0.15f), 2.6f - (i * 0.08f), 1.5f);
                cable.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
                cable.transform.localScale = new Vector3(0.025f, 1.8f, 0.025f);
                cable.GetComponent<Renderer>().sharedMaterial = blueGlowMat;
            }
        }

        private static void CreateOrbConduit(Transform orbTransform, Vector3 basePos)
        {
            GameObject conduit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            conduit.name = "Orb_Ribbed_Conduit";
            conduit.transform.SetParent(orbTransform.parent);

            Vector3 orbPos = orbTransform.position;
            Vector3 mid = (orbPos + basePos) * 0.5f;
            conduit.transform.position = mid;
            conduit.transform.up = (orbPos - basePos).normalized;
            float dist = Vector3.Distance(orbPos, basePos);
            conduit.transform.localScale = new Vector3(0.022f, dist * 0.5f, 0.022f);

            Collider col = conduit.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            SetMaterialColor(conduit, new Color(0.1f, 0.12f, 0.15f), 0.7f, 0.3f);
        }

        private static void BuildHologramUIGeometry(Transform canvasRoot, HologramUIController uiCtrl)
        {
            // Translucent purple background panel matching reference photo
            GameObject bgGo = new GameObject("Cyber_Grid_Panel");
            bgGo.transform.SetParent(canvasRoot);
            RectTransform bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(700f, 360f);
            bgRect.anchoredPosition = Vector2.zero;
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.05f, 0.28f, 0.82f);

            // Header Container
            GameObject headerGo = new GameObject("Header_Bar");
            headerGo.transform.SetParent(bgGo.transform);
            RectTransform headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(660f, 40f);
            headerRect.anchoredPosition = new Vector2(0f, 140f);

            // Title: PLACE THE ORB
            GameObject titleGo = new GameObject("Title_Text");
            titleGo.transform.SetParent(headerGo.transform);
            RectTransform titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(350f, 40f);
            titleRect.anchoredPosition = new Vector2(-140f, 0f);
            TextMeshProUGUI titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "PLACE THE ORB";
            titleTmp.fontSize = 28;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Left;

            // Counter: 0 / 25
            GameObject counterGo = new GameObject("Counter_Text");
            counterGo.transform.SetParent(headerGo.transform);
            RectTransform countRect = counterGo.AddComponent<RectTransform>();
            countRect.sizeDelta = new Vector2(180f, 40f);
            countRect.anchoredPosition = new Vector2(230f, 0f);
            TextMeshProUGUI countTmp = counterGo.AddComponent<TextMeshProUGUI>();
            countTmp.text = "0 / 25";
            countTmp.fontSize = 24;
            countTmp.fontStyle = FontStyles.Bold;
            countTmp.color = new Color(0.8f, 0.7f, 1.0f);
            countTmp.alignment = TextAlignmentOptions.Right;

            // Tech Cards Container
            GameObject cardsGo = new GameObject("Upgrade_Cards_Row");
            cardsGo.transform.SetParent(bgGo.transform);
            RectTransform cardsRect = cardsGo.AddComponent<RectTransform>();
            cardsRect.sizeDelta = new Vector2(660f, 130f);
            cardsRect.anchoredPosition = new Vector2(0f, 50f);

            // Card 1: Power Glove
            GameObject gloveCard = CreateCard(cardsGo.transform, new Vector2(-180f, 0f), "Power Glove", "ðŸ”’ LOCKED");
            // Card 2: Locomotion
            GameObject locoCard = CreateCard(cardsGo.transform, new Vector2(180f, 0f), "Locomotion", "ðŸ”’ LOCKED");

            // Center Tooltip Banner
            GameObject tooltipGo = new GameObject("Center_Tooltip_Banner");
            tooltipGo.transform.SetParent(bgGo.transform);
            RectTransform tooltipRect = tooltipGo.AddComponent<RectTransform>();
            tooltipRect.sizeDelta = new Vector2(400f, 54f);
            tooltipRect.anchoredPosition = new Vector2(0f, -40f);
            Image tooltipBg = tooltipGo.AddComponent<Image>();
            tooltipBg.color = new Color(0.12f, 0.28f, 0.65f, 0.85f);

            GameObject tipTextGo = new GameObject("Tooltip_Text");
            tipTextGo.transform.SetParent(tooltipGo.transform);
            RectTransform tipTextRect = tipTextGo.AddComponent<RectTransform>();
            tipTextRect.sizeDelta = new Vector2(380f, 48f);
            tipTextRect.anchoredPosition = Vector2.zero;
            TextMeshProUGUI tipTmp = tipTextGo.AddComponent<TextMeshProUGUI>();
            tipTmp.text = "Place an orb into the console to begin";
            tipTmp.fontSize = 17;
            tipTmp.color = Color.white;
            tipTmp.alignment = TextAlignmentOptions.Center;

            // Reticle Ring at bottom center
            GameObject reticleGo = new GameObject("Reticle_Ring");
            reticleGo.transform.SetParent(bgGo.transform);
            RectTransform reticleRect = reticleGo.AddComponent<RectTransform>();
            reticleRect.sizeDelta = new Vector2(70f, 70f);
            reticleRect.anchoredPosition = new Vector2(0f, -120f);
            Image reticleImg = reticleGo.AddComponent<Image>();
            reticleImg.color = new Color(1f, 1f, 1f, 0.85f);

            // Link references into uiCtrl
            SetPrivateField(uiCtrl, "headerTitleText", titleTmp);
            SetPrivateField(uiCtrl, "headerCountText", countTmp);
            SetPrivateField(uiCtrl, "tooltipText", tipTmp);
            SetPrivateField(uiCtrl, "tooltipBgImage", tooltipBg);
            SetPrivateField(uiCtrl, "reticleRingTransform", reticleRect);

            var gloveText = gloveCard.GetComponentInChildren<TextMeshProUGUI>();
            var locoText = locoCard.GetComponentInChildren<TextMeshProUGUI>();
            SetPrivateField(uiCtrl, "powerGloveStatusText", gloveText);
            SetPrivateField(uiCtrl, "locomotionStatusText", locoText);
        }

        private static GameObject CreateCard(Transform parent, Vector2 pos, string title, string status)
        {
            GameObject cardGo = new GameObject("Card_" + title.Replace(" ", ""));
            cardGo.transform.SetParent(parent);
            RectTransform cardRect = cardGo.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(240f, 110f);
            cardRect.anchoredPosition = pos;

            Image bg = cardGo.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.12f, 0.45f, 0.75f);

            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(cardGo.transform);
            RectTransform tRect = titleGo.AddComponent<RectTransform>();
            tRect.sizeDelta = new Vector2(220f, 30f);
            tRect.anchoredPosition = new Vector2(0f, 25f);
            TextMeshProUGUI tTmp = titleGo.AddComponent<TextMeshProUGUI>();
            tTmp.text = title;
            tTmp.fontSize = 20;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.color = Color.white;
            tTmp.alignment = TextAlignmentOptions.Center;

            GameObject statusGo = new GameObject("Status_Badge");
            statusGo.transform.SetParent(cardGo.transform);
            RectTransform sRect = statusGo.AddComponent<RectTransform>();
            sRect.sizeDelta = new Vector2(160f, 28f);
            sRect.anchoredPosition = new Vector2(0f, -20f);
            Image sBg = statusGo.AddComponent<Image>();
            sBg.color = new Color(0.5f, 0.15f, 0.6f, 0.9f);

            GameObject statusTextGo = new GameObject("Status_Text");
            statusTextGo.transform.SetParent(statusGo.transform);
            RectTransform stRect = statusTextGo.AddComponent<RectTransform>();
            stRect.sizeDelta = new Vector2(150f, 26f);
            stRect.anchoredPosition = Vector2.zero;
            TextMeshProUGUI sTmp = statusTextGo.AddComponent<TextMeshProUGUI>();
            sTmp.text = status;
            sTmp.fontSize = 15;
            sTmp.color = Color.white;
            sTmp.alignment = TextAlignmentOptions.Center;

            return cardGo;
        }

        private static void SetMaterialColor(GameObject go, Color col, float metallic = 0.5f, float roughness = 0.5f)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            Material mat = new Material(litShader);
            mat.color = col;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 1f - roughness);
            rend.sharedMaterial = mat;
        }

        private static Material CreateEmissiveMaterial(string name, Color emission)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            Material mat = new Material(litShader);
            mat.name = name;
            mat.color = Color.white;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            return mat;
        }

        private static Material CreateGlassMaterial(string name, Color tint)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            Material mat = new Material(litShader);
            mat.name = name;
            mat.color = tint;
            mat.SetFloat("_Surface", 1); // Transparent in URP
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
            for (int i = 0; i < scenes.Length; i++)
            {
                newScenes[i + 1] = scenes[i];
            }
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
