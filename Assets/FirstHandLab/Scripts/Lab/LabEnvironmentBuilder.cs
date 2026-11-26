using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Procedural 3D Environment Builder for the First Hand Laboratory Experience.
    /// Constructs the curved titanium observation pod, tactile console workbench,
    /// power core sockets, mechanical levers, push buttons, grabbable energy cores,
    /// and the panoramic alien vista outside the observation window.
    /// </summary>
    public static class LabEnvironmentBuilder
    {
        #region Material & Shader Generation

        private static Shader urpLitShader;
        private static Shader urpUnlitShader;

        private static Shader GetLitShader()
        {
            if (urpLitShader == null)
            {
                urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLitShader == null) urpLitShader = Shader.Find("Standard");
            }
            return urpLitShader;
        }

        private static Shader GetUnlitShader()
        {
            if (urpUnlitShader == null)
            {
                urpUnlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (urpUnlitShader == null) urpUnlitShader = Shader.Find("Unlit/Color");
            }
            return urpUnlitShader;
        }

        public static Material CreateSciFiMetalMaterial(Color baseColor, float metallic = 0.85f, float smoothness = 0.75f)
        {
            Material mat = new Material(GetLitShader());
            mat.name = "M_SciFiMetal_" + ColorUtility.ToHtmlStringRGB(baseColor);
            mat.color = baseColor;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        public static Material CreateEmissiveMaterial(Color emissionColor, float intensity = 3.5f)
        {
            Material mat = new Material(GetLitShader());
            mat.name = "M_Emissive_" + ColorUtility.ToHtmlStringRGB(emissionColor);
            mat.color = Color.Lerp(emissionColor, Color.white, 0.25f);
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emissionColor * intensity);
            }
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.2f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.9f);
            return mat;
        }

        public static Material CreateGlassMaterial(Color tint, float alpha = 0.25f)
        {
            Material mat = new Material(GetLitShader());
            mat.name = "M_SciFiGlass";
            Color c = tint;
            c.a = alpha;
            mat.color = c;

            // Configure transparent blend mode for URP
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1); // Transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0); // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
            return mat;
        }

        public static Material CreateHologramMaterial(Color holoColor)
        {
            Material mat = new Material(GetUnlitShader());
            mat.name = "M_Hologram";
            Color c = holoColor;
            c.a = 0.65f;
            mat.color = c;

            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive glow
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 100;
            return mat;
        }

        #endregion

        #region Complete Lab Generation

        public static GameObject BuildLabHierarchy()
        {
            GameObject labRoot = new GameObject("FirstHand_ObservationLab");

            // 1. Core Materials
            Material floorMat = CreateSciFiMetalMaterial(new Color(0.12f, 0.14f, 0.17f), 0.7f, 0.6f);
            Material wallMat = CreateSciFiMetalMaterial(new Color(0.18f, 0.21f, 0.26f), 0.85f, 0.7f);
            Material trimMat = CreateSciFiMetalMaterial(new Color(0.08f, 0.09f, 0.11f), 0.9f, 0.8f);
            Material glassMat = CreateGlassMaterial(new Color(0.6f, 0.85f, 1.0f), 0.2f);
            Material cyanEmissive = CreateEmissiveMaterial(new Color(0.1f, 0.8f, 1.0f), 3.0f);
            Material amberEmissive = CreateEmissiveMaterial(new Color(1.0f, 0.65f, 0.1f), 3.0f);
            Material magentaEmissive = CreateEmissiveMaterial(new Color(0.95f, 0.15f, 0.85f), 3.2f);
            Material redEmissive = CreateEmissiveMaterial(new Color(1.0f, 0.1f, 0.1f), 3.5f);

            // 2. Room Shell & Architecture
            GameObject roomGo = new GameObject("Architecture");
            roomGo.transform.SetParent(labRoot.transform, false);
            BuildPodArchitecture(roomGo, floorMat, wallMat, trimMat, glassMat, cyanEmissive);

            // 3. Blast Shield Shutter
            GameObject blastShieldGo = BuildBlastShield(roomGo, trimMat, cyanEmissive);

            // 4. Central Interactive Workbench Console
            GameObject workbenchGo = new GameObject("WorkbenchConsole");
            workbenchGo.transform.SetParent(labRoot.transform, false);
            workbenchGo.transform.position = new Vector3(0f, 0f, 3.4f);

            List<PowerCoreSocket> sockets = new List<PowerCoreSocket>();
            PhysicalLeverSlider blastLever = null;
            PhysicalPushButton holoButton = null;
            PhysicalPushButton emergencyButton = null;
            GameObject hologramDisplay = null;
            Transform hologramCore = null;

            BuildTactileWorkbench(workbenchGo, wallMat, trimMat, cyanEmissive, amberEmissive, magentaEmissive, redEmissive,
                out sockets, out blastLever, out holoButton, out emergencyButton, out hologramDisplay, out hologramCore);

            // 5. Grabbable Energy Cores
            GameObject coresRack = new GameObject("PowerCores");
            coresRack.transform.SetParent(labRoot.transform, false);
            BuildGrabbableCores(coresRack, cyanEmissive, amberEmissive, magentaEmissive);

            // 6. Panoramic Alien Vista Outside Window
            GameObject vistaGo = new GameObject("AlienVista");
            vistaGo.transform.SetParent(labRoot.transform, false);
            BuildAlienVista(vistaGo, magentaEmissive, cyanEmissive, amberEmissive);

            // 7. Lighting & Atmosphere
            List<Light> interiorLights = new List<Light>();
            List<Light> emergencyLights = new List<Light>();
            BuildLabLighting(labRoot, out interiorLights, out emergencyLights);

            // 8. Lab System Coordinator
            LabSystemManager manager = labRoot.AddComponent<LabSystemManager>();
            manager.RegisterSockets(
                sockets.Count > 0 ? sockets[0] : null,
                sockets.Count > 1 ? sockets[1] : null,
                sockets.Count > 2 ? sockets[2] : null
            );
            manager.RegisterShield(blastShieldGo.transform, new Vector3(0f, 0f, 6.4f), new Vector3(0f, 6.0f, 6.4f));
            manager.RegisterHologram(hologramDisplay, hologramCore);
            manager.RegisterLights(interiorLights.ToArray(), emergencyLights.ToArray());

            // Wire up workbench controls to manager
            if (blastLever != null)
            {
                blastLever.OnValueChanged.AddListener(manager.SetBlastShieldLever);
            }
            if (holoButton != null)
            {
                holoButton.OnPressed.AddListener(manager.ToggleHologram);
            }
            if (emergencyButton != null)
            {
                emergencyButton.OnPressed.AddListener(manager.ToggleEmergencyMode);
            }

            return labRoot;
        }

        #endregion

        #region Room Architecture

        private static void BuildPodArchitecture(GameObject parent, Material floorMat, Material wallMat, Material trimMat, Material glassMat, Material conduitMat)
        {
            // Floor (14m x 16m)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "LabFloor";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(14f, 0.2f, 16f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Emissive Floor Energy Conduits leading to the workbench
            for (int i = -2; i <= 2; i += 2)
            {
                GameObject conduit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                conduit.name = $"FloorConduit_{i}";
                conduit.transform.SetParent(parent.transform, false);
                conduit.transform.position = new Vector3(i * 1.4f, 0.005f, 0.5f);
                conduit.transform.localScale = new Vector3(0.08f, 0.015f, 6f);
                conduit.GetComponent<Renderer>().sharedMaterial = conduitMat;
                Collider col = conduit.GetComponent<Collider>();
                if (col != null) UnityEngine.Object.DestroyImmediate(col);
            }

            // Ceiling (14m x 16m, height 5.2m)
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "LabCeiling";
            ceiling.transform.SetParent(parent.transform, false);
            ceiling.transform.position = new Vector3(0f, 5.2f, 0f);
            ceiling.transform.localScale = new Vector3(14f, 0.2f, 16f);
            ceiling.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Back Wall (Z = -8m)
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "BackWall";
            backWall.transform.SetParent(parent.transform, false);
            backWall.transform.position = new Vector3(0f, 2.5f, -8f);
            backWall.transform.localScale = new Vector3(14f, 5.2f, 0.4f);
            backWall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Left Wall (X = -7m)
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWall";
            leftWall.transform.SetParent(parent.transform, false);
            leftWall.transform.position = new Vector3(-7f, 2.5f, 0f);
            leftWall.transform.localScale = new Vector3(0.4f, 5.2f, 16f);
            leftWall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Right Wall (X = +7m)
            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWall";
            rightWall.transform.SetParent(parent.transform, false);
            rightWall.transform.position = new Vector3(7f, 2.5f, 0f);
            rightWall.transform.localScale = new Vector3(0.4f, 5.2f, 16f);
            rightWall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Bulkhead Ribbed Arches across the ceiling
            for (float z = -6f; z <= 4f; z += 3.3f)
            {
                GameObject arch = new GameObject($"BulkheadArch_Z_{z:F1}");
                arch.transform.SetParent(parent.transform, false);
                arch.transform.position = new Vector3(0f, 0f, z);

                // Top arch beam
                GameObject topBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                topBeam.name = "TopBeam";
                topBeam.transform.SetParent(arch.transform, false);
                topBeam.transform.position = new Vector3(0f, 5.0f, z);
                topBeam.transform.localScale = new Vector3(13.8f, 0.35f, 0.45f);
                topBeam.GetComponent<Renderer>().sharedMaterial = trimMat;

                // Left pillar
                GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftPillar.name = "LeftPillar";
                leftPillar.transform.SetParent(arch.transform, false);
                leftPillar.transform.position = new Vector3(-6.75f, 2.5f, z);
                leftPillar.transform.localScale = new Vector3(0.45f, 5.0f, 0.45f);
                leftPillar.GetComponent<Renderer>().sharedMaterial = trimMat;

                // Right pillar
                GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightPillar.name = "RightPillar";
                rightPillar.transform.SetParent(arch.transform, false);
                rightPillar.transform.position = new Vector3(6.75f, 2.5f, z);
                rightPillar.transform.localScale = new Vector3(0.45f, 5.0f, 0.45f);
                rightPillar.GetComponent<Renderer>().sharedMaterial = trimMat;

                // Inset cyan light strip
                GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strip.name = "ArchLightStrip";
                strip.transform.SetParent(arch.transform, false);
                strip.transform.position = new Vector3(0f, 4.88f, z);
                strip.transform.localScale = new Vector3(10f, 0.04f, 0.12f);
                strip.GetComponent<Renderer>().sharedMaterial = conduitMat;
                Collider sc = strip.GetComponent<Collider>();
                if (sc != null) UnityEngine.Object.DestroyImmediate(sc);
            }

            // Panoramic Observation Window (Z = 6.2m)
            // Lower sill wall
            GameObject windowSill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windowSill.name = "WindowSill";
            windowSill.transform.SetParent(parent.transform, false);
            windowSill.transform.position = new Vector3(0f, 0.45f, 6.2f);
            windowSill.transform.localScale = new Vector3(14f, 0.9f, 0.5f);
            windowSill.GetComponent<Renderer>().sharedMaterial = trimMat;

            // Upper header wall
            GameObject windowHeader = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windowHeader.name = "WindowHeader";
            windowHeader.transform.SetParent(parent.transform, false);
            windowHeader.transform.position = new Vector3(0f, 4.6f, 6.2f);
            windowHeader.transform.localScale = new Vector3(14f, 1.2f, 0.5f);
            windowHeader.GetComponent<Renderer>().sharedMaterial = trimMat;

            // Glass Panes (Curved panoramic 3-segment window)
            // Center pane
            GameObject centerGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerGlass.name = "ObservationWindow_Center";
            centerGlass.transform.SetParent(parent.transform, false);
            centerGlass.transform.position = new Vector3(0f, 2.5f, 6.2f);
            centerGlass.transform.localScale = new Vector3(5.5f, 3.2f, 0.08f);
            centerGlass.GetComponent<Renderer>().sharedMaterial = glassMat;

            // Left angled pane
            GameObject leftGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftGlass.name = "ObservationWindow_Left";
            leftGlass.transform.SetParent(parent.transform, false);
            leftGlass.transform.position = new Vector3(-3.8f, 2.5f, 5.9f);
            leftGlass.transform.rotation = Quaternion.Euler(0f, -22f, 0f);
            leftGlass.transform.localScale = new Vector3(3.5f, 3.2f, 0.08f);
            leftGlass.GetComponent<Renderer>().sharedMaterial = glassMat;

            // Right angled pane
            GameObject rightGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightGlass.name = "ObservationWindow_Right";
            rightGlass.transform.SetParent(parent.transform, false);
            rightGlass.transform.position = new Vector3(3.8f, 2.5f, 5.9f);
            rightGlass.transform.rotation = Quaternion.Euler(0f, 22f, 0f);
            rightGlass.transform.localScale = new Vector3(3.5f, 3.2f, 0.08f);
            rightGlass.GetComponent<Renderer>().sharedMaterial = glassMat;
        }

        private static GameObject BuildBlastShield(GameObject parent, Material shieldMat, Material emissiveMat)
        {
            GameObject shieldRoot = new GameObject("BlastShieldMechanism");
            shieldRoot.transform.SetParent(parent.transform, false);
            shieldRoot.transform.position = new Vector3(0f, 0f, 6.4f);

            // Shutter plate that covers the 14m x 4m window opening
            GameObject shutterPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shutterPlate.name = "BlastShieldPlate";
            shutterPlate.transform.SetParent(shieldRoot.transform, false);
            shutterPlate.transform.position = new Vector3(0f, 2.5f, 0f);
            shutterPlate.transform.localScale = new Vector3(13.5f, 3.5f, 0.25f);
            shutterPlate.GetComponent<Renderer>().sharedMaterial = shieldMat;

            // Industrial warning hazard stripes along the bottom edge
            GameObject hazardStripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazardStripe.name = "HazardStripe";
            hazardStripe.transform.SetParent(shieldRoot.transform, false);
            hazardStripe.transform.position = new Vector3(0f, 0.85f, -0.14f);
            hazardStripe.transform.localScale = new Vector3(13.2f, 0.15f, 0.04f);
            hazardStripe.GetComponent<Renderer>().sharedMaterial = emissiveMat;

            return shieldRoot;
        }

        #endregion

        #region Tactile Interactive Workbench

        private static void BuildTactileWorkbench(
            GameObject workbenchRoot,
            Material tableMat,
            Material trimMat,
            Material cyanMat,
            Material amberMat,
            Material magentaMat,
            Material redMat,
            out List<PowerCoreSocket> sockets,
            out PhysicalLeverSlider blastLever,
            out PhysicalPushButton holoButton,
            out PhysicalPushButton emergencyButton,
            out GameObject hologramDisplay,
            out Transform hologramRotatingCore)
        {
            sockets = new List<PowerCoreSocket>();

            // 1. Workbench Desk Base (Width 3.4m, Depth 1.2m, Height 0.88m)
            GameObject deskBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deskBase.name = "DeskBase";
            deskBase.transform.SetParent(workbenchRoot.transform, false);
            deskBase.transform.position = new Vector3(0f, 0.44f, 0f);
            deskBase.transform.localScale = new Vector3(3.2f, 0.88f, 1.15f);
            deskBase.GetComponent<Renderer>().sharedMaterial = tableMat;

            // Angled top console face
            GameObject consolePanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            consolePanel.name = "ConsolePanel";
            consolePanel.transform.SetParent(workbenchRoot.transform, false);
            consolePanel.transform.position = new Vector3(0f, 0.92f, 0.2f);
            consolePanel.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            consolePanel.transform.localScale = new Vector3(3.1f, 0.08f, 0.65f);
            consolePanel.GetComponent<Renderer>().sharedMaterial = trimMat;

            // 2. Central Gauntlet / Hologram Assembly Cradle
            GameObject cradle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cradle.name = "AssemblyCradle";
            cradle.transform.SetParent(workbenchRoot.transform, false);
            cradle.transform.position = new Vector3(0f, 0.90f, -0.1f);
            cradle.transform.localScale = new Vector3(0.65f, 0.04f, 0.65f);
            cradle.GetComponent<Renderer>().sharedMaterial = trimMat;

            // Glowing emitter ring on cradle
            GameObject emitterRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            emitterRing.name = "EmitterRing";
            emitterRing.transform.SetParent(cradle.transform, false);
            emitterRing.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            emitterRing.transform.localScale = new Vector3(0.85f, 0.1f, 0.85f);
            emitterRing.GetComponent<Renderer>().sharedMaterial = cyanMat;

            // Hologram Display (Floating rotating planet/wireframe)
            hologramDisplay = new GameObject("HolographicDisplay");
            hologramDisplay.transform.SetParent(workbenchRoot.transform, false);
            hologramDisplay.transform.position = new Vector3(0f, 1.35f, -0.1f);

            Material holoMat = CreateHologramMaterial(new Color(0.2f, 0.85f, 1.0f));

            GameObject holoGlobe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            holoGlobe.name = "HologramPlanet";
            holoGlobe.transform.SetParent(hologramDisplay.transform, false);
            holoGlobe.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            holoGlobe.GetComponent<Renderer>().sharedMaterial = holoMat;
            Collider gc = holoGlobe.GetComponent<Collider>();
            if (gc != null) UnityEngine.Object.DestroyImmediate(gc);

            // Planetary Ring
            GameObject holoRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holoRing.name = "HologramRing";
            holoRing.transform.SetParent(hologramDisplay.transform, false);
            holoRing.transform.localScale = new Vector3(0.65f, 0.005f, 0.65f);
            holoRing.transform.rotation = Quaternion.Euler(25f, 0f, 15f);
            holoRing.GetComponent<Renderer>().sharedMaterial = holoMat;
            Collider rc = holoRing.GetComponent<Collider>();
            if (rc != null) UnityEngine.Object.DestroyImmediate(rc);

            hologramRotatingCore = hologramDisplay.transform;

            // 3. Three Power Core Docking Sockets (Lower Console Tier)
            string[] socketIds = { "AlphaCore", "BetaCore", "GammaCore" };
            string[] socketNames = { "PRIMARY REACTOR", "STASIS FIELD", "QUANTUM SCANNER" };
            Material[] socketColors = { cyanMat, amberMat, magentaMat };
            float[] socketX = { -0.75f, 0f, 0.75f };

            for (int i = 0; i < 3; i++)
            {
                GameObject socketObj = new GameObject($"PowerSocket_{i + 1}_{socketIds[i]}");
                socketObj.transform.SetParent(workbenchRoot.transform, false);
                socketObj.transform.position = new Vector3(socketX[i], 0.90f, -0.42f);

                // Socket Housing Cylinder
                GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                housing.name = "Housing";
                housing.transform.SetParent(socketObj.transform, false);
                housing.transform.localScale = new Vector3(0.24f, 0.05f, 0.24f);
                housing.GetComponent<Renderer>().sharedMaterial = trimMat;

                // Status Ring LED
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "StatusRingLED";
                ring.transform.SetParent(socketObj.transform, false);
                ring.transform.localPosition = new Vector3(0f, 0.03f, 0f);
                ring.transform.localScale = new Vector3(0.28f, 0.015f, 0.28f);
                ring.GetComponent<Renderer>().sharedMaterial = socketColors[i];

                // Dock Anchor
                GameObject anchor = new GameObject("DockAnchor");
                anchor.transform.SetParent(socketObj.transform, false);
                anchor.transform.localPosition = new Vector3(0f, 0.12f, 0f);

                // Point Light for socket
                GameObject lightGo = new GameObject("SocketLight");
                lightGo.transform.SetParent(socketObj.transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                Light sl = lightGo.AddComponent<Light>();
                sl.type = LightType.Point;
                sl.range = 1.2f;
                sl.intensity = 0.5f;

                // PowerCoreSocket script & Trigger Collider
                SphereCollider trigger = socketObj.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.25f;

                PowerCoreSocket socketComp = socketObj.AddComponent<PowerCoreSocket>();
                socketComp.Initialize(anchor.transform, socketIds[i], socketNames[i], ring.GetComponent<Renderer>(), sl);
                sockets.Add(socketComp);
            }

            // 4. Right Controls Tier: Mechanical Levers
            // Blast Shield Lever (Rotational Hinge)
            GameObject leverBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leverBase.name = "BlastShieldLeverBase";
            leverBase.transform.SetParent(workbenchRoot.transform, false);
            leverBase.transform.position = new Vector3(1.18f, 0.97f, 0.18f);
            leverBase.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            leverBase.transform.localScale = new Vector3(0.18f, 0.06f, 0.35f);
            leverBase.GetComponent<Renderer>().sharedMaterial = trimMat;

            GameObject leverHandlePivot = new GameObject("LeverHandlePivot");
            leverHandlePivot.transform.SetParent(leverBase.transform, false);
            leverHandlePivot.transform.localPosition = new Vector3(0f, 0.03f, 0f);

            GameObject leverArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leverArm.name = "Arm";
            leverArm.transform.SetParent(leverHandlePivot.transform, false);
            leverArm.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            leverArm.transform.localScale = new Vector3(0.035f, 0.12f, 0.035f);
            leverArm.GetComponent<Renderer>().sharedMaterial = trimMat;

            GameObject leverGrip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leverGrip.name = "Grip";
            leverGrip.transform.SetParent(leverHandlePivot.transform, false);
            leverGrip.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            leverGrip.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            leverGrip.GetComponent<Renderer>().sharedMaterial = amberMat;

            BoxCollider leverCol = leverBase.AddComponent<BoxCollider>();
            leverCol.size = new Vector3(0.2f, 0.3f, 0.35f);
            leverCol.center = new Vector3(0f, 0.15f, 0f);
            blastLever = leverBase.AddComponent<PhysicalLeverSlider>();
            blastLever.Initialize(leverHandlePivot.transform, PhysicalLeverSlider.MotionType.RotationalHinge, Vector3.right, -35f, 35f, 8);

            // Throttle Slider (Linear)
            GameObject sliderBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sliderBase.name = "PowerThrottleBase";
            sliderBase.transform.SetParent(workbenchRoot.transform, false);
            sliderBase.transform.position = new Vector3(1.38f, 0.97f, 0.18f);
            sliderBase.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            sliderBase.transform.localScale = new Vector3(0.12f, 0.05f, 0.35f);
            sliderBase.GetComponent<Renderer>().sharedMaterial = trimMat;

            GameObject sliderKnob = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sliderKnob.name = "SliderKnob";
            sliderKnob.transform.SetParent(sliderBase.transform, false);
            sliderKnob.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            sliderKnob.transform.localScale = new Vector3(0.08f, 0.04f, 0.08f);
            sliderKnob.GetComponent<Renderer>().sharedMaterial = cyanMat;

            // 5. Left Controls Tier: Bank of Tactile Push Buttons
            // Button 1: Hologram Projector Toggle
            holoButton = CreatePushButtonOnConsole(workbenchRoot.transform, new Vector3(-1.18f, 0.97f, 0.24f),
                "Btn_HoloToggle", cyanMat, trimMat, true, true);

            // Button 2: Emergency Klaxon Toggle
            emergencyButton = CreatePushButtonOnConsole(workbenchRoot.transform, new Vector3(-1.18f, 0.97f, 0.08f),
                "Btn_EmergencyKlaxon", redMat, trimMat, true, false);

            // Button 3: Coolant Purge (Momentary)
            CreatePushButtonOnConsole(workbenchRoot.transform, new Vector3(-1.38f, 0.97f, 0.24f),
                "Btn_CoolantPurge", amberMat, trimMat, false, false);

            // Button 4: Diagnostics (Momentary)
            CreatePushButtonOnConsole(workbenchRoot.transform, new Vector3(-1.38f, 0.97f, 0.08f),
                "Btn_Diagnostics", cyanMat, trimMat, false, false);
        }

        private static PhysicalPushButton CreatePushButtonOnConsole(
            Transform parent, Vector3 pos, string name, Material ledMat, Material bezelMat, bool isToggle, bool initialToggle)
        {
            GameObject btnRoot = new GameObject(name);
            btnRoot.transform.SetParent(parent, false);
            btnRoot.transform.position = pos;
            btnRoot.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            // Bezel base
            GameObject bezel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bezel.name = "Bezel";
            bezel.transform.SetParent(btnRoot.transform, false);
            bezel.transform.localScale = new Vector3(0.09f, 0.02f, 0.09f);
            bezel.GetComponent<Renderer>().sharedMaterial = bezelMat;

            // Moving button cap
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "PlungerCap";
            cap.transform.SetParent(btnRoot.transform, false);
            cap.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            cap.transform.localScale = new Vector3(0.07f, 0.025f, 0.07f);
            cap.GetComponent<Renderer>().sharedMaterial = ledMat;

            BoxCollider col = btnRoot.AddComponent<BoxCollider>();
            col.size = new Vector3(0.09f, 0.06f, 0.09f);
            col.center = new Vector3(0f, 0.02f, 0f);

            PhysicalPushButton btnComp = btnRoot.AddComponent<PhysicalPushButton>();
            btnComp.Initialize(cap.transform, Vector3.down, 0.015f, cap.GetComponent<Renderer>(), ledMat.color, Color.white, isToggle, initialToggle);
            return btnComp;
        }

        #endregion

        #region Grabbable Energy Cores

        private static void BuildGrabbableCores(GameObject parent, Material cyanMat, Material amberMat, Material magentaMat)
        {
            // Core 1: Alpha Core (Neon Cyan) - Placed on the console table
            CreateEnergyCore(parent.transform, new Vector3(-0.85f, 0.98f, 3.1f), "AlphaCore", "Cyan Plasma Core",
                new Color(0.1f, 0.8f, 1.0f), cyanMat);

            // Core 2: Beta Core (Solar Amber) - Placed in the center
            CreateEnergyCore(parent.transform, new Vector3(0f, 0.98f, 3.0f), "BetaCore", "Solar Fusion Core",
                new Color(1.0f, 0.65f, 0.1f), amberMat);

            // Core 3: Gamma Core (Neon Magenta) - Placed on the right
            CreateEnergyCore(parent.transform, new Vector3(0.85f, 0.98f, 3.1f), "GammaCore", "Graviton Warp Core",
                new Color(0.95f, 0.15f, 0.85f), magentaMat);
        }

        private static GameObject CreateEnergyCore(
            Transform parent, Vector3 pos, string id, string displayName, Color glowColor, Material mat)
        {
            GameObject coreGo = new GameObject($"PowerCore_{id}");
            coreGo.transform.SetParent(parent, false);
            coreGo.transform.position = pos;

            // Core sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "CoreSphere";
            sphere.transform.SetParent(coreGo.transform, false);
            sphere.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
            sphere.GetComponent<Renderer>().sharedMaterial = mat;
            Collider childCol = sphere.GetComponent<Collider>();
            if (childCol != null) UnityEngine.Object.DestroyImmediate(childCol);

            // Outer protective containment rings
            GameObject ring1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring1.name = "ContainmentRing1";
            ring1.transform.SetParent(coreGo.transform, false);
            ring1.transform.localScale = new Vector3(0.17f, 0.015f, 0.17f);
            ring1.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
            ring1.GetComponent<Renderer>().sharedMaterial = mat;
            Collider rc1 = ring1.GetComponent<Collider>();
            if (rc1 != null) UnityEngine.Object.DestroyImmediate(rc1);

            // Core Point Light
            GameObject lightGo = new GameObject("CoreLight");
            lightGo.transform.SetParent(coreGo.transform, false);
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = glowColor;
            l.range = 2.0f;
            l.intensity = 1.8f;

            // Core Collider and Rigidbody on root object
            SphereCollider sc = coreGo.AddComponent<SphereCollider>();
            sc.radius = 0.09f;

            Rigidbody rb = coreGo.AddComponent<Rigidbody>();
            rb.mass = 2.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            GrabbablePowerCore coreComp = coreGo.AddComponent<GrabbablePowerCore>();
            coreComp.Initialize(id, displayName, glowColor, sphere.GetComponent<Renderer>(), l);
            return coreGo;
        }

        #endregion

        #region Panoramic Alien Vista Outside Window

        private static void BuildAlienVista(GameObject parent, Material magentaMat, Material cyanMat, Material amberMat)
        {
            // 1. Giant Ringed Planet in the Sky (suspended at (12, 28, 85))
            GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "CelestialPlanet";
            planet.transform.SetParent(parent.transform, false);
            planet.transform.position = new Vector3(14f, 26f, 85f);
            planet.transform.localScale = new Vector3(28f, 28f, 28f);

            Material planetMat = CreateSciFiMetalMaterial(new Color(0.9f, 0.45f, 0.65f), 0.1f, 0.85f);
            planetMat.EnableKeyword("_EMISSION");
            planetMat.SetColor("_EmissionColor", new Color(0.7f, 0.2f, 0.5f) * 0.8f);
            planet.GetComponent<Renderer>().sharedMaterial = planetMat;
            Collider pc = planet.GetComponent<Collider>();
            if (pc != null) UnityEngine.Object.DestroyImmediate(pc);

            // Giant Planetary Ring
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "PlanetaryRing";
            ring.transform.SetParent(planet.transform, false);
            ring.transform.localScale = new Vector3(2.4f, 0.005f, 2.4f);
            ring.transform.rotation = Quaternion.Euler(32f, -15f, 18f);
            ring.GetComponent<Renderer>().sharedMaterial = CreateHologramMaterial(new Color(1.0f, 0.7f, 0.9f));
            Collider prc = ring.GetComponent<Collider>();
            if (prc != null) UnityEngine.Object.DestroyImmediate(prc);

            // 2. Floating Alien Islands / Mesas with Glass Crystals
            Material rockMat = CreateSciFiMetalMaterial(new Color(0.22f, 0.18f, 0.28f), 0.3f, 0.4f);
            Material crystalMat = CreateGlassMaterial(new Color(0.95f, 0.4f, 0.85f), 0.45f);

            // Main central floating mesa
            CreateFloatingIsland(parent.transform, new Vector3(0f, 2.5f, 38f), new Vector3(18f, 7f, 16f), rockMat, crystalMat);

            // Left floating spire island
            CreateFloatingIsland(parent.transform, new Vector3(-24f, 10f, 52f), new Vector3(14f, 12f, 12f), rockMat, crystalMat);

            // Right floating island with cascading waterfall energy
            CreateFloatingIsland(parent.transform, new Vector3(22f, 6f, 44f), new Vector3(16f, 9f, 15f), rockMat, crystalMat);

            // 3. Energy Cascades / Waterfalls (Vertical glowing beam between floating islands)
            GameObject energyFall = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            energyFall.name = "EnergyCascade";
            energyFall.transform.SetParent(parent.transform, false);
            energyFall.transform.position = new Vector3(0f, -4f, 38f);
            energyFall.transform.localScale = new Vector3(1.2f, 15f, 1.2f);
            energyFall.GetComponent<Renderer>().sharedMaterial = CreateHologramMaterial(new Color(0.4f, 0.85f, 1.0f));
            Collider efc = energyFall.GetComponent<Collider>();
            if (efc != null) UnityEngine.Object.DestroyImmediate(efc);
        }

        private static void CreateFloatingIsland(Transform parent, Vector3 pos, Vector3 scale, Material rockMat, Material crystalMat)
        {
            GameObject island = new GameObject($"FloatingIsland_{pos.x:F0}_{pos.z:F0}");
            island.transform.SetParent(parent, false);
            island.transform.position = pos;

            // Rocky body (flattened sphere or faceted cube)
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "RockMesa";
            rock.transform.SetParent(island.transform, false);
            rock.transform.localScale = scale;
            rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            Collider rc = rock.GetComponent<Collider>();
            if (rc != null) UnityEngine.Object.DestroyImmediate(rc);

            // Glass Crystal Spires on top of the island
            int crystalCount = 5;
            for (int i = 0; i < crystalCount; i++)
            {
                float angle = i * (Mathf.PI * 2f / crystalCount);
                float radius = scale.x * 0.25f;
                Vector3 crystalPos = new Vector3(Mathf.Cos(angle) * radius, scale.y * 0.45f, Mathf.Sin(angle) * radius);

                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crystal.name = $"CrystalSpire_{i}";
                crystal.transform.SetParent(island.transform, false);
                crystal.transform.localPosition = crystalPos;
                crystal.transform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(-10f, 10f));
                crystal.transform.localScale = new Vector3(0.5f, 3.5f + i * 0.8f, 0.5f);
                crystal.GetComponent<Renderer>().sharedMaterial = crystalMat;
                Collider cc = crystal.GetComponent<Collider>();
                if (cc != null) UnityEngine.Object.DestroyImmediate(cc);
            }
        }

        #endregion

        #region Lighting & Atmosphere

        private static void BuildLabLighting(GameObject parent, out List<Light> interiorLights, out List<Light> emergencyLights)
        {
            interiorLights = new List<Light>();
            emergencyLights = new List<Light>();

            GameObject lightingRoot = new GameObject("Lighting");
            lightingRoot.transform.SetParent(parent.transform, false);

            // 1. Exterior Alien Vista Directional Sunlight (Soft Rose / Magenta Twilight)
            GameObject sunGo = new GameObject("AlienSun_Directional");
            sunGo.transform.SetParent(lightingRoot.transform, false);
            sunGo.transform.rotation = Quaternion.Euler(28f, -35f, 0f);
            Light sunLight = sunGo.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(0.95f, 0.65f, 0.82f); // Pinkish twilight
            sunLight.intensity = 1.1f;
            sunLight.shadows = LightShadows.Soft;

            // 2. Interior Ceiling Fluorescent Fixtures
            Vector3[] lightPositions = {
                new Vector3(-3.5f, 4.8f, 0f),
                new Vector3(3.5f, 4.8f, 0f),
                new Vector3(0f, 4.8f, 2.5f),
                new Vector3(0f, 4.8f, -4.0f)
            };

            foreach (var pos in lightPositions)
            {
                GameObject lightGo = new GameObject("CeilingLight");
                lightGo.transform.SetParent(lightingRoot.transform, false);
                lightGo.transform.position = pos;
                Light l = lightGo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(0.85f, 0.95f, 1.0f); // Crisp cyan-white
                l.range = 8.5f;
                l.intensity = 1.2f;
                interiorLights.Add(l);
            }

            // 3. Emergency Red Klaxon Lights (Normally OFF)
            Vector3[] emPositions = {
                new Vector3(-6.2f, 4.2f, 0f),
                new Vector3(6.2f, 4.2f, 0f)
            };

            foreach (var pos in emPositions)
            {
                GameObject emGo = new GameObject("EmergencyKlaxonLight");
                emGo.transform.SetParent(lightingRoot.transform, false);
                emGo.transform.position = pos;
                Light el = emGo.AddComponent<Light>();
                el.type = LightType.Point;
                el.color = new Color(1.0f, 0.1f, 0.05f); // Pure Warning Red
                el.range = 10f;
                el.intensity = 0f;
                emGo.SetActive(false); // Default inactive
                emergencyLights.Add(el);
            }
        }

        #endregion
    }
}
