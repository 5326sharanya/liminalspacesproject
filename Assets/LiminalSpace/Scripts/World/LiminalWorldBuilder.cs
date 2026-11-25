using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Procedurally builds the Hub and all 6 Liminal Realms for each Cycle,
    /// incorporating dynamic anomalies, Cosmic Relics, fireflies, rolling hills, and sculpted details.
    /// </summary>
    public class LiminalWorldBuilder : MonoBehaviour
    {
        public Transform playerSpawnPoint;

        private void Start()
        {
            if (GameObject.Find("WorldRoot") == null && LiminalGameManager.Instance == null)
            {
                BuildCompleteWorld();
            }
        }

        public void BuildCompleteWorld(LiminalRunState runState = null)
        {
            if (runState == null)
            {
                runState = new LiminalRunState();
            }

            GameObject oldWorld = GameObject.Find("WorldRoot");
            if (oldWorld != null)
            {
                if (Application.isPlaying)
                    Destroy(oldWorld);
                else
                    DestroyImmediate(oldWorld);
            }

            GameObject worldRoot = new GameObject("WorldRoot");
            worldRoot.transform.position = Vector3.zero;

            // 1. Build Hub
            GameObject hub = BuildHub(worldRoot.transform, runState);
            Transform hubSpawn = hub.transform.Find("HubSpawnPoint");
            playerSpawnPoint = hubSpawn;

            // 2. Build all 6 Assigned Realms for this Cycle
            Transform hubDoors = hub.transform.Find("Doors");
            for (int i = 0; i < runState.CurrentDoors.Count; i++)
            {
                DoorAssignment assign = runState.CurrentDoors[i];
                GameObject realmObj = BuildRealmForAssignment(worldRoot.transform, assign, i);

                Transform entrance = realmObj.transform.Find("PlayerEntrance");
                Transform doorObj = hubDoors != null ? hubDoors.Find("HubDoor_" + assign.doorNumber) : null;
                WireDoor(doorObj, assign.doorNumber, assign.displayTitle, entrance, assign);

                Transform retDoor = realmObj.transform.Find("ReturnDoor");
                WireReturnDoor(retDoor, hubSpawn);
            }
        }

        private GameObject BuildRealmForAssignment(Transform root, DoorAssignment assign, int slotIndex)
        {
            // Place each realm far apart in world space so lights/fog/audio never bleed between realms
            Vector3 realmOffset = new Vector3((slotIndex + 1) * 320f, 0f, 0f);
            GameObject realm = assign.realmType switch
            {
                RealmType.SilentField => BuildSilentFieldRealm(root, realmOffset, assign),
                RealmType.Poolrooms => BuildPoolroomsRealm(root, realmOffset, assign),
                RealmType.YellowBackrooms => BuildYellowBackroomsRealm(root, realmOffset, assign),
                RealmType.CrimsonStairwell => BuildCrimsonRealm(root, realmOffset, assign),
                RealmType.VelvetCinema => BuildCinemaRealm(root, realmOffset, assign),
                RealmType.WhiteVoid => BuildWhiteVoidRealm(root, realmOffset, assign),
                _ => BuildSilentFieldRealm(root, realmOffset, assign)
            };

            // Spawn Cosmic Relic inside if assigned
            if (assign.hasRelic)
            {
                GameObject relic = LiminalAssetFactory.BuildLiminalRelic();
                relic.transform.SetParent(realm.transform);
                relic.transform.localPosition = new Vector3(2.5f, 0f, 11f);
                relic.AddComponent<RelicPickup>();
            }

            return realm;
        }

        private void WireDoor(Transform doorObj, int index, string title, Transform destination, DoorAssignment assign)
        {
            if (doorObj == null) return;
            DoorController dc = doorObj.GetComponent<DoorController>();
            if (dc == null) dc = doorObj.gameObject.AddComponent<DoorController>();
            dc.Configure(index, title, destination, assign, false, false);
        }

        private void WireReturnDoor(Transform doorObj, Transform destination)
        {
            if (doorObj == null) return;
            DoorController dc = doorObj.GetComponent<DoorController>();
            if (dc == null) dc = doorObj.gameObject.AddComponent<DoorController>();
            dc.Configure(0, "Return to Corridor", destination, null, true, false);
        }

        // =========================================================================
        // HUB: THE CORRIDOR OF FATE
        // =========================================================================
        private GameObject BuildHub(Transform parent, LiminalRunState runState)
        {
            GameObject hub = new GameObject("Hub_CorridorOfFate");
            hub.transform.SetParent(parent);
            hub.transform.position = Vector3.zero;

            Material wallMat = LiminalAssetFactory.CreateMaterial(Color.white, 0.12f, 0f, LiminalAssetFactory.CreateYellowWallpaperTexture());
            Material trimMat = LiminalAssetFactory.CreateMaterial(new Color(0.28f, 0.22f, 0.16f), 0.35f);
            Material floorMat = LiminalAssetFactory.CreateMaterial(Color.white, 0.08f, 0f, LiminalAssetFactory.CreateCarpetTexture());
            Material ceilingMat = LiminalAssetFactory.CreateMaterial(new Color(0.84f, 0.83f, 0.78f), 0.1f);

            float len = 34f;
            float width = 5.4f;
            float height = 3.8f;

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(hub.transform);
            floor.transform.localPosition = new Vector3(0, -0.1f, len * 0.5f);
            floor.transform.localScale = new Vector3(width, 0.2f, len);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Ceiling
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(hub.transform);
            ceiling.transform.localPosition = new Vector3(0, height + 0.1f, len * 0.5f);
            ceiling.transform.localScale = new Vector3(width, 0.2f, len);
            ceiling.GetComponent<Renderer>().sharedMaterial = ceilingMat;

            // Walls
            BuildRoomWalls(hub.transform, width, len, height, wallMat);

            // Wood Baseboard Trim along left & right walls
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject baseboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                baseboard.transform.SetParent(hub.transform);
                baseboard.transform.localPosition = new Vector3(s * (width * 0.5f - 0.12f), 0.09f, len * 0.5f);
                baseboard.transform.localScale = new Vector3(0.06f, 0.18f, len);
                baseboard.GetComponent<Renderer>().sharedMaterial = trimMat;
            }

            // World-Space Inscription Sign on Front Wall
            GameObject signObj = new GameObject("CycleWallInscription");
            signObj.transform.SetParent(hub.transform);
            signObj.transform.localPosition = new Vector3(0, 2.35f, len - 0.22f);
            signObj.transform.localRotation = Quaternion.identity;
            TextMesh tm = signObj.AddComponent<TextMesh>();
            tm.text = $"CYCLE {runState.CurrentCycle}\nTWO DOORS. TWO DESTINIES.\nCHOOSE YOUR FATE";
            tm.fontSize = 32;
            tm.characterSize = 0.055f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.95f, 0.90f, 0.72f);

            // Reality Escape Door 0 (unlocks when Relics collected!)
            if (runState.RelicsCollected >= runState.RelicsRequired)
            {
                GameObject exitDoor = LiminalAssetFactory.BuildDoorPortal("HubDoor_RealityExit", new Color(1.0f, 0.88f, 0.4f), "DOOR 0: REALITY EXIT", true);
                exitDoor.transform.SetParent(hub.transform);
                exitDoor.transform.localPosition = new Vector3(0, 0, len - 0.25f);
                exitDoor.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                var dc = exitDoor.AddComponent<DoorController>();
                dc.Configure(0, "Reality Exit", null, null, false, true);
            }

            // Fluorescent Ceiling Lights
            for (float z = 4f; z < len; z += 6f)
            {
                GameObject lightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lightPanel.name = "LightFixture_" + z;
                lightPanel.transform.SetParent(hub.transform);
                lightPanel.transform.localPosition = new Vector3(0, height - 0.04f, z);
                lightPanel.transform.localScale = new Vector3(1.25f, 0.06f, 2.4f);

                Material litMat = LiminalAssetFactory.CreateEmissiveUnlit(new Color(1.15f, 1.12f, 0.95f));
                lightPanel.GetComponent<Renderer>().sharedMaterial = litMat;
                LiminalAssetFactory.SafeDestroy(lightPanel.GetComponent<Collider>());

                GameObject pLight = new GameObject("PointLight");
                pLight.transform.SetParent(lightPanel.transform);
                pLight.transform.localPosition = new Vector3(0, -0.45f, 0);
                Light l = pLight.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1.0f, 0.95f, 0.82f);
                l.intensity = 1.65f;
                l.range = 8.5f;

                if (z > 15f)
                {
                    pLight.AddComponent<FlickeringLight>();
                }
            }

            // Fluorescent Hum Audio
            GameObject humObj = new GameObject("FluorescentHumSource");
            humObj.transform.SetParent(hub.transform);
            humObj.transform.localPosition = new Vector3(0, height * 0.5f, len * 0.5f);
            AudioSource asrc = humObj.AddComponent<AudioSource>();
            asrc.clip = LiminalAudioSynth.CreateFluorescentHum();
            asrc.loop = true;
            asrc.playOnAwake = true;
            asrc.volume = 0.28f;
            asrc.spatialBlend = 0.4f;
            asrc.Play();

            // 2 Doors Container at End of Hallway
            GameObject doors = new GameObject("Doors");
            doors.transform.SetParent(hub.transform);

            // Door 1 (Left): The Celestial Horizon (Luminous Lavender/Gold Frame)
            // Door 2 (Right): The Subterranean Poolrooms (Rusted Aqua Frame)
            float endZ = len - 6.0f;
            float[] doorX = { -1.38f, 1.38f };
            float[] doorYRot = { 10f, -10f };

            for (int i = 0; i < runState.CurrentDoors.Count; i++)
            {
                DoorAssignment assign = runState.CurrentDoors[i];
                float xPos = (i < doorX.Length) ? doorX[i] : 0f;
                float yRot = (i < doorYRot.Length) ? doorYRot[i] : 0f;

                string plaque = $"DOOR {assign.doorNumber}\n{assign.displayTitle.ToUpper()}";
                GameObject doorObj = LiminalAssetFactory.BuildDoorPortal($"HubDoor_{assign.doorNumber}", assign.frameColor, plaque, true);
                doorObj.transform.SetParent(doors.transform);
                doorObj.transform.localPosition = new Vector3(xPos, 0, endZ);
                doorObj.transform.localRotation = Quaternion.Euler(0, yRot, 0);
            }

            GameObject hubSpawn = new GameObject("HubSpawnPoint");
            hubSpawn.transform.SetParent(hub.transform);
            hubSpawn.transform.localPosition = new Vector3(0, 1.0f, 2.5f);
            hubSpawn.transform.localRotation = Quaternion.identity;

            return hub;
        }

        // =========================================================================
        // REALM 1: THE CELESTIAL HORIZON (ETHEREAL FANTASY WORLD)
        // =========================================================================
        private GameObject BuildSilentFieldRealm(Transform parent, Vector3 offset, DoorAssignment assign)
        {
            GameObject realm = new GameObject("Realm_CelestialHorizon");
            realm.transform.SetParent(parent);
            realm.transform.position = offset;

            bool whiteFlowers = (assign.anomaly == AnomalyType.WhiteFlowers);
            bool crimsonSky = (assign.anomaly == AnomalyType.CrimsonSky);
            bool twinWatchers = (assign.anomaly == AnomalyType.TwinWatchers);
            bool hollowGaze = (assign.anomaly == AnomalyType.HollowGaze);
            bool missingBed = (assign.anomaly == AnomalyType.MissingBed);

            // 1. Vast Rolling Ethereal Meadow with Bioluminescent Grass Texture
            Material grassMat = LiminalAssetFactory.CreateMaterial(Color.white, 0.42f, 0.05f, LiminalAssetFactory.CreateLuminousGrassTexture(), new Color(0.10f, 0.32f, 0.36f) * 0.45f);
            GameObject field = LiminalAssetFactory.BuildRollingGreenField(grassMat);
            field.transform.SetParent(realm.transform, false);
            field.transform.localPosition = Vector3.zero;

            // 2. Expansive Twilight Starfield Sky Dome
            GameObject starfieldObj = new GameObject("CelestialStarfieldSkyDome");
            starfieldObj.transform.SetParent(realm.transform, false);
            starfieldObj.transform.localPosition = new Vector3(0, 2f, 0);
            StarfieldGenerator starGen = starfieldObj.AddComponent<StarfieldGenerator>();
            starGen.SetCrimsonMode(crimsonSky);
            starGen.SetupStarfield();

            // 3. Gigantic Banded Ringed Planet hovering close to the horizon
            Vector3 planetPos = crimsonSky ? new Vector3(140f, 65f, 220f) : new Vector3(125f, 62f, 210f);
            GameObject ringedPlanet = LiminalAssetFactory.BuildGiganticRingedPlanet(planetPos, 130f);
            ringedPlanet.transform.SetParent(realm.transform, false);

            // 4. Luminous Secondary Moon
            Vector3 moonPos = new Vector3(-105f, 85f, 185f);
            GameObject moon = LiminalAssetFactory.BuildLuminousMoon(moonPos, 46f);
            moon.transform.SetParent(realm.transform, false);

            // 5. Monumental Celestial Arch spanning the horizon
            GameObject celestialArch = LiminalAssetFactory.BuildCelestialMonumentArch(new Vector3(6f, 0f, 52f), 34f, 2.6f);
            celestialArch.transform.SetParent(realm.transform, false);

            // 6. Surreal Floating Islands with Cascading Luminous Waterfalls
            Material rockMat = LiminalAssetFactory.CreateMaterial(new Color(0.24f, 0.22f, 0.30f), 0.25f);
            Material islandTopMat = LiminalAssetFactory.CreateMaterial(new Color(0.35f, 0.85f, 0.75f), 0.5f, 0f, null, new Color(0.20f, 0.65f, 0.60f) * 0.8f);
            Material waterMat = LiminalAssetFactory.CreateMaterial(new Color(0.65f, 0.94f, 1.0f, 0.82f), 0.98f, 0.15f, LiminalAssetFactory.CreateCrystallineWaterTexture(), new Color(0.35f, 0.78f, 0.95f) * 0.9f);

            // Floating Island 1 (Left) with Waterfall
            GameObject island1 = LiminalAssetFactory.BuildFloatingIsland(new Vector3(-38f, 34f, 82f), new Vector3(32f, 16f, 32f), islandTopMat, rockMat);
            island1.transform.SetParent(realm.transform, false);

            GameObject waterfall1 = LiminalAssetFactory.BuildLuminousWaterfall(new Vector3(-38f, 34f, 82f), 34f, 9f, waterMat);
            waterfall1.transform.SetParent(realm.transform, false);

            // Floating Island 2 (Right, elevated higher)
            GameObject island2 = LiminalAssetFactory.BuildFloatingIsland(new Vector3(48f, 48f, 120f), new Vector3(38f, 18f, 38f), islandTopMat, rockMat);
            island2.transform.SetParent(realm.transform, false);

            // 7. Winding Crystalline River / Reflective Pool in the valley
            GameObject riverPool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            riverPool.name = "ReflectiveCrystallinePool";
            riverPool.transform.SetParent(realm.transform, false);
            riverPool.transform.localPosition = new Vector3(8.5f, 0.04f, 16.5f);
            riverPool.transform.localScale = new Vector3(7.2f, 0.03f, 12.5f);
            riverPool.GetComponent<Renderer>().sharedMaterial = waterMat;

            // 8. Delicate Stardust / Cosmic Pollen Floating Particles
            Color pollenCol = crimsonSky ? new Color(1.0f, 0.35f, 0.35f) : new Color(0.72f, 0.88f, 1.0f);
            GameObject stardust = LiminalAssetFactory.BuildStardustPollenParticles(pollenCol, 240);
            stardust.transform.SetParent(realm.transform, false);
            stardust.transform.localPosition = new Vector3(0, 1.8f, 20f);

            // 9. Bioluminescent Alien Flowers flanking the landscape
            Vector3[] flowerPos =
            {
                new Vector3(-2.2f, 0f, 4.2f),
                new Vector3(2.5f, 0f, 4.8f),
                new Vector3(-4.8f, 0f, 12f),
                new Vector3(4.2f, 0f, 14f),
                new Vector3(-1.8f, 0f, 22f),
                new Vector3(3.6f, 0f, 26f),
                new Vector3(-2.8f, 0f, 32f),
                new Vector3(2.8f, 0f, 33f)
            };
            Color[] flowerColors =
            {
                new Color(0.92f, 0.58f, 0.76f), // Rose Pink
                new Color(0.48f, 0.92f, 0.98f), // Icy Cyan
                new Color(0.78f, 0.65f, 0.95f), // Lavender
                new Color(1.0f, 0.85f, 0.60f)   // Soft Warm Gold
            };

            for (int f = 0; f < flowerPos.Length; f++)
            {
                Color pCol = whiteFlowers ? Color.white : flowerColors[f % flowerColors.Length];
                Color gCol = whiteFlowers ? new Color(0.85f, 0.90f, 1.0f) : flowerColors[(f + 1) % flowerColors.Length];
                GameObject fl = LiminalAssetFactory.BuildBioluminescentAlienFlower(100 + f, pCol, gCol);
                fl.transform.SetParent(realm.transform, false);
                fl.transform.localPosition = flowerPos[f];
            }

            // 10. Ambient Audio: Celestial Choir Pad + Cascading Waterfall Audio
            GameObject audioObj = new GameObject("CelestialAmbienceAudio");
            audioObj.transform.SetParent(realm.transform, false);
            audioObj.transform.localPosition = Vector3.zero;
            AudioSource ambSrc = audioObj.AddComponent<AudioSource>();
            ambSrc.clip = LiminalAudioSynth.CreateCelestialEtherealAmbience();
            ambSrc.loop = true;
            ambSrc.playOnAwake = true;
            ambSrc.volume = 0.55f;
            ambSrc.spatialBlend = 0.4f;
            ambSrc.Play();

            // Waterfall sound near the falls
            GameObject fallAudio = new GameObject("WaterfallAudio");
            fallAudio.transform.SetParent(realm.transform, false);
            fallAudio.transform.localPosition = new Vector3(-38f, 6f, 82f);
            AudioSource fallSrc = fallAudio.AddComponent<AudioSource>();
            fallSrc.clip = LiminalAudioSynth.CreateCascadingWaterAudio();
            fallSrc.loop = true;
            fallSrc.spatialBlend = 1.0f;
            fallSrc.maxDistance = 75f;
            fallSrc.volume = 0.65f;
            fallSrc.Play();

            // 11. Soft Celestial Directional Sunlight/Starlight
            GameObject celestialSun = new GameObject("CelestialStarlight");
            celestialSun.transform.SetParent(realm.transform, false);
            celestialSun.transform.localPosition = new Vector3(0, 30f, 0);
            Light sunL = celestialSun.AddComponent<Light>();
            sunL.type = LightType.Directional;
            sunL.color = crimsonSky ? new Color(0.95f, 0.35f, 0.35f) : new Color(1.0f, 0.90f, 0.84f);
            sunL.intensity = 0.55f;
            sunL.shadows = LightShadows.Soft;
            celestialSun.transform.localRotation = Quaternion.Euler(48f, -25f, 0);

            // 12. Freestanding White Doorway (Return Door) with Glitter Sand Threshold
            GameObject doorway = LiminalAssetFactory.BuildDoorPortal("ReturnDoor", Color.white, "RETURN TO CORRIDOR", false);
            doorway.transform.SetParent(realm.transform, false);
            doorway.transform.localPosition = new Vector3(0, 0, 0);

            GameObject sandThreshold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sandThreshold.name = "GlitterSandThreshold";
            sandThreshold.transform.SetParent(realm.transform, false);
            sandThreshold.transform.localPosition = new Vector3(0, 0.02f, 1.8f);
            sandThreshold.transform.localScale = new Vector3(3.6f, 0.04f, 3.8f);
            Material sandMat = LiminalAssetFactory.CreateMaterial(Color.white, 0.92f, 0.25f, LiminalAssetFactory.CreateGlitterSandTexture(), new Color(0.18f, 0.22f, 0.32f));
            sandThreshold.GetComponent<Renderer>().sharedMaterial = sandMat;

            // 13. Cobblestone Garden Path leading to the Overlook
            Vector3[] pathPoints =
            {
                new Vector3(0, 0.02f, 3.8f),
                new Vector3(0.5f, 0.02f, 7f),
                new Vector3(1.4f, 0.02f, 11f),
                new Vector3(2.5f, 0.02f, 15f),
                new Vector3(2.8f, 0.02f, 19f),
                new Vector3(2.2f, 0.02f, 23f),
                new Vector3(0.8f, 0.02f, 27f),
                new Vector3(-0.4f, 0.02f, 31f)
            };

            Material gravelMat = LiminalAssetFactory.CreateMaterial(new Color(0.58f, 0.54f, 0.62f), 0.35f);
            for (int p = 0; p < pathPoints.Length - 1; p++)
            {
                Vector3 p1 = pathPoints[p];
                Vector3 p2 = pathPoints[p + 1];
                Vector3 mid = (p1 + p2) * 0.5f;
                float dist = Vector3.Distance(p1, p2);

                GameObject pathSeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pathSeg.name = "PathSegment_" + p;
                pathSeg.transform.SetParent(realm.transform, false);
                pathSeg.transform.localPosition = mid;
                pathSeg.transform.localScale = new Vector3(1.65f, 0.03f, dist);
                pathSeg.transform.localRotation = Quaternion.LookRotation(p2 - p1);
                pathSeg.GetComponent<Renderer>().sharedMaterial = gravelMat;
            }

            // 14. Dining Table with Bread, Water & Ethereal Roses
            GameObject diningTable = LiminalAssetFactory.BuildDiningTableWithFood(whiteFlowers);
            diningTable.transform.SetParent(realm.transform, false);
            diningTable.transform.localPosition = new Vector3(-3.8f, 0, 16.5f);
            diningTable.transform.localRotation = Quaternion.Euler(0, 35f, 0);
            diningTable.AddComponent<TableInteraction>();

            // 15. Golden Proceed Archway (unlocks after resting 48h)
            GameObject proceedArch = LiminalAssetFactory.BuildProceedArchway();
            proceedArch.transform.SetParent(realm.transform, false);
            proceedArch.transform.localPosition = new Vector3(4.5f, 0, 34f);
            proceedArch.transform.localRotation = Quaternion.Euler(0, 200f, 0);
            proceedArch.AddComponent<ProceedPortal>();
            proceedArch.SetActive(false);

            // 16. Pristine White Bed on the Panoramic Celestial Overlook
            if (!missingBed)
            {
                GameObject whiteBed = LiminalAssetFactory.BuildWhiteBed();
                whiteBed.transform.SetParent(realm.transform, false);
                whiteBed.transform.localPosition = new Vector3(-0.4f, 0, 32.5f);
                whiteBed.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                var bedComp = whiteBed.AddComponent<BedInteraction>();
                bedComp.proceedArchwayToActivate = proceedArch;
            }

            // 17. The Tall Silent Man watching peacefully across the horizon
            GameObject tallMan = LiminalAssetFactory.BuildTallSilentMan(hollowGaze);
            tallMan.transform.SetParent(realm.transform, false);
            tallMan.transform.localPosition = new Vector3(-10f, 0, 24f);
            tallMan.transform.localRotation = Quaternion.Euler(0, 90f, 0);

            TallSilentManController tsmCtrl = tallMan.AddComponent<TallSilentManController>();
            tsmCtrl.isCorruptedWhisperer = hollowGaze;

            GameObject entitySpawn = new GameObject("EntitySpawnPoint");
            entitySpawn.transform.SetParent(realm.transform, false);
            entitySpawn.transform.localPosition = new Vector3(-14f, 0, 24f);

            GameObject entityExit = new GameObject("EntityExitPoint");
            entityExit.transform.SetParent(realm.transform, false);
            entityExit.transform.localPosition = new Vector3(-26f, 0, 35f);

            tsmCtrl.spawnPoint = entitySpawn.transform;
            tsmCtrl.tableTarget = diningTable.transform;
            tsmCtrl.exitPoint = entityExit.transform;

            if (twinWatchers)
            {
                GameObject twin = LiminalAssetFactory.BuildTallSilentMan(false);
                twin.name = "TwinWatcherAnomaly";
                twin.transform.SetParent(realm.transform, false);
                twin.transform.localPosition = new Vector3(8.5f, 0, 24f);
                twin.transform.localRotation = Quaternion.Euler(0, -140f, 0);
            }

            // Entrance & Trigger Zone
            GameObject entrance = new GameObject("PlayerEntrance");
            entrance.transform.SetParent(realm.transform, false);
            entrance.transform.localPosition = new Vector3(0, 1.0f, 1.8f);

            GameObject triggerZone = new GameObject("SequenceTriggerZone");
            triggerZone.transform.SetParent(realm.transform, false);
            triggerZone.transform.localPosition = new Vector3(0, 1.0f, 6.0f);
            BoxCollider tc = triggerZone.AddComponent<BoxCollider>();
            tc.isTrigger = true;
            tc.size = new Vector3(14f, 4f, 6f);
            triggerZone.AddComponent<SanctuarySequenceTrigger>().targetController = tsmCtrl;

            return realm;
        }

        // =========================================================================
        // REALM 2: THE POOLROOMS
        // =========================================================================
        private GameObject BuildPoolroomsRealm(Transform parent, Vector3 offset, DoorAssignment assign)
        {
            GameObject realm = new GameObject("Realm_Poolrooms");
            realm.transform.SetParent(parent);
            realm.transform.position = offset;

            bool bloodWater = (assign.anomaly == AnomalyType.BloodWater);
            bool redLights = (assign.anomaly == AnomalyType.ReversedLights);

            Material tileMat = LiminalAssetFactory.CreateMaterial(Color.white, 0.85f, 0.05f, LiminalAssetFactory.CreatePoolTileTexture());
            Color wColor = bloodWater ? new Color(0.55f, 0.05f, 0.08f, 0.88f) : new Color(0.18f, 0.78f, 0.88f, 0.72f);
            Material waterMat = LiminalAssetFactory.CreateMaterial(wColor, 0.98f, 0.2f);

            float size = 26f;
            float height = 4.6f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "TiledFloor";
            floor.transform.SetParent(realm.transform);
            floor.transform.localPosition = new Vector3(0, -0.4f, size * 0.5f);
            floor.transform.localScale = new Vector3(size, 0.2f, size);
            floor.GetComponent<Renderer>().sharedMaterial = tileMat;

            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "PoolWater";
            water.transform.SetParent(realm.transform);
            water.transform.localPosition = new Vector3(0, -0.12f, size * 0.5f);
            water.transform.localScale = new Vector3(size - 0.2f, 0.35f, size - 0.2f);
            water.GetComponent<Renderer>().sharedMaterial = waterMat;

            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "TiledCeiling";
            ceiling.transform.SetParent(realm.transform);
            ceiling.transform.localPosition = new Vector3(0, height, size * 0.5f);
            ceiling.transform.localScale = new Vector3(size, 0.2f, size);
            ceiling.GetComponent<Renderer>().sharedMaterial = tileMat;

            BuildRoomWalls(realm.transform, size, size, height, tileMat);

            float[] px = { -6f, 6f, -6f, 6f };
            float[] pz = { 7f, 7f, 19f, 19f };
            for (int i = 0; i < 4; i++)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.transform.SetParent(realm.transform);
                pillar.transform.localPosition = new Vector3(px[i], height * 0.5f, pz[i]);
                pillar.transform.localScale = new Vector3(1.8f, height, 1.8f);
                pillar.GetComponent<Renderer>().sharedMaterial = tileMat;
            }

            GameObject lightObj = new GameObject("AquaLight");
            lightObj.transform.SetParent(realm.transform);
            lightObj.transform.localPosition = new Vector3(0, 3.0f, size * 0.5f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = redLights ? new Color(1.0f, 0.25f, 0.25f) : new Color(0.45f, 0.90f, 0.98f);
            l.intensity = 2.4f;
            l.range = 24f;

            GameObject dripAudio = new GameObject("DripAudio");
            dripAudio.transform.SetParent(realm.transform);
            dripAudio.transform.localPosition = new Vector3(0, 1.5f, 12f);
            AudioSource das = dripAudio.AddComponent<AudioSource>();
            das.clip = LiminalAudioSynth.CreateWaterDrip();
            das.loop = true;
            das.spatialBlend = 1.0f;
            das.maxDistance = 30f;
            das.volume = 0.4f;
            das.Play();

            // Proceed Portal at far end (only active if Normal!)
            if (!assign.isAnomaly)
            {
                GameObject proceed = LiminalAssetFactory.BuildProceedArchway();
                proceed.transform.SetParent(realm.transform);
                proceed.transform.localPosition = new Vector3(0, 0, size - 2.2f);
                proceed.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                proceed.AddComponent<ProceedPortal>();
            }

            GameObject retDoor = LiminalAssetFactory.BuildDoorPortal("ReturnDoor", new Color(0.4f, 0.85f, 0.95f), "RETURN", false);
            retDoor.transform.SetParent(realm.transform);
            retDoor.transform.localPosition = new Vector3(0, 0, 1.2f);

            GameObject entrance = new GameObject("PlayerEntrance");
            entrance.transform.SetParent(realm.transform);
            entrance.transform.localPosition = new Vector3(0, 1.0f, 2.5f);

            return realm;
        }

        // =========================================================================
        // REALM 3: MONO-YELLOW BACKROOMS
        // =========================================================================
        private GameObject BuildYellowBackroomsRealm(Transform parent, Vector3 offset, DoorAssignment assign)
        {
            GameObject realm = new GameObject("Realm_YellowBackrooms");
            realm.transform.SetParent(parent);
            realm.transform.position = offset;

            bool redAnomaly = (assign.anomaly == AnomalyType.ReversedLights);

            Material wallMat = LiminalAssetFactory.CreateMaterial(Color.white, 0.1f, 0f, LiminalAssetFactory.CreateYellowWallpaperTexture());
            Material carpetMat = LiminalAssetFactory.CreateMaterial(Color.white, 0.08f, 0f, LiminalAssetFactory.CreateCarpetTexture());
            Material ceilingMat = LiminalAssetFactory.CreateMaterial(new Color(0.85f, 0.84f, 0.80f), 0.1f);

            float size = 28f;
            float height = 3.2f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(realm.transform);
            floor.transform.localPosition = new Vector3(0, -0.1f, size * 0.5f);
            floor.transform.localScale = new Vector3(size, 0.2f, size);
            floor.GetComponent<Renderer>().sharedMaterial = carpetMat;

            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.transform.SetParent(realm.transform);
            ceiling.transform.localPosition = new Vector3(0, height + 0.1f, size * 0.5f);
            ceiling.transform.localScale = new Vector3(size, 0.2f, size);
            ceiling.GetComponent<Renderer>().sharedMaterial = ceilingMat;

            BuildRoomWalls(realm.transform, size, size, height, wallMat);

            CreatePartition(realm.transform, new Vector3(-4.5f, height * 0.5f, 8f), new Vector3(0.35f, height, 9f), wallMat);
            CreatePartition(realm.transform, new Vector3(5.5f, height * 0.5f, 13f), new Vector3(0.35f, height, 11f), wallMat);
            CreatePartition(realm.transform, new Vector3(-1f, height * 0.5f, 18f), new Vector3(9f, height, 0.35f), wallMat);

            GameObject flLight = new GameObject("FlickerLight");
            flLight.transform.SetParent(realm.transform);
            flLight.transform.localPosition = new Vector3(0, height - 0.25f, 14f);
            Light fl = flLight.AddComponent<Light>();
            fl.type = LightType.Point;
            fl.color = redAnomaly ? new Color(1.0f, 0.15f, 0.15f) : new Color(0.95f, 0.90f, 0.65f);
            fl.intensity = 2.0f;
            fl.range = 18f;
            flLight.AddComponent<FlickeringLight>();

            if (!assign.isAnomaly)
            {
                GameObject proceed = LiminalAssetFactory.BuildProceedArchway();
                proceed.transform.SetParent(realm.transform);
                proceed.transform.localPosition = new Vector3(-7f, 0, size - 2.5f);
                proceed.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                proceed.AddComponent<ProceedPortal>();
            }

            GameObject retDoor = LiminalAssetFactory.BuildDoorPortal("ReturnDoor", new Color(0.9f, 0.8f, 0.3f), "RETURN", false);
            retDoor.transform.SetParent(realm.transform);
            retDoor.transform.localPosition = new Vector3(0, 0, 1.2f);

            GameObject entrance = new GameObject("PlayerEntrance");
            entrance.transform.SetParent(realm.transform);
            entrance.transform.localPosition = new Vector3(0, 1.0f, 2.5f);

            return realm;
        }

        // =========================================================================
        // REALM 4: CRIMSON STAIRWELL
        // =========================================================================
        private GameObject BuildCrimsonRealm(Transform parent, Vector3 offset, DoorAssignment assign)
        {
            GameObject realm = new GameObject("Realm_CrimsonStairwell");
            realm.transform.SetParent(parent);
            realm.transform.position = offset;

            bool reversed = (assign.anomaly == AnomalyType.ReversedLights);
            Material concreteMat = LiminalAssetFactory.CreateMaterial(new Color(0.20f, 0.20f, 0.22f), 0.25f);
            Material railMat = LiminalAssetFactory.CreateMaterial(new Color(0.75f, 0.15f, 0.15f), 0.6f, 0.7f);

            float size = 20f;
            float height = 6.0f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(realm.transform);
            floor.transform.localPosition = new Vector3(0, -0.1f, size * 0.5f);
            floor.transform.localScale = new Vector3(size, 0.2f, size);
            floor.GetComponent<Renderer>().sharedMaterial = concreteMat;

            BuildRoomWalls(realm.transform, size, size, height, concreteMat);

            // Stepped concrete staircase leading up to an elevated platform
            for (int s = 0; s < 10; s++)
            {
                GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.transform.SetParent(realm.transform);
                step.transform.localPosition = new Vector3(0, 0.15f + s * 0.25f, 7f + s * 0.65f);
                step.transform.localScale = new Vector3(3.2f, 0.25f, 0.7f);
                step.GetComponent<Renderer>().sharedMaterial = concreteMat;
            }

            GameObject landing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landing.transform.SetParent(realm.transform);
            landing.transform.localPosition = new Vector3(0, 2.5f, 15.8f);
            landing.transform.localScale = new Vector3(5.0f, 0.3f, 5.0f);
            landing.GetComponent<Renderer>().sharedMaterial = concreteMat;

            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.SetParent(realm.transform);
            rail.transform.localPosition = new Vector3(-1.55f, 2.1f, 10.2f);
            rail.transform.localScale = new Vector3(0.08f, 0.08f, 6.8f);
            rail.transform.localRotation = Quaternion.Euler(-21f, 0, 0);
            rail.GetComponent<Renderer>().sharedMaterial = railMat;

            GameObject redBeacon = new GameObject("RedBeacon");
            redBeacon.transform.SetParent(realm.transform);
            redBeacon.transform.localPosition = new Vector3(0, 4.8f, size * 0.5f);
            Light rl = redBeacon.AddComponent<Light>();
            rl.type = LightType.Spot;
            rl.color = reversed ? new Color(0.2f, 1.0f, 0.4f) : new Color(1.0f, 0.06f, 0.06f);
            rl.intensity = 5.0f;
            rl.range = 20f;
            rl.spotAngle = 80f;
            redBeacon.AddComponent<RotatingLight>();

            if (!assign.isAnomaly)
            {
                GameObject proceed = LiminalAssetFactory.BuildProceedArchway();
                proceed.transform.SetParent(realm.transform);
                proceed.transform.localPosition = new Vector3(0, 2.65f, 17.2f);
                proceed.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                proceed.AddComponent<ProceedPortal>();
            }

            GameObject retDoor = LiminalAssetFactory.BuildDoorPortal("ReturnDoor", new Color(0.8f, 0.15f, 0.15f), "RETURN", false);
            retDoor.transform.SetParent(realm.transform);
            retDoor.transform.localPosition = new Vector3(0, 0, 1.2f);

            GameObject entrance = new GameObject("PlayerEntrance");
            entrance.transform.SetParent(realm.transform);
            entrance.transform.localPosition = new Vector3(0, 1.0f, 2.5f);

            return realm;
        }

        // =========================================================================
        // REALM 5: VELVET CINEMA
        // =========================================================================
        private GameObject BuildCinemaRealm(Transform parent, Vector3 offset, DoorAssignment assign)
        {
            GameObject realm = new GameObject("Realm_VelvetCinema");
            realm.transform.SetParent(parent);
            realm.transform.position = offset;

            bool staticAnomaly = (assign.anomaly == AnomalyType.StaticScreen);

            Material redCarpet = LiminalAssetFactory.CreateMaterial(new Color(0.45f, 0.06f, 0.12f), 0.15f);
            Material velvetMat = LiminalAssetFactory.CreateMaterial(new Color(0.62f, 0.05f, 0.14f), 0.2f);
            Material darkMat = LiminalAssetFactory.CreateMaterial(new Color(0.08f, 0.08f, 0.10f), 0.2f);
            Material screenMat = LiminalAssetFactory.CreateEmissiveUnlit(staticAnomaly ? new Color(1.2f, 0.12f, 0.12f) : new Color(0.92f, 0.95f, 1.0f));

            float size = 24f;
            float height = 5.4f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(realm.transform);
            floor.transform.localPosition = new Vector3(0, -0.1f, size * 0.5f);
            floor.transform.localScale = new Vector3(size, 0.2f, size);
            floor.GetComponent<Renderer>().sharedMaterial = redCarpet;

            BuildRoomWalls(realm.transform, size, size, height, darkMat);

            // Rows of Red Velvet Theater Seats
            for (int row = 0; row < 3; row++)
            {
                for (int col = -3; col <= 3; col++)
                {
                    if (col == 0) continue; // Center aisle
                    GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    seat.transform.SetParent(realm.transform);
                    seat.transform.localPosition = new Vector3(col * 1.4f, 0.45f, 8f + row * 2.5f);
                    seat.transform.localScale = new Vector3(0.85f, 0.9f, 0.75f);
                    seat.GetComponent<Renderer>().sharedMaterial = velvetMat;
                }
            }

            // Movie Screen
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "ProjectionScreen";
            screen.transform.SetParent(realm.transform);
            screen.transform.localPosition = new Vector3(0, 2.8f, size - 0.3f);
            screen.transform.localScale = new Vector3(13f, 4.6f, 0.1f);
            screen.GetComponent<Renderer>().sharedMaterial = screenMat;

            GameObject beam = new GameObject("ProjectorBeam");
            beam.transform.SetParent(realm.transform);
            beam.transform.localPosition = new Vector3(0, 4.6f, 4f);
            Light bl = beam.AddComponent<Light>();
            bl.type = LightType.Spot;
            bl.color = staticAnomaly ? new Color(1.0f, 0.2f, 0.2f) : new Color(0.92f, 0.96f, 1.0f);
            bl.intensity = 4.2f;
            bl.range = 28f;
            bl.spotAngle = 42f;
            beam.transform.localRotation = Quaternion.Euler(14f, 0, 0);

            if (!assign.isAnomaly)
            {
                GameObject proceed = LiminalAssetFactory.BuildProceedArchway();
                proceed.transform.SetParent(realm.transform);
                proceed.transform.localPosition = new Vector3(6.5f, 0, size - 2.2f);
                proceed.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                proceed.AddComponent<ProceedPortal>();
            }

            GameObject retDoor = LiminalAssetFactory.BuildDoorPortal("ReturnDoor", new Color(0.6f, 0.1f, 0.3f), "RETURN", false);
            retDoor.transform.SetParent(realm.transform);
            retDoor.transform.localPosition = new Vector3(0, 0, 1.2f);

            GameObject entrance = new GameObject("PlayerEntrance");
            entrance.transform.SetParent(realm.transform);
            entrance.transform.localPosition = new Vector3(0, 1.0f, 2.5f);

            return realm;
        }

        // =========================================================================
        // REALM 6: WHITE MIST VOID
        // =========================================================================
        private GameObject BuildWhiteVoidRealm(Transform parent, Vector3 offset, DoorAssignment assign)
        {
            GameObject realm = new GameObject("Realm_WhiteVoid");
            realm.transform.SetParent(parent);
            realm.transform.position = offset;

            Material whiteFloorMat = LiminalAssetFactory.CreateMaterial(new Color(0.96f, 0.96f, 0.98f), 0.92f);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "WhiteGlossyFloor";
            floor.transform.SetParent(realm.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(10f, 1f, 10f);
            floor.GetComponent<Renderer>().sharedMaterial = whiteFloorMat;

            GameObject proceed = LiminalAssetFactory.BuildProceedArchway();
            proceed.transform.SetParent(realm.transform);
            proceed.transform.localPosition = new Vector3(0, 0, 22f);
            proceed.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            proceed.AddComponent<ProceedPortal>();
            proceed.SetActive(false);

            // Solitary Vintage Payphone
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "PhonePole";
            pole.transform.SetParent(realm.transform);
            pole.transform.localPosition = new Vector3(0, 1.0f, 14f);
            pole.transform.localScale = new Vector3(0.12f, 1.0f, 0.12f);
            pole.GetComponent<Renderer>().sharedMaterial = LiminalAssetFactory.CreateMaterial(new Color(0.12f, 0.12f, 0.12f), 0.6f);

            GameObject phoneBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            phoneBox.name = "Payphone";
            phoneBox.transform.SetParent(pole.transform);
            phoneBox.transform.localPosition = new Vector3(0, 0.5f, 0.25f);
            phoneBox.transform.localScale = new Vector3(3.2f, 0.55f, 2.2f);
            phoneBox.GetComponent<Renderer>().sharedMaterial = LiminalAssetFactory.CreateMaterial(new Color(0.18f, 0.18f, 0.20f), 0.4f);

            var phoneComp = pole.AddComponent<PayphoneInteraction>();
            phoneComp.proceedArchwayToActivate = proceed;

            GameObject light = new GameObject("PayphoneLight");
            light.transform.SetParent(realm.transform);
            light.transform.localPosition = new Vector3(0, 4.0f, 14f);
            Light pl = light.AddComponent<Light>();
            pl.type = LightType.Spot;
            pl.color = assign.isAnomaly ? new Color(1.0f, 0.35f, 0.35f) : Color.white;
            pl.intensity = 3.2f;
            pl.range = 12f;
            pl.spotAngle = 60f;
            light.transform.localRotation = Quaternion.Euler(90f, 0, 0);

            if (assign.anomaly == AnomalyType.HollowGaze)
            {
                GameObject mistWatcher = LiminalAssetFactory.BuildTallSilentMan(true);
                mistWatcher.name = "MistWatcherAnomaly";
                mistWatcher.transform.SetParent(realm.transform);
                mistWatcher.transform.localPosition = new Vector3(2.4f, 0, 16.5f);
                mistWatcher.transform.localRotation = Quaternion.Euler(0, -165f, 0);
            }

            GameObject retDoor = LiminalAssetFactory.BuildDoorPortal("ReturnDoor", Color.white, "RETURN", false);
            retDoor.transform.SetParent(realm.transform);
            retDoor.transform.localPosition = new Vector3(0, 0, 1.2f);

            GameObject entrance = new GameObject("PlayerEntrance");
            entrance.transform.SetParent(realm.transform);
            entrance.transform.localPosition = new Vector3(0, 1.0f, 2.5f);

            return realm;
        }

        private void BuildRoomWalls(Transform parent, float width, float length, float height, Material mat)
        {
            GameObject bw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bw.transform.SetParent(parent);
            bw.transform.localPosition = new Vector3(0, height * 0.5f, 0);
            bw.transform.localScale = new Vector3(width, height, 0.2f);
            bw.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject fw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fw.transform.SetParent(parent);
            fw.transform.localPosition = new Vector3(0, height * 0.5f, length);
            fw.transform.localScale = new Vector3(width, height, 0.2f);
            fw.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject lw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lw.transform.SetParent(parent);
            lw.transform.localPosition = new Vector3(-width * 0.5f, height * 0.5f, length * 0.5f);
            lw.transform.localScale = new Vector3(0.2f, height, length);
            lw.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject rw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rw.transform.SetParent(parent);
            rw.transform.localPosition = new Vector3(width * 0.5f, height * 0.5f, length * 0.5f);
            rw.transform.localScale = new Vector3(0.2f, height, length);
            rw.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private void CreatePartition(Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.transform.SetParent(parent);
            p.transform.localPosition = localPos;
            p.transform.localScale = scale;
            p.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }

    public class SanctuarySequenceTrigger : MonoBehaviour
    {
        public TallSilentManController targetController;
        private bool m_Triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (m_Triggered) return;
            if (other.GetComponent<LiminalPlayerController>() != null)
            {
                m_Triggered = true;
                targetController?.TriggerSequence();
            }
        }
    }

    public class RotatingLight : MonoBehaviour
    {
        public float speed = 120f;
        private void Update()
        {
            transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.World);
        }
    }

    public class FlickeringLight : MonoBehaviour
    {
        private Light m_Light;
        private float m_BaseIntensity;

        private void Awake()
        {
            m_Light = GetComponent<Light>();
            if (m_Light != null) m_BaseIntensity = m_Light.intensity;
        }

        private void Update()
        {
            if (m_Light == null) return;
            if (Random.value > 0.94f)
            {
                m_Light.intensity = m_BaseIntensity * Random.Range(0.2f, 1.0f);
            }
            else
            {
                m_Light.intensity = m_BaseIntensity;
            }
        }
    }
}
