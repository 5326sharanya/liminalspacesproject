using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Generates a pitch-black starry sky dome with thousands of multi-tinted twinkling stars.
    /// Supports CrimsonSky anomaly mode for Silent Field anomalies.
    /// </summary>
    public class StarfieldGenerator : MonoBehaviour
    {
        [Header("Star Settings")]
        [SerializeField] private int m_StarCount = 2800;
        [SerializeField] private float m_DomeRadius = 180f;
        [SerializeField] private float m_MinSize = 0.30f;
        [SerializeField] private float m_MaxSize = 1.45f;
        [SerializeField] private bool m_Twinkle = true;
        [SerializeField] private bool m_CrimsonMode = false;

        private ParticleSystem m_ParticleSystem;
        private ParticleSystem.Particle[] m_Particles;
        private float[] m_TwinkleOffsets;
        private Color[] m_BaseColors;

        private void Awake()
        {
            if (m_Particles == null)
            {
                SetupStarfield();
            }
        }

        public void SetCrimsonMode(bool crimson)
        {
            m_CrimsonMode = crimson;
            if (m_Particles != null)
            {
                RecolorStars();
            }
        }

        public void SetupStarfield()
        {
            m_ParticleSystem = GetComponent<ParticleSystem>();
            if (m_ParticleSystem == null)
            {
                m_ParticleSystem = gameObject.AddComponent<ParticleSystem>();
            }

            var main = m_ParticleSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = m_StarCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.startLifetime = Mathf.Infinity;

            var emission = m_ParticleSystem.emission;
            emission.enabled = false;

            var shape = m_ParticleSystem.shape;
            shape.enabled = false;

            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                Material starMat = new Material(LiminalAssetFactory.GetUnlitShader());
                starMat.color = Color.white;
                if (starMat.HasProperty("_BaseColor")) starMat.SetColor("_BaseColor", Color.white);
                renderer.sharedMaterial = starMat;
            }

            m_Particles = new ParticleSystem.Particle[m_StarCount];
            m_TwinkleOffsets = new float[m_StarCount];
            m_BaseColors = new Color[m_StarCount];

            System.Random rand = new System.Random(1337);

            for (int i = 0; i < m_StarCount; i++)
            {
                float u = (float)rand.NextDouble();
                float v = (float)rand.NextDouble();

                float theta = 2f * Mathf.PI * u;
                float phi = Mathf.Acos(1f - v * 0.96f);

                float x = m_DomeRadius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = m_DomeRadius * Mathf.Cos(phi) + 2f;
                float z = m_DomeRadius * Mathf.Sin(phi) * Mathf.Sin(theta);

                m_Particles[i].position = transform.position + new Vector3(x, y, z);

                // Occasional brighter hero stars
                double sizeRoll = rand.NextDouble();
                float size = (sizeRoll > 0.94)
                    ? Mathf.Lerp(m_MaxSize * 0.9f, m_MaxSize * 1.45f, (float)rand.NextDouble())
                    : Mathf.Lerp(m_MinSize, m_MaxSize * 0.85f, (float)rand.NextDouble());
                m_Particles[i].startSize = size;

                float alpha = (float)(0.60f + rand.NextDouble() * 0.40f);
                m_BaseColors[i] = PickStarTint(rand.NextDouble(), alpha);
                m_Particles[i].startColor = m_BaseColors[i];
                m_Particles[i].remainingLifetime = Mathf.Infinity;

                m_TwinkleOffsets[i] = (float)(rand.NextDouble() * Mathf.PI * 2.0);
            }

            if (m_CrimsonMode)
            {
                RecolorStars();
            }
            else
            {
                m_ParticleSystem.SetParticles(m_Particles, m_StarCount);
            }
        }

        private Color PickStarTint(double roll, float alpha)
        {
            if (m_CrimsonMode)
            {
                return new Color(1.0f, 0.22f, 0.22f, alpha);
            }
            if (roll < 0.22) return new Color(0.76f, 0.88f, 1.0f, alpha); // Ice blue
            if (roll < 0.36) return new Color(1.0f, 0.94f, 0.78f, alpha); // Pale gold
            return new Color(1.0f, 1.0f, 1.0f, alpha);                     // Pure crisp white
        }

        private void RecolorStars()
        {
            System.Random rand = new System.Random(1337);
            for (int i = 0; i < m_StarCount; i++)
            {
                float alpha = m_BaseColors[i].a;
                m_BaseColors[i] = PickStarTint(rand.NextDouble(), alpha);
                m_Particles[i].startColor = m_BaseColors[i];
            }
            m_ParticleSystem.SetParticles(m_Particles, m_StarCount);
        }

        private void Update()
        {
            if (!m_Twinkle || m_Particles == null) return;

            float time = Time.time * 2.4f;
            bool changed = false;

            int step = 3;
            for (int i = (Time.frameCount % step); i < m_StarCount; i += step)
            {
                float t = Mathf.Sin(time + m_TwinkleOffsets[i]) * 0.35f + 0.65f;
                Color baseCol = m_BaseColors[i];
                m_Particles[i].startColor = new Color(baseCol.r, baseCol.g, baseCol.b, baseCol.a * t);
                changed = true;
            }

            if (changed)
            {
                m_ParticleSystem.SetParticles(m_Particles, m_StarCount);
            }
        }
    }
}
