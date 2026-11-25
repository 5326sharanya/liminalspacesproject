using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Procedurally constructs high-craft materials, textures, particle effects, and sculpted 3D geometry
    /// for the liminal space experience with zero external asset dependencies.
    /// </summary>
    public static class LiminalAssetFactory
    {
        private static Shader s_LitShader;
        private static Shader s_UnlitShader;

        public static Shader GetLitShader()
        {
            if (s_LitShader == null)
            {
                s_LitShader = Shader.Find("Universal Render Pipeline/Lit");
                if (s_LitShader == null) s_LitShader = Shader.Find("Standard");
                if (s_LitShader == null) s_LitShader = Shader.Find("Diffuse");
            }
            return s_LitShader;
        }

        public static Shader GetUnlitShader()
        {
            if (s_UnlitShader == null)
            {
                s_UnlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (s_UnlitShader == null) s_UnlitShader = Shader.Find("Unlit/Color");
                if (s_UnlitShader == null) s_UnlitShader = Shader.Find("Diffuse");
            }
            return s_UnlitShader;
        }

        public static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        public static Material CreateMaterial(Color color, float smoothness = 0.2f, float metallic = 0.0f, Texture2D mainTex = null, Color? emissionColor = null)
        {
            Material mat = new Material(GetLitShader());
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mainTex != null)
            {
                mat.mainTexture = mainTex;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            }
            if (emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor.Value);
            }
            return mat;
        }

        public static Material CreateEmissiveUnlit(Color color)
        {
            Material mat = new Material(GetUnlitShader());
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            return mat;
        }

        // ==================== PROCEDURAL TEXTURES ====================

                public static Texture2D CreateGrassTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseGreen = new Color(0.14f, 0.44f, 0.14f);
            Color brightGreen = new Color(0.32f, 0.70f, 0.24f);
            Color darkGreen = new Color(0.06f, 0.24f, 0.08f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;
                    float macro = Mathf.PerlinNoise(nx * 10f, ny * 10f);
                    float blade = Mathf.PerlinNoise(nx * 95f, ny * 25f);
                    float n = macro * 0.6f + blade * 0.4f;
                    Color c = Color.Lerp(darkGreen, baseGreen, n);
                    if (n > 0.6f) c = Color.Lerp(c, brightGreen, (n - 0.6f) * 2.2f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreateGlitterSandTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            System.Random rand = new System.Random(777);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float baseTone = 0.86f + (float)rand.NextDouble() * 0.10f;
                    Color c = new Color(baseTone, baseTone * 0.98f, baseTone * 1.04f);
                    if (rand.NextDouble() > 0.94)
                    {
                        c = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreateYellowWallpaperTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color yellowBase = new Color(0.84f, 0.77f, 0.44f);
            Color stripeDark = new Color(0.74f, 0.65f, 0.34f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float stripe = Mathf.Sin(x * 0.35f) * 0.5f + 0.5f;
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.22f;
                    float grime = (y < 24) ? (1f - y / 24f) * 0.25f : 0f;
                    Color c = Color.Lerp(yellowBase, stripeDark, stripe * 0.35f + noise + grime);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreateCarpetTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            System.Random rand = new System.Random(303);
            Color baseCol = new Color(0.36f, 0.33f, 0.25f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float weave = ((x + y) % 4 == 0) ? 0.05f : -0.03f;
                    float speck = (float)(rand.NextDouble() * 0.08 - 0.04);
                    Color c = new Color(
                        Mathf.Clamp01(baseCol.r + weave + speck),
                        Mathf.Clamp01(baseCol.g + weave + speck),
                        Mathf.Clamp01(baseCol.b + weave + speck));
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreatePoolTileTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color tileColor = new Color(0.93f, 0.97f, 0.99f);
            Color groutColor = new Color(0.38f, 0.68f, 0.78f);
            int tileSize = 32;
            int groutWidth = 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int mx = x % tileSize;
                    int my = y % tileSize;
                    if (mx < groutWidth || my < groutWidth)
                        tex.SetPixel(x, y, groutColor);
                    else
                    {
                        float subtle = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.04f;
                        tex.SetPixel(x, y, new Color(tileColor.r - subtle, tileColor.g - subtle, tileColor.b));
                    }
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreateWoodTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color woodBase = new Color(0.44f, 0.28f, 0.15f);
            Color woodDark = new Color(0.24f, 0.14f, 0.07f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float grain = Mathf.Sin((x * 0.35f) + Mathf.PerlinNoise(x * 0.05f, y * 0.25f) * 10f) * 0.5f + 0.5f;
                    Color c = Color.Lerp(woodBase, woodDark, grain * 0.55f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreateBandedPlanetTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color[] palette = {
                new Color(0.72f, 0.65f, 0.88f),
                new Color(0.88f, 0.70f, 0.76f),
                new Color(0.60f, 0.80f, 0.88f),
                new Color(0.55f, 0.58f, 0.78f),
                new Color(0.95f, 0.92f, 0.88f)
            };

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / size;
                float band = Mathf.Sin(ny * 24f + Mathf.PerlinNoise(ny * 6f, 0.5f) * 4f) * 0.5f + 0.5f;
                int pIndex = Mathf.Clamp((int)(ny * palette.Length), 0, palette.Length - 1);
                int nextIndex = (pIndex + 1) % palette.Length;
                Color baseC = Color.Lerp(palette[pIndex], palette[nextIndex], band);

                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float swirl = Mathf.PerlinNoise(nx * 12f, ny * 18f) * 0.12f;
                    Color c = Color.Lerp(baseC, Color.white, swirl);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreateMoonTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color moonBase = new Color(0.85f, 0.88f, 0.94f);
            Color craterColor = new Color(0.68f, 0.72f, 0.82f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;
                    float n = Mathf.PerlinNoise(nx * 10f, ny * 10f) * 0.7f + Mathf.PerlinNoise(nx * 32f, ny * 32f) * 0.3f;
                    Color c = Color.Lerp(craterColor, moonBase, n);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreatePlanetaryRingTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, 16, TextureFormat.RGBA32, false);
            Color ringColor = new Color(0.92f, 0.88f, 0.98f);

            for (int x = 0; x < size; x++)
            {
                float nx = (float)x / size;
                float ringPattern = Mathf.Sin(nx * 35f) * 0.4f + Mathf.Cos(nx * 85f) * 0.25f + 0.35f;
                float edgeFade = Mathf.Sin(nx * Mathf.PI);
                float alpha = Mathf.Clamp01(ringPattern * edgeFade * 0.82f);
                Color c = new Color(ringColor.r, ringColor.g, ringColor.b, alpha);
                for (int y = 0; y < 16; y++)
                {
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        public static Texture2D CreateLuminousGrassTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color deepCyan = new Color(0.06f, 0.28f, 0.32f);
            Color etherealGreen = new Color(0.20f, 0.62f, 0.50f);
            Color lumGlow = new Color(0.58f, 0.92f, 0.88f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;
                    float macro = Mathf.PerlinNoise(nx * 8f, ny * 8f);
                    float blade = Mathf.PerlinNoise(nx * 85f, ny * 22f);
                    float n = macro * 0.55f + blade * 0.45f;
                    Color c = Color.Lerp(deepCyan, etherealGreen, n);
                    if (n > 0.65f)
                    {
                        c = Color.Lerp(c, lumGlow, (n - 0.65f) * 2.2f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CreateCrystallineWaterTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color deepWater = new Color(0.12f, 0.56f, 0.72f, 0.78f);
            Color lightCaustic = new Color(0.72f, 0.94f, 0.98f, 0.90f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;
                    float c1 = Mathf.PerlinNoise(nx * 14f, ny * 14f);
                    float c2 = Mathf.PerlinNoise((nx + 0.3f) * 20f, (ny + 0.3f) * 20f);
                    float caustic = Mathf.Pow(c1 * c2, 1.8f) * 2.6f;
                    Color c = Color.Lerp(deepWater, lightCaustic, Mathf.Clamp01(caustic));
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        // ==================== TERRAIN & PARTICLES ====================

        /// <summary>
        public static GameObject BuildRollingGreenField(Material grassMat)
        {
            GameObject obj = new GameObject("ShinyGreenField");
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshCollider mc = obj.AddComponent<MeshCollider>();

            int res = 64;
            float size = 150f;
            float half = size * 0.5f;

            Vector3[] verts = new Vector3[(res + 1) * (res + 1)];
            Vector2[] uvs = new Vector2[verts.Length];
            int[] tris = new int[res * res * 6];

            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++)
                {
                    float px = ((float)x / res) * size - half;
                    float pz = ((float)z / res) * size - half;

                    // Keep central garden zone (near path/table/bed) flat for clean prop placement
                    float distFromCenter = new Vector2(px, pz - 16f).magnitude;
                    float hillFactor = Mathf.Clamp01((distFromCenter - 22f) / 35f);
                    float y = (Mathf.PerlinNoise(px * 0.045f + 10f, pz * 0.045f + 10f) * 6.5f) * hillFactor;

                    int idx = z * (res + 1) + x;
                    verts[idx] = new Vector3(px, y, pz);
                    uvs[idx] = new Vector2(px * 0.22f, pz * 0.22f);
                }
            }

            int t = 0;
            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    int i0 = z * (res + 1) + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + (res + 1);
                    int i3 = i2 + 1;

                    tris[t++] = i0; tris[t++] = i2; tris[t++] = i1;
                    tris[t++] = i1; tris[t++] = i2; tris[t++] = i3;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "RollingFieldMesh";
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();

            mf.sharedMesh = mesh;
            mr.sharedMaterial = grassMat;
            mc.sharedMesh = mesh;

            return obj;
        }

        /// <summary>
        /// Builds drifting emerald & golden firefly particles over the night meadow.
        /// </summary>
        public static GameObject BuildFireflyParticles(Color particleColor, int maxCount = 180)
        {
            GameObject go = new GameObject("AmbientFireflies");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 6.5f;
            main.startSpeed = 0.28f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
            main.maxParticles = maxCount;
            main.startColor = particleColor;

            var emission = ps.emission;
            emission.rateOverTime = 24f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(38f, 3.2f, 44f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.45f;
            noise.frequency = 0.5f;

            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(particleColor, 0f), new GradientColorKey(particleColor, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.95f, 0.4f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLife.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = CreateEmissiveUnlit(particleColor * 2.2f);

            return go;
        }

        // ==================== PROCEDURAL MESHES & MODELS ====================

        public static GameObject BuildTallSilentMan(bool hollowGaze = false)
        {
            GameObject root = new GameObject("TallSilentMan");

            Material bodyMat = CreateMaterial(new Color(0.03f, 0.03f, 0.035f), 0.18f, 0.05f);
            Material eyeMat = CreateEmissiveUnlit(new Color(1.4f, 1.4f, 1.5f)); // HDR glow for Bloom
            Material pupilMat = CreateMaterial(Color.black, 0.9f);

            // Neck
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Neck";
            neck.transform.SetParent(root.transform);
            neck.transform.localPosition = new Vector3(0, 2.52f, 0);
            neck.transform.localScale = new Vector3(0.11f, 0.16f, 0.11f);
            neck.GetComponent<Renderer>().sharedMaterial = bodyMat;
            SafeDestroy(neck.GetComponent<Collider>());

            // Torso (tall, slender)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform);
            torso.transform.localPosition = new Vector3(0, 1.82f, 0);
            torso.transform.localScale = new Vector3(0.34f, 0.85f, 0.22f);
            torso.GetComponent<Renderer>().sharedMaterial = bodyMat;
            SafeDestroy(torso.GetComponent<Collider>());

            // Shoulders bar
            GameObject shoulders = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shoulders.name = "Shoulders";
            shoulders.transform.SetParent(root.transform);
            shoulders.transform.localPosition = new Vector3(0, 2.38f, 0);
            shoulders.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            shoulders.transform.localScale = new Vector3(0.18f, 0.38f, 0.18f);
            shoulders.GetComponent<Renderer>().sharedMaterial = bodyMat;
            SafeDestroy(shoulders.GetComponent<Collider>());

            // Diamond / Inverted Pyramid Head
            GameObject head = CreateDiamondHead(bodyMat);
            head.name = "DiamondHead";
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0, 2.86f, 0);
            head.transform.localScale = new Vector3(0.62f, 0.68f, 0.56f);

            // Eyes (omitted if HollowGaze anomaly)
            if (!hollowGaze)
            {
                float eyeX = 0.12f;
                float eyeY = 2.88f;
                float eyeZ = 0.24f;

                for (int i = 0; i < 2; i++)
                {
                    float sign = (i == 0) ? -1f : 1f;
                    GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    eye.name = (i == 0) ? "LeftEye" : "RightEye";
                    eye.transform.SetParent(root.transform);
                    eye.transform.localPosition = new Vector3(sign * eyeX, eyeY, eyeZ);
                    eye.transform.localScale = new Vector3(0.095f, 0.095f, 0.045f);
                    eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                    SafeDestroy(eye.GetComponent<Collider>());

                    GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    pupil.name = "Pupil";
                    pupil.transform.SetParent(eye.transform);
                    pupil.transform.localPosition = new Vector3(0, 0, 0.48f);
                    pupil.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                    pupil.GetComponent<Renderer>().sharedMaterial = pupilMat;
                    SafeDestroy(pupil.GetComponent<Collider>());
                }
            }

            // Left & Right Arms (elongated)
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(root.transform);
            leftArm.transform.localPosition = new Vector3(-0.37f, 1.55f, 0);
            leftArm.transform.localScale = new Vector3(0.10f, 0.85f, 0.10f);
            leftArm.GetComponent<Renderer>().sharedMaterial = bodyMat;
            SafeDestroy(leftArm.GetComponent<Collider>());

            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(root.transform);
            rightArm.transform.localPosition = new Vector3(0.37f, 1.55f, 0);
            rightArm.transform.localScale = new Vector3(0.10f, 0.85f, 0.10f);
            rightArm.GetComponent<Renderer>().sharedMaterial = bodyMat;
            SafeDestroy(rightArm.GetComponent<Collider>());

            // Left & Right Legs (elongated)
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(root.transform);
            leftLeg.transform.localPosition = new Vector3(-0.15f, 0.62f, 0);
            leftLeg.transform.localScale = new Vector3(0.13f, 0.78f, 0.13f);
            leftLeg.GetComponent<Renderer>().sharedMaterial = bodyMat;
            SafeDestroy(leftLeg.GetComponent<Collider>());

            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(root.transform);
            rightLeg.transform.localPosition = new Vector3(0.15f, 0.62f, 0);
            rightLeg.transform.localScale = new Vector3(0.13f, 0.78f, 0.13f);
            rightLeg.GetComponent<Renderer>().sharedMaterial = bodyMat;
            SafeDestroy(rightLeg.GetComponent<Collider>());

            CapsuleCollider col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 1.5f, 0);
            col.radius = 0.4f;
            col.height = 3.0f;

            return root;
        }

        private static GameObject CreateDiamondHead(Material mat)
        {
            GameObject obj = new GameObject("DiamondMesh");
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            Mesh mesh = new Mesh();
            mesh.name = "DiamondHeadMesh";

            Vector3 top = new Vector3(0, 0.52f, 0);
            Vector3 bottom = new Vector3(0, -0.48f, 0);
            Vector3 front = new Vector3(0, 0.06f, 0.46f);
            Vector3 back = new Vector3(0, 0.06f, -0.36f);
            Vector3 left = new Vector3(-0.46f, 0.06f, 0);
            Vector3 right = new Vector3(0.46f, 0.06f, 0);

            Vector3[] vertices = new Vector3[]
            {
                top, front, right,
                top, right, back,
                top, back, left,
                top, left, front,
                bottom, right, front,
                bottom, back, right,
                bottom, left, back,
                bottom, front, left
            };

            int[] triangles = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) triangles[i] = i;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mf.sharedMesh = mesh;

            return obj;
        }

        public static GameObject BuildWhiteBed()
        {
            GameObject root = new GameObject("WhiteQuietBed");

            Material sheetMat = CreateMaterial(new Color(0.97f, 0.97f, 1.0f), 0.2f, 0f, null, new Color(0.06f, 0.06f, 0.08f));
            Material pillowMat = CreateMaterial(Color.white, 0.15f, 0f, null, new Color(0.08f, 0.08f, 0.10f));
            Material frameMat = CreateMaterial(new Color(0.22f, 0.16f, 0.12f), 0.45f);

            // Legs
            float[] lx = { -0.95f, 0.95f, -0.95f, 0.95f };
            float[] lz = { -1.05f, -1.05f, 1.05f, 1.05f };
            for (int i = 0; i < 4; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.transform.SetParent(root.transform);
                leg.transform.localPosition = new Vector3(lx[i], 0.12f, lz[i]);
                leg.transform.localScale = new Vector3(0.1f, 0.12f, 0.1f);
                leg.GetComponent<Renderer>().sharedMaterial = frameMat;
                SafeDestroy(leg.GetComponent<Collider>());
            }

            // Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "BedFrame";
            frame.transform.SetParent(root.transform);
            frame.transform.localPosition = new Vector3(0, 0.25f, 0);
            frame.transform.localScale = new Vector3(2.1f, 0.15f, 2.35f);
            frame.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Mattress
            GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mattress.name = "Mattress";
            mattress.transform.SetParent(root.transform);
            mattress.transform.localPosition = new Vector3(0, 0.46f, 0);
            mattress.transform.localScale = new Vector3(2.0f, 0.32f, 2.25f);
            mattress.GetComponent<Renderer>().sharedMaterial = sheetMat;

            // Draped Blanket
            GameObject duvet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            duvet.name = "Duvet";
            duvet.transform.SetParent(root.transform);
            duvet.transform.localPosition = new Vector3(0, 0.56f, -0.22f);
            duvet.transform.localScale = new Vector3(2.08f, 0.20f, 1.55f);
            duvet.GetComponent<Renderer>().sharedMaterial = sheetMat;

            // Rounded Pillows
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject pillow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pillow.name = "Pillow_" + (i < 0 ? "Left" : "Right");
                pillow.transform.SetParent(root.transform);
                pillow.transform.localPosition = new Vector3(i * 0.48f, 0.66f, 0.78f);
                pillow.transform.localScale = new Vector3(0.75f, 0.22f, 0.50f);
                pillow.transform.localRotation = Quaternion.Euler(14f, 0, 0);
                pillow.GetComponent<Renderer>().sharedMaterial = pillowMat;
            }

            // Headboard
            GameObject headboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headboard.name = "Headboard";
            headboard.transform.SetParent(root.transform);
            headboard.transform.localPosition = new Vector3(0, 0.85f, 1.16f);
            headboard.transform.localScale = new Vector3(2.15f, 0.95f, 0.12f);
            headboard.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Subtle soothing glow over bed
            GameObject bedLight = new GameObject("BedSerenityLight");
            bedLight.transform.SetParent(root.transform);
            bedLight.transform.localPosition = new Vector3(0, 2.6f, 0);
            Light bl = bedLight.AddComponent<Light>();
            bl.type = LightType.Point;
            bl.color = new Color(0.82f, 0.90f, 1.0f);
            bl.intensity = 1.6f;
            bl.range = 6.5f;

            return root;
        }

        public static GameObject BuildDiningTableWithFood(bool whiteFlowers = false)
        {
            GameObject root = new GameObject("DiningTableWithFood");

            Texture2D woodTex = CreateWoodTexture();
            Material woodMat = CreateMaterial(Color.white, 0.35f, 0.05f, woodTex);
            Material breadCrustMat = CreateMaterial(new Color(0.72f, 0.46f, 0.20f), 0.15f);
            Material plateMat = CreateMaterial(new Color(0.96f, 0.96f, 0.96f), 0.85f);
            Material glassMat = CreateMaterial(new Color(0.85f, 0.95f, 1.0f, 0.7f), 0.95f, 0.1f, null, new Color(0.15f, 0.28f, 0.35f));
            Color petalColor = whiteFlowers ? new Color(0.92f, 0.95f, 1.0f) : new Color(0.90f, 0.06f, 0.12f);
            Color petalGlow = whiteFlowers ? new Color(0.2f, 0.25f, 0.3f) : new Color(0.18f, 0.01f, 0.02f);
            Material roseMat = CreateMaterial(petalColor, 0.28f, 0f, null, petalGlow);
            Material stemMat = CreateMaterial(new Color(0.12f, 0.38f, 0.10f), 0.15f);

            GameObject tableTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tableTop.name = "TableTop";
            tableTop.transform.SetParent(root.transform);
            tableTop.transform.localPosition = new Vector3(0, 0.95f, 0);
            tableTop.transform.localScale = new Vector3(1.65f, 0.08f, 1.05f);
            tableTop.GetComponent<Renderer>().sharedMaterial = woodMat;

            float[] legX = { -0.72f, 0.72f, -0.72f, 0.72f };
            float[] legZ = { -0.42f, -0.42f, 0.42f, 0.42f };
            for (int i = 0; i < 4; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg_" + i;
                leg.transform.SetParent(root.transform);
                leg.transform.localPosition = new Vector3(legX[i], 0.45f, legZ[i]);
                leg.transform.localScale = new Vector3(0.08f, 0.92f, 0.08f);
                leg.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            // Chair
            GameObject chair = new GameObject("Chair");
            chair.transform.SetParent(root.transform);
            chair.transform.localPosition = new Vector3(0, 0, -0.68f);

            GameObject chairSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairSeat.transform.SetParent(chair.transform);
            chairSeat.transform.localPosition = new Vector3(0, 0.5f, 0);
            chairSeat.transform.localScale = new Vector3(0.55f, 0.06f, 0.5f);
            chairSeat.GetComponent<Renderer>().sharedMaterial = woodMat;

            GameObject chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairBack.transform.SetParent(chair.transform);
            chairBack.transform.localPosition = new Vector3(0, 0.85f, -0.22f);
            chairBack.transform.localScale = new Vector3(0.55f, 0.65f, 0.05f);
            chairBack.GetComponent<Renderer>().sharedMaterial = woodMat;

            float[] clx = { -0.23f, 0.23f, -0.23f, 0.23f };
            float[] clz = { -0.21f, -0.21f, 0.21f, 0.21f };
            for (int i = 0; i < 4; i++)
            {
                GameObject cl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cl.transform.SetParent(chair.transform);
                cl.transform.localPosition = new Vector3(clx[i], 0.25f, clz[i]);
                cl.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
                cl.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            // Plate & Bread
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "Plate";
            plate.transform.SetParent(root.transform);
            plate.transform.localPosition = new Vector3(-0.15f, 1.01f, 0.05f);
            plate.transform.localScale = new Vector3(0.38f, 0.02f, 0.38f);
            plate.GetComponent<Renderer>().sharedMaterial = plateMat;

            for (int b = 0; b < 4; b++)
            {
                GameObject slice = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slice.name = "BreadSlice_" + b;
                slice.transform.SetParent(plate.transform);
                slice.transform.localPosition = new Vector3((b - 1.5f) * 0.16f, 1.2f, 0);
                slice.transform.localScale = new Vector3(0.12f, 1.2f, 0.55f);
                slice.transform.localRotation = Quaternion.Euler(0, 12f * b, 10f);
                slice.GetComponent<Renderer>().sharedMaterial = breadCrustMat;
            }

            // Glass of Water
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glass.name = "WaterGlass";
            glass.transform.SetParent(root.transform);
            glass.transform.localPosition = new Vector3(0.28f, 1.09f, 0.08f);
            glass.transform.localScale = new Vector3(0.12f, 0.16f, 0.12f);
            glass.GetComponent<Renderer>().sharedMaterial = glassMat;

            // Roses on Table
            GameObject roseBouquet = new GameObject("TableRoses");
            roseBouquet.transform.SetParent(root.transform);
            roseBouquet.transform.localPosition = new Vector3(-0.46f, 1.02f, -0.1f);

            for (int r = 0; r < 3; r++)
            {
                GameObject roseHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                roseHead.name = "Rose_" + r;
                roseHead.transform.SetParent(roseBouquet.transform);
                roseHead.transform.localPosition = new Vector3(r * 0.08f, 0.05f, (r % 2) * 0.06f);
                roseHead.transform.localScale = new Vector3(0.085f, 0.085f, 0.085f);
                roseHead.GetComponent<Renderer>().sharedMaterial = roseMat;

                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.transform.SetParent(roseHead.transform);
                stem.transform.localPosition = new Vector3(0, -0.5f, 0);
                stem.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
                stem.GetComponent<Renderer>().sharedMaterial = stemMat;
            }

            return root;
        }

        public static GameObject BuildRoseBush(int seed = 123, bool whiteAnomaly = false)
        {
            GameObject bush = new GameObject("RoseBush");
            System.Random rand = new System.Random(seed);

            Color flowerColor = whiteAnomaly ? new Color(0.92f, 0.94f, 0.98f) : new Color(0.92f, 0.05f, 0.12f);
            Color flowerEmit = whiteAnomaly ? new Color(0.25f, 0.28f, 0.35f) : new Color(0.22f, 0.01f, 0.02f);

            Material roseMat = CreateMaterial(flowerColor, 0.32f, 0f, null, flowerEmit);
            Material leafMat = CreateMaterial(new Color(0.11f, 0.36f, 0.10f), 0.25f);

            for (int i = 0; i < 5; i++)
            {
                GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mound.name = "Leaves_" + i;
                mound.transform.SetParent(bush.transform);
                float x = (float)(rand.NextDouble() * 0.55 - 0.275);
                float y = (float)(rand.NextDouble() * 0.32 + 0.28);
                float z = (float)(rand.NextDouble() * 0.55 - 0.275);
                mound.transform.localPosition = new Vector3(x, y, z);
                mound.transform.localScale = new Vector3(0.62f, 0.54f, 0.62f);
                mound.GetComponent<Renderer>().sharedMaterial = leafMat;
                SafeDestroy(mound.GetComponent<Collider>());
            }

            int flowerCount = 8 + rand.Next(5);
            for (int f = 0; f < flowerCount; f++)
            {
                GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = "FlowerBloom_" + f;
                flower.transform.SetParent(bush.transform);
                float fx = (float)(rand.NextDouble() * 0.85 - 0.425);
                float fy = (float)(rand.NextDouble() * 0.45 + 0.42);
                float fz = (float)(rand.NextDouble() * 0.85 - 0.425);
                flower.transform.localPosition = new Vector3(fx, fy, fz);
                flower.transform.localScale = new Vector3(0.15f, 0.13f, 0.15f);
                flower.GetComponent<Renderer>().sharedMaterial = roseMat;
                SafeDestroy(flower.GetComponent<Collider>());
            }

            return bush;
        }

        public static GameObject BuildLanternPost()
        {
            GameObject lantern = new GameObject("LanternPost");

            Material ironMat = CreateMaterial(new Color(0.14f, 0.14f, 0.16f), 0.65f, 0.85f);
            Color warmGold = new Color(1.4f, 1.05f, 0.52f);
            Material glowMat = CreateEmissiveUnlit(warmGold);

            GameObject basePed = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePed.transform.SetParent(lantern.transform);
            basePed.transform.localPosition = new Vector3(0, 0.06f, 0);
            basePed.transform.localScale = new Vector3(0.22f, 0.06f, 0.22f);
            basePed.GetComponent<Renderer>().sharedMaterial = ironMat;

            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.SetParent(lantern.transform);
            post.transform.localPosition = new Vector3(0, 0.4f, 0);
            post.transform.localScale = new Vector3(0.07f, 0.35f, 0.07f);
            post.GetComponent<Renderer>().sharedMaterial = ironMat;

            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.transform.SetParent(lantern.transform);
            cap.transform.localPosition = new Vector3(0, 0.92f, 0);
            cap.transform.localScale = new Vector3(0.24f, 0.04f, 0.24f);
            cap.GetComponent<Renderer>().sharedMaterial = ironMat;

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Core";
            core.transform.SetParent(lantern.transform);
            core.transform.localPosition = new Vector3(0, 0.78f, 0);
            core.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
            core.GetComponent<Renderer>().sharedMaterial = glowMat;

            GameObject lightObj = new GameObject("WarmLight");
            lightObj.transform.SetParent(lantern.transform);
            lightObj.transform.localPosition = new Vector3(0, 0.80f, 0);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.78f, 0.40f);
            l.intensity = 2.8f;
            l.range = 8.5f;
            l.shadows = LightShadows.Soft;

            return lantern;
        }

        public static GameObject BuildDoorPortal(string doorName, Color frameColor, string signText, bool showHeaderPlaque = true)
        {
            GameObject root = new GameObject(doorName);

            Material frameMat = CreateMaterial(frameColor, 0.75f, 0.15f);
            Material doorLeafMat = CreateMaterial(new Color(0.95f, 0.95f, 0.97f), 0.45f);
            Material insetMat = CreateMaterial(new Color(0.84f, 0.85f, 0.88f), 0.35f);
            Material handleMat = CreateMaterial(new Color(0.92f, 0.78f, 0.28f), 0.92f, 0.9f);
            Material glowStripMat = CreateEmissiveUnlit(frameColor * 1.35f);

            // Posts & Header
            GameObject leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPost.name = "LeftPost";
            leftPost.transform.SetParent(root.transform);
            leftPost.transform.localPosition = new Vector3(-0.82f, 1.4f, 0);
            leftPost.transform.localScale = new Vector3(0.16f, 2.8f, 0.28f);
            leftPost.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPost.name = "RightPost";
            rightPost.transform.SetParent(root.transform);
            rightPost.transform.localPosition = new Vector3(0.82f, 1.4f, 0);
            rightPost.transform.localScale = new Vector3(0.16f, 2.8f, 0.28f);
            rightPost.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject header = GameObject.CreatePrimitive(PrimitiveType.Cube);
            header.name = "Header";
            header.transform.SetParent(root.transform);
            header.transform.localPosition = new Vector3(0, 2.78f, 0);
            header.transform.localScale = new Vector3(1.80f, 0.22f, 0.28f);
            header.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Subtle Glowing Indicator Strip above door
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "GlowStrip";
            strip.transform.SetParent(root.transform);
            strip.transform.localPosition = new Vector3(0, 2.92f, -0.02f);
            strip.transform.localScale = new Vector3(1.2f, 0.04f, 0.16f);
            strip.GetComponent<Renderer>().sharedMaterial = glowStripMat;
            SafeDestroy(strip.GetComponent<Collider>());

            // Door Leaf Pivot & Sculpted Leaf
            GameObject pivot = new GameObject("DoorPivot");
            pivot.transform.SetParent(root.transform);
            pivot.transform.localPosition = new Vector3(-0.74f, 0, 0);

            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.name = "DoorLeaf";
            leaf.transform.SetParent(pivot.transform);
            leaf.transform.localPosition = new Vector3(0.74f, 1.35f, 0);
            leaf.transform.localScale = new Vector3(1.48f, 2.68f, 0.08f);
            leaf.GetComponent<Renderer>().sharedMaterial = doorLeafMat;

            // Recessed door panels
            for (int p = 0; p < 2; p++)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.transform.SetParent(leaf.transform);
                panel.transform.localPosition = new Vector3(0, p == 0 ? 0.22f : -0.22f, 0);
                panel.transform.localScale = new Vector3(0.72f, 0.34f, 1.18f);
                panel.GetComponent<Renderer>().sharedMaterial = insetMat;
                SafeDestroy(panel.GetComponent<Collider>());
            }

            // Brass Handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "Handle";
            handle.transform.SetParent(leaf.transform);
            handle.transform.localPosition = new Vector3(0.38f, -0.04f, 0.0f);
            handle.transform.localScale = new Vector3(0.06f, 0.06f, 1.8f);
            handle.GetComponent<Renderer>().sharedMaterial = handleMat;
            SafeDestroy(handle.GetComponent<Collider>());

            // 3D World Plaque Text
            if (showHeaderPlaque && !string.IsNullOrEmpty(signText))
            {
                GameObject textObj = new GameObject("DoorPlaqueText");
                textObj.transform.SetParent(root.transform);
                textObj.transform.localPosition = new Vector3(0, 3.12f, -0.15f);
                textObj.transform.localRotation = Quaternion.identity;

                TextMesh tm = textObj.AddComponent<TextMesh>();
                tm.text = signText;
                tm.fontSize = 36;
                tm.characterSize = 0.055f;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = Color.Lerp(frameColor, Color.white, 0.5f);
            }

            return root;
        }

        /// <summary>
        /// Builds a glowing floating Cosmic Relic pickup for players to hunt across realms.
        /// </summary>
        public static GameObject BuildLiminalRelic()
        {
            GameObject relic = new GameObject("CosmicRelic");

            Material gemMat = CreateEmissiveUnlit(new Color(0.55f, 1.35f, 1.55f));
            GameObject core = CreateDiamondHead(gemMat);
            core.name = "RelicCrystal";
            core.transform.SetParent(relic.transform);
            core.transform.localPosition = new Vector3(0, 1.25f, 0);
            core.transform.localScale = new Vector3(0.42f, 0.55f, 0.42f);

            GameObject lightObj = new GameObject("RelicLight");
            lightObj.transform.SetParent(relic.transform);
            lightObj.transform.localPosition = new Vector3(0, 1.25f, 0);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.45f, 0.95f, 1.0f);
            l.intensity = 2.4f;
            l.range = 6.0f;

            SphereCollider sc = relic.AddComponent<SphereCollider>();
            sc.center = new Vector3(0, 1.25f, 0);
            sc.radius = 0.9f;

            return relic;
        }

        /// <summary>
        /// Builds the luminous Golden Proceed Archway that appears after surviving 48 hours.
        /// </summary>
        public static GameObject BuildProceedArchway()
        {
            GameObject arch = new GameObject("ProceedArchway");

            Material goldMat = CreateEmissiveUnlit(new Color(1.35f, 1.08f, 0.48f));
            Material pillarMat = CreateMaterial(new Color(0.95f, 0.90f, 0.78f), 0.75f, 0.3f);

            GameObject lp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lp.transform.SetParent(arch.transform);
            lp.transform.localPosition = new Vector3(-0.95f, 1.5f, 0);
            lp.transform.localScale = new Vector3(0.22f, 3.0f, 0.28f);
            lp.GetComponent<Renderer>().sharedMaterial = pillarMat;

            GameObject rp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rp.transform.SetParent(arch.transform);
            rp.transform.localPosition = new Vector3(0.95f, 1.5f, 0);
            rp.transform.localScale = new Vector3(0.22f, 3.0f, 0.28f);
            rp.GetComponent<Renderer>().sharedMaterial = pillarMat;

            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.transform.SetParent(arch.transform);
            top.transform.localPosition = new Vector3(0, 3.05f, 0);
            top.transform.localScale = new Vector3(2.2f, 0.25f, 0.32f);
            top.GetComponent<Renderer>().sharedMaterial = pillarMat;

            GameObject curtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curtain.name = "PortalCurtain";
            curtain.transform.SetParent(arch.transform);
            curtain.transform.localPosition = new Vector3(0, 1.5f, 0);
            curtain.transform.localScale = new Vector3(1.65f, 2.85f, 0.04f);
            curtain.GetComponent<Renderer>().sharedMaterial = goldMat;
            SafeDestroy(curtain.GetComponent<Collider>());

            GameObject lObj = new GameObject("PortalLight");
            lObj.transform.SetParent(arch.transform);
            lObj.transform.localPosition = new Vector3(0, 1.6f, 0);
            Light l = lObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.85f, 0.45f);
            l.intensity = 3.2f;
            l.range = 10f;

            return arch;
        }

        // ==================== CELESTIAL & ETHEREAL SURREALISM ASSETS ====================

        public static GameObject BuildGiganticRingedPlanet(Vector3 localPos, float radius)
        {
            GameObject root = new GameObject("GiganticRingedPlanet");
            root.transform.localPosition = localPos;

            // Colossal Planet Sphere
            GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "PlanetSphere";
            planet.transform.SetParent(root.transform, false);
            planet.transform.localScale = Vector3.one * (radius * 2f);
            SafeDestroy(planet.GetComponent<Collider>());

            Material planetMat = CreateMaterial(Color.white, 0.25f, 0f, CreateBandedPlanetTexture(), new Color(0.32f, 0.25f, 0.44f) * 0.5f);
            planet.GetComponent<Renderer>().sharedMaterial = planetMat;

            // Planetary Rings Disk
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "PlanetaryRings";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localScale = new Vector3(radius * 4.6f, 0.05f, radius * 4.6f);
            ring.transform.localRotation = Quaternion.Euler(28f, 40f, -14f);
            SafeDestroy(ring.GetComponent<Collider>());

            Material ringMat = CreateMaterial(Color.white, 0.55f, 0f, CreatePlanetaryRingTexture(), new Color(0.78f, 0.68f, 0.94f) * 0.85f);
            ring.GetComponent<Renderer>().sharedMaterial = ringMat;

            // Atmospheric Radiance Light
            GameObject halo = new GameObject("AtmosphericRadiance");
            halo.transform.SetParent(root.transform, false);
            Light l = halo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.85f, 0.78f, 1.0f);
            l.intensity = 5.0f;
            l.range = radius * 3.5f;

            return root;
        }

        public static GameObject BuildLuminousMoon(Vector3 localPos, float radius)
        {
            GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moon.name = "LuminousMoon";
            moon.transform.localPosition = localPos;
            moon.transform.localScale = Vector3.one * (radius * 2f);
            SafeDestroy(moon.GetComponent<Collider>());

            Material moonMat = CreateMaterial(Color.white, 0.35f, 0f, CreateMoonTexture(), new Color(0.70f, 0.76f, 0.95f) * 0.95f);
            moon.GetComponent<Renderer>().sharedMaterial = moonMat;

            GameObject glow = new GameObject("MoonGlowLight");
            glow.transform.SetParent(moon.transform, false);
            Light l = glow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.82f, 0.88f, 1.0f);
            l.intensity = 3.8f;
            l.range = radius * 4.0f;

            return moon;
        }

        public static GameObject BuildCelestialMonumentArch(Vector3 localPos, float radius, float thickness)
        {
            GameObject root = new GameObject("CelestialMonumentArch");
            root.transform.localPosition = localPos;

            Material archMat = CreateMaterial(new Color(0.92f, 0.90f, 0.96f), 0.85f, 0.15f, null, new Color(0.70f, 0.65f, 0.95f) * 0.8f);
            int segments = 22;
            float stepAngle = 180f / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * stepAngle * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "ArchSegment_" + i;
                seg.transform.SetParent(root.transform, false);
                seg.transform.localPosition = new Vector3(x, y, 0);
                float chord = (2f * Mathf.PI * radius / (segments * 2f)) * 1.12f;
                seg.transform.localScale = new Vector3(chord, thickness, thickness * 1.35f);
                seg.transform.localRotation = Quaternion.Euler(0, 0, (i * stepAngle) - 90f);
                seg.GetComponent<Renderer>().sharedMaterial = archMat;
                SafeDestroy(seg.GetComponent<Collider>());
            }

            return root;
        }

        public static GameObject BuildFloatingIsland(Vector3 localPos, Vector3 scale, Material topMat, Material rockMat)
        {
            GameObject island = new GameObject("FloatingIsland");
            island.transform.localPosition = localPos;
            island.transform.localScale = scale;

            // Top Plateau
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.name = "PlateauTop";
            top.transform.SetParent(island.transform, false);
            top.transform.localPosition = new Vector3(0, 0.15f, 0);
            top.transform.localScale = new Vector3(1.0f, 0.25f, 1.0f);
            top.GetComponent<Renderer>().sharedMaterial = topMat;
            SafeDestroy(top.GetComponent<Collider>());

            // Rocky Underbelly
            GameObject baseRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            baseRock.name = "IslandUnderbelly";
            baseRock.transform.SetParent(island.transform, false);
            baseRock.transform.localPosition = new Vector3(0, -0.45f, 0);
            baseRock.transform.localScale = new Vector3(0.92f, 1.25f, 0.92f);
            baseRock.GetComponent<Renderer>().sharedMaterial = rockMat;
            SafeDestroy(baseRock.GetComponent<Collider>());

            // Hanging luminous crystal stalactites
            System.Random rand = new System.Random((int)Mathf.Abs(localPos.x * 13f + localPos.z));
            Material crystalMat = CreateEmissiveUnlit(new Color(0.75f, 0.90f, 1.0f) * 2.0f);
            for (int i = 0; i < 5; i++)
            {
                float ox = (float)(rand.NextDouble() * 0.6 - 0.3);
                float oz = (float)(rand.NextDouble() * 0.6 - 0.3);
                float len = (float)(rand.NextDouble() * 0.7 + 0.5);

                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crystal.name = "Stalactite_" + i;
                crystal.transform.SetParent(island.transform, false);
                crystal.transform.localPosition = new Vector3(ox, -1.05f - len * 0.5f, oz);
                crystal.transform.localScale = new Vector3(0.06f, len * 0.5f, 0.06f);
                crystal.GetComponent<Renderer>().sharedMaterial = crystalMat;
                SafeDestroy(crystal.GetComponent<Collider>());
            }

            return island;
        }

        public static GameObject BuildLuminousWaterfall(Vector3 topPos, float height, float width, Material waterMat)
        {
            GameObject fall = new GameObject("LuminousWaterfall");
            fall.transform.localPosition = topPos;

            GameObject curtain = GameObject.CreatePrimitive(PrimitiveType.Quad);
            curtain.name = "WaterfallSheet";
            curtain.transform.SetParent(fall.transform, false);
            curtain.transform.localPosition = new Vector3(0, -height * 0.5f, 0);
            curtain.transform.localScale = new Vector3(width, height, 1f);
            curtain.GetComponent<Renderer>().sharedMaterial = waterMat;
            SafeDestroy(curtain.GetComponent<Collider>());

            GameObject mistLight = new GameObject("WaterfallMistLight");
            mistLight.transform.SetParent(fall.transform, false);
            mistLight.transform.localPosition = new Vector3(0, -height + 0.6f, 0.4f);
            Light l = mistLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.68f, 0.94f, 1.0f);
            l.intensity = 2.4f;
            l.range = width * 2.6f;

            return fall;
        }

        public static GameObject BuildBioluminescentAlienFlower(int seed, Color petalColor, Color centerGlowColor)
        {
            GameObject flower = new GameObject("BioluminescentAlienFlower");

            Material stemMat = CreateMaterial(new Color(0.12f, 0.42f, 0.35f), 0.35f);
            Material petalMat = CreateMaterial(petalColor, 0.85f, 0.05f, null, petalColor * 1.7f);
            Material centerMat = CreateEmissiveUnlit(centerGlowColor * 2.6f);

            // Stem
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(flower.transform, false);
            stem.transform.localPosition = new Vector3(0, 0.45f, 0);
            stem.transform.localScale = new Vector3(0.045f, 0.45f, 0.045f);
            stem.GetComponent<Renderer>().sharedMaterial = stemMat;
            SafeDestroy(stem.GetComponent<Collider>());

            // Glowing Center Bud
            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.name = "GlowCenter";
            center.transform.SetParent(flower.transform, false);
            center.transform.localPosition = new Vector3(0, 0.92f, 0);
            center.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
            center.GetComponent<Renderer>().sharedMaterial = centerMat;
            SafeDestroy(center.GetComponent<Collider>());

            Light pl = center.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = centerGlowColor;
            pl.intensity = 0.9f;
            pl.range = 2.8f;

            // Layered Petals
            int petalCount = 6;
            for (int i = 0; i < petalCount; i++)
            {
                float angle = i * (360f / petalCount);
                GameObject petal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                petal.name = "Petal_" + i;
                petal.transform.SetParent(flower.transform, false);
                petal.transform.localPosition = new Vector3(0, 0.88f, 0);
                petal.transform.localRotation = Quaternion.Euler(32f, angle, 0);
                petal.transform.Translate(Vector3.forward * 0.18f, Space.Self);
                petal.transform.localScale = new Vector3(0.12f, 0.04f, 0.24f);
                petal.GetComponent<Renderer>().sharedMaterial = petalMat;
                SafeDestroy(petal.GetComponent<Collider>());
            }

            return flower;
        }

        public static GameObject BuildStardustPollenParticles(Color color, int maxCount = 220)
        {
            GameObject go = new GameObject("StardustPollenParticles");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 8.5f;
            main.startSpeed = 0.22f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.16f);
            main.maxParticles = maxCount;
            main.startColor = color;

            var emission = ps.emission;
            emission.rateOverTime = 26f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(55f, 4.5f, 65f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.35f;
            noise.frequency = 0.4f;

            var velOverLife = ps.velocityOverLifetime;
            velOverLife.enabled = true;
            velOverLife.y = 0.18f;

            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.95f, 0.4f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLife.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = CreateEmissiveUnlit(color * 2.4f);

            return go;
        }
    }
}
