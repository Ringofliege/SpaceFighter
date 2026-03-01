using UnityEngine;

namespace SpaceFighter
{
    public static class AudioSynthesizer
    {
        private const int DefaultRate = 44100;

        // ── Lazy-initialized clip cache ──────────────────────────────────

        private static AudioClip _pierceChargeLoop;
        public static AudioClip PierceChargeLoop =>
            _pierceChargeLoop ?? (_pierceChargeLoop = GenerateSweep(200f, 800f, 2.5f, 0.4f));

        private static AudioClip _pierceFireSound;
        public static AudioClip PierceFireSound =>
            _pierceFireSound ?? (_pierceFireSound = GenerateSquareWaveADSR("PierceFire", 400f, 0.15f, 0.3f, 0.01f, 0.02f, 0.6f, 0.04f));

        private static AudioClip _overheatSound;
        public static AudioClip OverheatSound =>
            _overheatSound ?? (_overheatSound = GenerateNoise(0.3f, 0.35f));

        private static AudioClip _barrierOnSound;
        public static AudioClip BarrierOnSound =>
            _barrierOnSound ?? (_barrierOnSound = GenerateSineWave(300f, 0.2f, 0.5f));

        private static AudioClip _barrierOffSound;
        public static AudioClip BarrierOffSound =>
            _barrierOffSound ?? (_barrierOffSound = GenerateSweep(300f, 100f, 0.3f, 0.4f));

        private static AudioClip _lowEnergyWarning;
        public static AudioClip LowEnergyWarning =>
            _lowEnergyWarning ?? (_lowEnergyWarning = GeneratePulsingSine("LowEnergy", 600f, 0.5f, 0.3f, 8f));

        private static AudioClip _overtimeStartSound;
        public static AudioClip OvertimeStartSound =>
            _overtimeStartSound ?? (_overtimeStartSound = GenerateLayeredSine("OvertimeStart", new[] { 400f, 500f, 600f }, 1f, 0.35f));

        private static AudioClip _coreActiveSound;
        public static AudioClip CoreActiveSound =>
            _coreActiveSound ?? (_coreActiveSound = GeneratePulsingSine("CoreActive", 800f, 0.5f, 0.3f, 6f));

        private static AudioClip _dashSound;
        public static AudioClip DashSound =>
            _dashSound ?? (_dashSound = GenerateNoise(0.1f, 0.3f));

        private static AudioClip _blinkSound;
        public static AudioClip BlinkSound =>
            _blinkSound ?? (_blinkSound = GenerateSweep(1000f, 200f, 0.15f, 0.4f));

        private static AudioClip _shootSound;
        public static AudioClip ShootSound =>
            _shootSound ?? (_shootSound = GenerateSquareWave(500f, 0.08f, 0.25f));

        private static AudioClip _hitSound;
        public static AudioClip HitSound =>
            _hitSound ?? (_hitSound = GenerateHitClip());

        private static AudioClip _bashSound;
        public static AudioClip BashSound =>
            _bashSound ?? (_bashSound = GenerateSquareWave(150f, 0.2f, 0.3f));

        private static AudioClip _cloakSound;
        public static AudioClip CloakSound =>
            _cloakSound ?? (_cloakSound = GenerateSweep(800f, 400f, 0.3f, 0.2f));

        // ── Public generation methods ────────────────────────────────────

        public static AudioClip GenerateSineWave(float frequency, float duration, float volume = 0.5f, int sampleRate = DefaultRate)
        {
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = ApplyADSR(t, duration, 0.01f, 0.05f, 0.7f, 0.05f);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * env;
            }

            return CreateClip("Sine_" + (int)frequency, data, sampleRate);
        }

        public static AudioClip GenerateSquareWave(float frequency, float duration, float volume = 0.3f, int sampleRate = DefaultRate)
        {
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = ApplyADSR(t, duration, 0.005f, 0.02f, 0.8f, 0.02f);
                float val = Mathf.Sin(2f * Mathf.PI * frequency * t) >= 0f ? 1f : -1f;
                data[i] = val * volume * env;
            }

            return CreateClip("Square_" + (int)frequency, data, sampleRate);
        }

        public static AudioClip GenerateNoise(float duration, float volume = 0.3f, int sampleRate = DefaultRate)
        {
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = ApplyADSR(t, duration, 0.005f, 0.01f, 0.6f, 0.05f);
                data[i] = (Random.value * 2f - 1f) * volume * env;
            }

            return CreateClip("Noise", data, sampleRate);
        }

        public static AudioClip GenerateSweep(float startFreq, float endFreq, float duration, float volume = 0.5f, int sampleRate = DefaultRate)
        {
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float frac = t / duration;
                float freq = Mathf.Lerp(startFreq, endFreq, frac);
                float env = ApplyADSR(t, duration, 0.01f, 0.05f, 0.7f, 0.1f);

                phase += freq / sampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * phase) * volume * env;
            }

            return CreateClip($"Sweep_{(int)startFreq}_{(int)endFreq}", data, sampleRate);
        }

        // ── ADSR Envelope ────────────────────────────────────────────────

        public static float ApplyADSR(float t, float duration, float attack, float decay, float sustain, float release)
        {
            float releaseStart = duration - release;
            if (releaseStart < 0f) releaseStart = 0f;

            if (t < attack)
                return t / attack;
            if (t < attack + decay)
                return Mathf.Lerp(1f, sustain, (t - attack) / decay);
            if (t < releaseStart)
                return sustain;
            return Mathf.Lerp(sustain, 0f, (t - releaseStart) / release);
        }

        // ── Private helpers for pre-built clips ──────────────────────────

        private static AudioClip GenerateSquareWaveADSR(string name, float freq, float duration, float volume,
            float attack, float decay, float sustain, float release)
        {
            int samples = Mathf.CeilToInt(duration * DefaultRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / DefaultRate;
                float env = ApplyADSR(t, duration, attack, decay, sustain, release);
                float val = Mathf.Sin(2f * Mathf.PI * freq * t) >= 0f ? 1f : -1f;
                data[i] = val * volume * env;
            }

            return CreateClip(name, data, DefaultRate);
        }

        private static AudioClip GeneratePulsingSine(string name, float freq, float duration, float volume, float pulseRate)
        {
            int samples = Mathf.CeilToInt(duration * DefaultRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / DefaultRate;
                float env = ApplyADSR(t, duration, 0.02f, 0.05f, 0.8f, 0.1f);
                float pulse = (Mathf.Sin(2f * Mathf.PI * pulseRate * t) + 1f) * 0.5f;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * env * pulse;
            }

            return CreateClip(name, data, DefaultRate);
        }

        private static AudioClip GenerateLayeredSine(string name, float[] frequencies, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(duration * DefaultRate);
            float[] data = new float[samples];
            float perFreqVol = volume / frequencies.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / DefaultRate;
                float env = ApplyADSR(t, duration, 0.05f, 0.1f, 0.7f, 0.2f);
                float val = 0f;
                for (int f = 0; f < frequencies.Length; f++)
                    val += Mathf.Sin(2f * Mathf.PI * frequencies[f] * t);
                data[i] = val * perFreqVol * env;
            }

            return CreateClip(name, data, DefaultRate);
        }

        private static AudioClip GenerateHitClip()
        {
            float duration = 0.1f;
            int samples = Mathf.CeilToInt(duration * DefaultRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / DefaultRate;
                float env = ApplyADSR(t, duration, 0.002f, 0.01f, 0.5f, 0.03f);
                float sine = Mathf.Sin(2f * Mathf.PI * 200f * t) * 0.5f;
                float noise = (Random.value * 2f - 1f) * 0.5f;
                data[i] = (sine + noise) * 0.3f * env;
            }

            return CreateClip("Hit", data, DefaultRate);
        }

        private static AudioClip CreateClip(string name, float[] data, int sampleRate)
        {
            var clip = AudioClip.Create(name, data.Length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
