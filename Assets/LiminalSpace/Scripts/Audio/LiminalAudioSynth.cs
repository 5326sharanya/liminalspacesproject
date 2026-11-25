using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Generates atmospheric liminal audio clips dynamically using procedural DSP synthesis.
    /// Eliminates external audio dependencies while providing rich ambient soundscapes,
    /// door-listening clues, and anomaly warnings.
    /// </summary>
    public static class LiminalAudioSynth
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateNightAmbience(float duration = 10f)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(42);

            float b0 = 0f, b1 = 0f, b2 = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;

                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                b0 = 0.99765f * b0 + white * 0.0990460f;
                b1 = 0.96300f * b1 + white * 0.2965164f;
                b2 = 0.57000f * b2 + white * 1.0526913f;
                float wind = (b0 + b1 + b2 + white * 0.1848f) * 0.032f;

                float windMod = Mathf.Sin(t * 0.35f) * 0.5f + 0.5f;
                wind *= (0.4f + windMod * 0.6f);

                float cricketCycle = Mathf.Sin(t * 7.5f);
                float cricketSound = 0f;
                if (cricketCycle > 0.45f)
                {
                    float chirpFiller = Mathf.Sin(2f * Mathf.PI * 4400f * t) * 0.018f
                                      + Mathf.Sin(2f * Mathf.PI * 4750f * t) * 0.012f;
                    float flutter = Mathf.Sin(2f * Mathf.PI * 32f * t);
                    cricketSound = chirpFiller * (flutter > 0 ? flutter : 0f);
                }

                data[i] = Mathf.Clamp(wind + cricketSound, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("NightFieldAmbience", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateFluorescentHum(float duration = 6f)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(1337);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float hum = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.14f
                          + Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.22f
                          + Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.07f
                          + Mathf.Sin(2f * Mathf.PI * 240f * t) * 0.04f;

                float hiss = ((float)rand.NextDouble() * 2f - 1f) * 0.015f;
                data[i] = Mathf.Clamp(hum + hiss, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("FluorescentHum", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateWaterDrip()
        {
            float duration = 1.4f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float drip = 0f;
                if (t < 0.35f)
                {
                    float freq = Mathf.Lerp(1350f, 650f, t / 0.35f);
                    float env = Mathf.Exp(-t * 14f);
                    drip = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.3f;
                }
                // Subtle echo
                if (t > 0.45f && t < 0.85f)
                {
                    float lt = t - 0.45f;
                    float freq = Mathf.Lerp(1100f, 600f, lt / 0.4f);
                    float env = Mathf.Exp(-lt * 12f);
                    drip += Mathf.Sin(2f * Mathf.PI * freq * lt) * env * 0.12f;
                }
                data[i] = Mathf.Clamp(drip, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("WaterDrip", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateDoorOpenSound()
        {
            float duration = 0.8f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(99);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float click = (t < 0.05f) ? (float)(rand.NextDouble() * 2.0 - 1.0) * (1f - t / 0.05f) * 0.4f : 0f;
                float creakFreq = 180f + Mathf.Sin(t * 30f) * 40f;
                float creakEnv = Mathf.Sin((t / duration) * Mathf.PI);
                float creak = Mathf.Sin(2f * Mathf.PI * creakFreq * t) * creakEnv * 0.15f;

                data[i] = Mathf.Clamp(click + creak, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("DoorOpenSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateWarningHum()
        {
            float duration = 3.5f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin((t / duration) * Mathf.PI);
                float bass = Mathf.Sin(2f * Mathf.PI * 45f * t) * 0.4f
                           + Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.3f
                           + Mathf.Sin(2f * Mathf.PI * 92f * t) * 0.15f;
                data[i] = bass * env;
            }

            AudioClip clip = AudioClip.Create("WarningHum", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateDistortedWhisper()
        {
            float duration = 2.2f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(666);

            float prev = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin((t / duration) * Mathf.PI);
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                // Band-like modulation simulating eerie breathy whispers
                prev = prev * 0.85f + white * 0.15f;
                float mod = Mathf.Sin(2f * Mathf.PI * 4.5f * t) * 0.5f + 0.5f;
                float dissonance = Mathf.Sin(2f * Mathf.PI * (110f + Mathf.Sin(t * 9f) * 18f) * t) * 0.12f;
                data[i] = Mathf.Clamp((prev * mod * 1.4f + dissonance) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("DistortedWhisper", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreatePhoneRing()
        {
            float duration = 3.0f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float ring = 0f;
                if (t < 1.2f)
                {
                    float bell = Mathf.Sin(2f * Mathf.PI * 440f * t) + Mathf.Sin(2f * Mathf.PI * 480f * t);
                    float hammer = Mathf.Sin(2f * Mathf.PI * 22f * t) > 0 ? 1f : 0.3f;
                    ring = bell * hammer * 0.12f;
                }
                data[i] = Mathf.Clamp(ring, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("PhoneRing", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateSafeChime()
        {
            float duration = 4.0f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float sum = 0f;
                for (int c = 0; c < freqs.Length; c++)
                {
                    float noteStart = c * 0.35f;
                    if (t >= noteStart)
                    {
                        float localT = t - noteStart;
                        float env = Mathf.Exp(-localT * 1.5f);
                        sum += Mathf.Sin(2f * Mathf.PI * freqs[c] * localT) * env * 0.12f;
                    }
                }
                data[i] = Mathf.Clamp(sum, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SafeChime", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateRelicPickup()
        {
            float duration = 1.6f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float[] freqs = new float[] { 783.99f, 987.77f, 1318.51f, 1567.98f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float sum = 0f;
                for (int c = 0; c < freqs.Length; c++)
                {
                    float noteStart = c * 0.08f;
                    if (t >= noteStart)
                    {
                        float lt = t - noteStart;
                        float env = Mathf.Exp(-lt * 3.0f);
                        sum += Mathf.Sin(2f * Mathf.PI * freqs[c] * lt) * env * 0.14f;
                    }
                }
                data[i] = Mathf.Clamp(sum, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("RelicPickup", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateFootstep(int type = 0)
        {
            float duration = 0.12f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1f - (t / duration);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float tone = Mathf.Sin(2f * Mathf.PI * (type == 0 ? 90f : 160f) * t);
                data[i] = (noise * 0.3f + tone * 0.4f) * env * 0.25f;
            }

            AudioClip clip = AudioClip.Create("Footstep_" + type, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateEatDrinkSound()
        {
            float duration = 1.0f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(55);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin((t / duration) * Mathf.PI);
                float gulp = Mathf.Sin(2f * Mathf.PI * (280f - t * 120f) * t) * 0.3f;
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.05f;
                data[i] = (gulp + noise) * env;
            }

            AudioClip clip = AudioClip.Create("EatDrinkSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateCelestialEtherealAmbience(float duration = 12f)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(888);

            // Rich celestial choir pad: C, G, D, E (open 9th chord)
            float[] freqs = { 130.81f, 196.00f, 261.63f, 293.66f, 329.63f, 392.00f, 523.25f, 659.25f };
            float[] amps =  { 0.12f,   0.10f,   0.09f,   0.07f,   0.06f,   0.05f,   0.035f,  0.02f };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;

                // Slow breathing LFO for pad volume modulation
                float lfo1 = Mathf.Sin(2f * Mathf.PI * 0.12f * t) * 0.2f + 0.8f;
                float lfo2 = Mathf.Cos(2f * Mathf.PI * 0.08f * t) * 0.25f + 0.75f;

                float sample = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    // Gentle detuning for lush stereo-like chorus effect
                    float detune = (f % 2 == 0) ? 0.35f : -0.35f;
                    sample += Mathf.Sin(2f * Mathf.PI * (freqs[f] + detune) * t) * amps[f] * (f < 4 ? lfo1 : lfo2);
                }

                // Crystalline high-frequency shimmer / wind
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                float shimmer = Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.008f * Mathf.Sin(t * 1.5f);
                float softAir = white * 0.007f * lfo1;

                data[i] = Mathf.Clamp((sample * 0.85f + shimmer + softAir), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("CelestialEtherealAmbience", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateCascadingWaterAudio(float duration = 8f)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(404);

            float b0 = 0f, b1 = 0f, b2 = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);

                // Pink noise filter
                b0 = 0.99765f * b0 + white * 0.0990460f;
                b1 = 0.96300f * b1 + white * 0.2965164f;
                b2 = 0.57000f * b2 + white * 1.0526913f;
                float pink = (b0 + b1 + b2 + white * 0.1848f) * 0.038f;

                // Subtle water murmur modulation
                float mod = Mathf.Sin(t * 0.6f) * 0.15f + Mathf.Sin(t * 1.4f) * 0.1f + 0.75f;
                data[i] = Mathf.Clamp(pink * mod, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("CascadingWaterAudio", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
