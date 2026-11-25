using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LiminalSpace
{
    /// <summary>
    /// Drives URP Post-Processing (Bloom, Vignette, FilmGrain, ChromaticAberration, ColorAdjustments)
    /// and dynamic Sky/Fog palettes per realm for cinematic liminal aesthetics.
    /// </summary>
    public class LiminalVisualEffects : MonoBehaviour
    {
        private static LiminalVisualEffects s_Instance;
        public static LiminalVisualEffects Instance => s_Instance;

        private Volume m_Volume;
        private VolumeProfile m_Profile;
        private Bloom m_Bloom;
        private Vignette m_Vignette;
        private FilmGrain m_FilmGrain;
        private ChromaticAberration m_Chromatic;
        private ColorAdjustments m_ColorAdj;

        private float m_TargetChromatic = 0.04f;
        private float m_TargetVignette = 0.28f;
        private Coroutine m_PulseRoutine;

        private void Awake()
        {
            s_Instance = this;
            SetupVolume();
        }

        private void SetupVolume()
        {
            // Destroy existing template volumes if needed or reuse
            m_Volume = GetComponent<Volume>();
            if (m_Volume == null)
            {
                m_Volume = gameObject.AddComponent<Volume>();
            }
            m_Volume.isGlobal = true;
            m_Volume.priority = 10f;

            m_Profile = ScriptableObject.CreateInstance<VolumeProfile>();
            m_Volume.sharedProfile = m_Profile;

            m_Bloom = m_Profile.Add<Bloom>(true);
            m_Bloom.threshold.Override(0.85f);
            m_Bloom.intensity.Override(0.65f);
            m_Bloom.scatter.Override(0.65f);
            m_Bloom.highQualityFiltering.Override(true);

            m_Vignette = m_Profile.Add<Vignette>(true);
            m_Vignette.color.Override(Color.black);
            m_Vignette.intensity.Override(0.28f);
            m_Vignette.smoothness.Override(0.45f);

            m_FilmGrain = m_Profile.Add<FilmGrain>(true);
            m_FilmGrain.type.Override(FilmGrainLookup.Medium3);
            m_FilmGrain.intensity.Override(0.22f);
            m_FilmGrain.response.Override(0.8f);

            m_Chromatic = m_Profile.Add<ChromaticAberration>(true);
            m_Chromatic.intensity.Override(0.04f);

            m_ColorAdj = m_Profile.Add<ColorAdjustments>(true);
            m_ColorAdj.contrast.Override(12f);
            m_ColorAdj.saturation.Override(12f);
        }

        public static void ConfigureCameraForURP(Camera cam)
        {
            if (cam == null) return;
            var urpData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urpData == null)
            {
                urpData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            urpData.renderPostProcessing = true;
            urpData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        }

        public void ApplyHubAtmosphere()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.04f, 0.04f, 0.03f);
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.17f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.12f, 0.11f, 0.08f);
            RenderSettings.fogDensity = 0.018f;

            if (m_Bloom != null) m_Bloom.intensity.Override(0.55f);
            if (m_FilmGrain != null) m_FilmGrain.intensity.Override(0.24f);
            if (m_ColorAdj != null)
            {
                m_ColorAdj.colorFilter.Override(Color.white);
                m_ColorAdj.saturation.Override(5f);
            }
            m_TargetVignette = 0.30f;
            m_TargetChromatic = 0.05f;
        }

        public void ApplyRealmAtmosphere(RealmType realm, AnomalyType anomaly)
        {
            Camera cam = Camera.main;
            bool isCrimsonSky = (anomaly == AnomalyType.CrimsonSky);

            switch (realm)
            {
                case RealmType.SilentField:
                    // The Celestial Horizon: Ethereal twilight gradient (Powder Blue, Lavender, Dusty Rose)
                    Color skyColor = isCrimsonSky ? new Color(0.12f, 0.01f, 0.02f) : new Color(0.38f, 0.42f, 0.58f);
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = skyColor;
                    }
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    RenderSettings.ambientLight = isCrimsonSky
                        ? new Color(0.20f, 0.05f, 0.06f)
                        : new Color(0.52f, 0.50f, 0.68f); // Soft ethereal lavender twilight
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    RenderSettings.fogColor = isCrimsonSky ? new Color(0.18f, 0.02f, 0.04f) : new Color(0.66f, 0.62f, 0.78f);
                    RenderSettings.fogDensity = 0.0055f; // Atmospheric depth allowing colossal moons to shine through

                    if (m_Bloom != null)
                    {
                        m_Bloom.threshold.Override(0.65f);
                        m_Bloom.intensity.Override(1.15f);
                    }
                    if (m_FilmGrain != null) m_FilmGrain.intensity.Override(0.08f);
                    if (m_ColorAdj != null)
                    {
                        m_ColorAdj.colorFilter.Override(isCrimsonSky ? new Color(1f, 0.8f, 0.8f) : new Color(1.0f, 0.96f, 0.98f));
                        m_ColorAdj.saturation.Override(22f);
                        m_ColorAdj.contrast.Override(14f);
                    }
                    m_TargetVignette = 0.16f;
                    m_TargetChromatic = (anomaly != AnomalyType.None) ? 0.14f : 0.02f;
                    break;

                case RealmType.Poolrooms:
                    // Creepy Flooded Subterranean Poolrooms: Deep murky aqua, oppressive shadows
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0.04f, 0.10f, 0.14f);
                    }
                    RenderSettings.ambientLight = new Color(0.14f, 0.22f, 0.26f);
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = new Color(0.07f, 0.16f, 0.20f);
                    RenderSettings.fogDensity = 0.028f;
                    if (m_Bloom != null) m_Bloom.intensity.Override(0.70f);
                    if (m_FilmGrain != null) m_FilmGrain.intensity.Override(0.28f);
                    if (m_ColorAdj != null) m_ColorAdj.saturation.Override(14f);
                    m_TargetVignette = 0.38f;
                    m_TargetChromatic = 0.08f;
                    break;

                case RealmType.YellowBackrooms:
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0.14f, 0.12f, 0.06f);
                    }
                    RenderSettings.ambientLight = new Color(0.25f, 0.23f, 0.15f);
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = new Color(0.22f, 0.20f, 0.11f);
                    RenderSettings.fogDensity = 0.035f;
                    if (m_Bloom != null) m_Bloom.intensity.Override(0.55f);
                    if (m_FilmGrain != null) m_FilmGrain.intensity.Override(0.35f);
                    m_TargetVignette = 0.36f;
                    m_TargetChromatic = 0.10f;
                    break;

                case RealmType.CrimsonStairwell:
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0.06f, 0.01f, 0.01f);
                    }
                    RenderSettings.ambientLight = new Color(0.14f, 0.04f, 0.04f);
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = new Color(0.12f, 0.01f, 0.01f);
                    RenderSettings.fogDensity = 0.04f;
                    if (m_Bloom != null) m_Bloom.intensity.Override(0.95f);
                    m_TargetVignette = 0.44f;
                    m_TargetChromatic = 0.18f;
                    break;

                case RealmType.VelvetCinema:
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0.03f, 0.01f, 0.02f);
                    }
                    RenderSettings.ambientLight = new Color(0.09f, 0.04f, 0.06f);
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = new Color(0.05f, 0.02f, 0.03f);
                    RenderSettings.fogDensity = 0.028f;
                    if (m_Bloom != null) m_Bloom.intensity.Override(0.8f);
                    m_TargetVignette = 0.38f;
                    m_TargetChromatic = 0.08f;
                    break;

                case RealmType.WhiteVoid:
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0.88f, 0.90f, 0.92f);
                    }
                    RenderSettings.ambientLight = new Color(0.75f, 0.78f, 0.80f);
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    RenderSettings.fogColor = new Color(0.88f, 0.90f, 0.92f);
                    RenderSettings.fogDensity = 0.048f;
                    if (m_Bloom != null) m_Bloom.intensity.Override(0.6f);
                    if (m_FilmGrain != null) m_FilmGrain.intensity.Override(0.12f);
                    m_TargetVignette = 0.18f;
                    m_TargetChromatic = 0.05f;
                    break;
            }
        }

        public void PulseAnomalyWarning(float duration = 2.0f)
        {
            if (m_PulseRoutine != null) StopCoroutine(m_PulseRoutine);
            m_PulseRoutine = StartCoroutine(PulseRoutine(duration));
        }

        private IEnumerator PulseRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
                if (m_Chromatic != null) m_Chromatic.intensity.Override(Mathf.Lerp(m_TargetChromatic, 0.85f, t));
                if (m_Vignette != null)
                {
                    m_Vignette.intensity.Override(Mathf.Lerp(m_TargetVignette, 0.58f, t));
                    m_Vignette.color.Override(Color.Lerp(Color.black, new Color(0.45f, 0f, 0.05f), t));
                }
                yield return null;
            }
            if (m_Chromatic != null) m_Chromatic.intensity.Override(m_TargetChromatic);
            if (m_Vignette != null)
            {
                m_Vignette.intensity.Override(m_TargetVignette);
                m_Vignette.color.Override(Color.black);
            }
        }

        private void Update()
        {
            if (m_PulseRoutine == null)
            {
                if (m_Vignette != null)
                    m_Vignette.intensity.value = Mathf.Lerp(m_Vignette.intensity.value, m_TargetVignette, Time.deltaTime * 3f);
                if (m_Chromatic != null)
                    m_Chromatic.intensity.value = Mathf.Lerp(m_Chromatic.intensity.value, m_TargetChromatic, Time.deltaTime * 3f);
            }
        }
    }
}

