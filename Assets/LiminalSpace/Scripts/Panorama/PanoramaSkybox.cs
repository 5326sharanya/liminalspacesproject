using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Puts a single 360 panorama behind the whole scene by assigning an equirectangular
    /// texture to <see cref="RenderSettings.skybox"/>. Works in the editor and in a build.
    ///
    /// Drop any equirectangular photo, or a 360 <see cref="RenderTexture"/> fed by a
    /// VideoPlayer, into the Panorama slot; with nothing assigned it builds a cheap
    /// procedural placeholder so the scene is never empty. Call
    /// <see cref="SetPanorama"/> at runtime to swap between panoramas.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class PanoramaSkybox : MonoBehaviour
    {
        [Header("Panorama")]
        [Tooltip("Equirectangular (2:1) texture. Leave empty to use the generated placeholder.")]
        [SerializeField] private Texture m_Panorama;
        [Tooltip("Optional material asset. Created at runtime if left empty.")]
        [SerializeField] private Material m_SkyboxMaterial;
        [Range(0f, 360f)]
        [SerializeField] private float m_Rotation = 0f;
        [Range(0f, 4f)]
        [SerializeField] private float m_Exposure = 1f;
        [Tooltip("How much of the sky the photo really covers, top to bottom, in degrees.")]
        [Range(10f, 120f)]
        [SerializeField] private float m_BandVerticalFov = 55f;

        [Header("Options")]
        [SerializeField] private bool m_BuildPlaceholderIfMissing = true;
        [Tooltip("Also derive scene ambient light from the panorama. Costs a GI update.")]
        [SerializeField] private bool m_UpdateAmbientLighting = false;

        private static Texture2D s_Placeholder;

        public Texture PanoramaTexture => m_Panorama != null ? m_Panorama : (m_BuildPlaceholderIfMissing ? PlaceholderTexture() : null);

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                Apply();
            }
        }

        /// <summary>Swaps in a new panorama and applies it immediately.</summary>
        public void SetPanorama(Texture panorama)
        {
            m_Panorama = panorama;
            Apply();
        }

        public void SetRotation(float degrees)
        {
            m_Rotation = Mathf.Repeat(degrees, 360f);
            Apply();
        }

        /// <summary>Builds the material and pushes it to the scene's RenderSettings.</summary>
        public void Apply()
        {
            Texture texture = PanoramaTexture;
            if (texture == null)
            {
                return;
            }

            if (m_SkyboxMaterial == null)
            {
                // Prefer the band shader (correct for a normal panorama photo) and fall back
                // to Unity's panoramic shader (expects a full equirectangular 360 image).
                Shader shader = Shader.Find("LiminalSpace/Skybox Panorama Band");
                if (shader == null)
                {
                    shader = Shader.Find("Skybox/Panoramic");
                }

                if (shader == null)
                {
                    Debug.LogWarning("[PanoramaSkybox] 'Skybox/Panoramic' shader not found. " +
                                     "Add it to Always Included Shaders in Graphics settings.", this);
                    return;
                }

                m_SkyboxMaterial = new Material(shader) { name = "Panorama360 Skybox" };
            }

            m_SkyboxMaterial.SetFloat("_Exposure", m_Exposure);
            m_SkyboxMaterial.SetFloat("_Rotation", m_Rotation);
            m_SkyboxMaterial.SetTexture("_MainTex", texture);

            if (m_SkyboxMaterial.HasProperty("_VerticalFov"))
            {
                // Band shader: the photo keeps its real size around the horizon.
                m_SkyboxMaterial.SetFloat("_VerticalFov", m_BandVerticalFov);
            }
            else
            {
                // 1 = Latitude Longitude mapping, 0 = 360 degrees (not a 180 or stereo image).
                m_SkyboxMaterial.SetFloat("_Mapping", 1f);
                m_SkyboxMaterial.SetFloat("_ImageType", 0f);
                m_SkyboxMaterial.SetFloat("_MirrorOnBack", 0f);
            }

            RenderSettings.skybox = m_SkyboxMaterial;

            if (m_UpdateAmbientLighting)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                DynamicGI.UpdateEnvironment();
            }
        }

        /// <summary>
        /// Cheap dusk-toned equirectangular gradient so a fresh scene renders something.
        /// Replaced by any real panorama assigned in the inspector.
        /// </summary>
        private static Texture2D PlaceholderTexture()
        {
            if (s_Placeholder != null)
            {
                return s_Placeholder;
            }

            const int width = 1024;
            const int height = 512;

            Color zenith = new Color(0.10f, 0.12f, 0.24f);
            Color horizon = new Color(0.93f, 0.72f, 0.53f);
            Color ground = new Color(0.11f, 0.11f, 0.12f);
            Color sun = new Color(1f, 0.93f, 0.78f);

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
            {
                name = "Placeholder360",
                wrapModeU = TextureWrapMode.Repeat,
                wrapModeV = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1); // 0 = nadir, 1 = zenith
                bool isSky = v >= 0.5f;
                float band = isSky ? (v - 0.5f) * 2f : (0.5f - v) * 2f;
                Color baseColor = isSky
                    ? Color.Lerp(horizon, zenith, Mathf.Pow(band, 0.55f))
                    : Color.Lerp(horizon, ground, Mathf.Pow(band, 0.35f));

                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);

                    // Warm glow sitting just above the horizon at one longitude.
                    float du = Mathf.Repeat(u - 0.72f + 0.5f, 1f) - 0.5f;
                    float dv = v - 0.63f;
                    float distance = Mathf.Sqrt(du * du * 6f + dv * dv);
                    Color color = baseColor + sun * Mathf.Exp(-distance * distance * 140f) * 0.9f;

                    pixels[y * width + x] = new Color(
                        Mathf.Clamp01(color.r),
                        Mathf.Clamp01(color.g),
                        Mathf.Clamp01(color.b),
                        1f);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_Placeholder = texture;
            return s_Placeholder;
        }
    }
}
