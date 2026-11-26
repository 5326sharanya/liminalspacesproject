using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Procedural DSP audio synthesizer for the First-Hand sci-fi laboratory.
    /// Synthesizes tactile mechanical button clicks, heavy lever ratchets,
    /// power core magnetic locks, and ambient futuristic laboratory hums.
    /// </summary>
    public static class FirstHandAudioSynth
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateButtonClick()
        {
            float duration = 0.09f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(42);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 85f);
                float snap = Mathf.Sin(2f * Mathf.PI * 1400f * t) * 0.45f
                           + Mathf.Sin(2f * Mathf.PI * 2800f * t) * 0.25f;
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.15f;
                data[i] = Mathf.Clamp((snap + noise) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SciFiButtonClick", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateLeverClick()
        {
            float duration = 0.14f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(99);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 45f);
                float thud = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.6f;
                float metallic = Mathf.Sin(2f * Mathf.PI * 850f * t) * 0.25f;
                float rattle = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.12f;
                data[i] = Mathf.Clamp((thud + metallic + rattle) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SciFiLeverClick", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateCoreInsertSound()
        {
            float duration = 0.65f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            // Harmonic rising power chord: C4 (261Hz) -> G4 (392Hz) -> C5 (523Hz)
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                // Envelope: quick attack, sustained harmonic swell, gentle release
                float env = Mathf.Sin(progress * Mathf.PI);

                // Magnetic lock click at the beginning
                float click = (t < 0.05f) ? Mathf.Sin(2f * Mathf.PI * 1200f * t) * Mathf.Exp(-t * 90f) * 0.5f : 0f;

                // Rising power tone
                float freq = Mathf.Lerp(261.63f, 523.25f, progress);
                float hum = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f
                          + Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * t) * 0.25f
                          + Mathf.Sin(2f * Mathf.PI * (freq * 2.0f) * t) * 0.15f;

                data[i] = Mathf.Clamp((click + hum * env), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("PowerCoreLockChime", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateCoreRemoveSound()
        {
            float duration = 0.35f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;
                float env = Mathf.Exp(-t * 12f);

                // Falling pitch indicating power disconnection
                float freq = Mathf.Lerp(520f, 180f, progress);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.35f;
                data[i] = Mathf.Clamp(tone * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("PowerCoreDisengage", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateLabHum(float duration = 8f)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(333);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;

                // 50Hz clean laboratory sub-hum with smooth harmonics
                float sub = Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.18f
                          + Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.10f
                          + Mathf.Sin(2f * Mathf.PI * 150f * t) * 0.05f;

                // Gentle cooling fan / electronic pulse modulation
                float pulse = Mathf.Sin(2f * Mathf.PI * 0.35f * t) * 0.035f;
                float air = ((float)rand.NextDouble() * 2f - 1f) * 0.015f;

                data[i] = Mathf.Clamp(sub + pulse + air, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SciFiLabHum", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateHoloChime()
        {
            float duration = 0.45f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 9f);
                float chime = Mathf.Sin(2f * Mathf.PI * 1760f * t) * 0.3f
                            + Mathf.Sin(2f * Mathf.PI * 2637f * t) * 0.2f
                            + Mathf.Sin(2f * Mathf.PI * 3520f * t) * 0.1f;
                data[i] = Mathf.Clamp(chime * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("HoloInterfaceChime", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateBlastShieldServo(float duration = 2.5f)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(77);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin((t / duration) * Mathf.PI);

                // Heavy hydraulic servo frequency
                float motor = Mathf.Sin(2f * Mathf.PI * 88f * t) * 0.35f
                            + Mathf.Sin(2f * Mathf.PI * 176f * t) * 0.18f;
                float friction = ((float)rand.NextDouble() * 2f - 1f) * 0.06f;

                data[i] = Mathf.Clamp((motor + friction) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("BlastShieldServo", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
