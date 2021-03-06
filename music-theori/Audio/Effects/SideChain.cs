﻿using System;

namespace theori.Audio.Effects
{
    public sealed class SideChain : Dsp
    {
        private static CubicBezier curve = new CubicBezier(0.39f, 0.575f, 0.565f, 1);

        private double time;

        public float Amount = 1.0f;
        public double Duration = 0.5;

        public SideChain(int sampleRate)
            : base(sampleRate)
        {
        }
        
        protected override void ProcessImpl(Span<float> buffer)
        {
            int numSamples = buffer.Length / 2;
            if(Duration == 0.0)
                return;

            double step = 1.0 / SampleRate;
            for(int i = 0; i < numSamples; i++)
            {
                float r = (float)(time / Duration);
                // FadeIn
                const float fadeIn = 0.08f;
                if(r < fadeIn)
                    r = 1.0f - r / fadeIn;
                else r = curve.Sample((r - fadeIn) / (1.0f - fadeIn));
                float sampleGain = 1.0f - Amount * (1.0f - r) * Mix;
                buffer[i * 2 + 0] *= sampleGain;
                buffer[i * 2 + 1] *= sampleGain;

                time += step;
                if(time > Duration)
                    time = 0;
            }
        }
    }
}
