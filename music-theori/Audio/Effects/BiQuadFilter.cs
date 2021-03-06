﻿using System;
using System.Diagnostics;

namespace theori.Audio.Effects
{
    public sealed class BiQuadFilter : Dsp
    {
        private const uint order = 2;

        private float b0 = 1;
        private float b1 = 0;
        private float b2 = 0;
        private float a0 = 1;
        private float a1 = 0;
        private float a2 = 0;
        
        private readonly float[,] zb = new float[2, order];
        private readonly float[,] za = new float[2, order];

        public BiQuadFilter(int sampleRate)
            : base(sampleRate)
        {
        }

        protected override void ProcessImpl(Span<float> buffer)
        {
            int numSamples = buffer.Length / 2;

            float mix = Mix;

            float b0 = this.b0;
            float b1 = this.b1;
            float b2 = this.b2;
            float a0 = this.a0;
            float a1 = this.a1;
            float a2 = this.a2;

            for (int i = 0; i < numSamples; i++)
            {
                for (int c = 0; c < 2; c++)
                {
                    float src = buffer[i * 2 + c];
                    float filtered =
				        (b0 / a0) * src +
				        (b1 / a0) * zb[c, 0] +
				        (b2 / a0) * zb[c, 1] -
				        (a1 / a0) * za[c, 0] -
				        (a2 / a0) * za[c, 1];

			        // Shift delay buffers
			        zb[c, 1] = zb[c, 0];
			        zb[c, 0] = src;

			        // Feedback the calculated value into the IIR delay buffers
			        za[c, 1] = za[c, 0];
			        za[c, 0] = filtered;

                    //sample = filtered;
                    buffer[i * 2 + c] = src * (1 - mix) + filtered * mix;
                }
            }
        }

        public void SetLowPass(float q, float freq) => SetLowPass(q, freq, SampleRate);
        public void SetLowPass(float q, float freq, int sampleRate)
        {
	        // Limit q
	        q = Math.Max(q, 0.01f);

	        // Sampling frequency
	        double w0 = (2 * MathL.Pi * freq) / sampleRate;
	        double cw0 = Math.Cos(w0);
	        float alpha = (float)(Math.Sin(w0) / (2 * q));

	        b0 = (float)((1 - cw0) / 2);
	        b1 = (float)(1 - cw0);
	        b2 = (float)((1 - cw0) / 2);
	        a0 = 1 + alpha;
	        a1 = (float)(-2 * cw0);
	        a2 = 1 - alpha;
        }

        public void SetHighPass(float q, float freq) => SetHighPass(q, freq, SampleRate);
        public void SetHighPass(float q, float freq, int sampleRate)
        {
            // Limit q
	        q = Math.Max(q, 0.01f);

            Debug.Assert(freq < sampleRate, "freq !< sampleRate");
            double w0 = (2 * MathL.Pi * freq) / sampleRate;
            double cw0 = Math.Cos(w0);
            float alpha = (float)(Math.Sin(w0) / (2 * q));

            b0 = (float)((1 + cw0) / 2);
            b1 = (float)-(1 + cw0);
            b2 = (float)((1 + cw0) / 2);
            a0 = 1 + alpha;
            a1 = (float)(-2 * cw0);
            a2 = 1 - alpha;
        }

        public void SetPeaking(float q, float freq, float gain) => SetPeaking(q, freq, gain, SampleRate);
        public void SetPeaking(float q, float freq, float gain, int sampleRate)
        {
	        // Limit q
	        q = Math.Max(q, 0.01f);

	        double w0 = (2 * MathL.Pi * freq) / sampleRate;
	        double cw0 = Math.Cos(w0);
	        float alpha = (float)(Math.Sin(w0) / (2 * q));
	        double A = Math.Pow(10, (gain / 40));

	        b0 = 1 + (float)(alpha * A);
	        b1 = -2 * (float)cw0;
	        b2 = 1 - (float)(alpha*A);
	        a0 = 1 + (float)(alpha / A);
	        a1 = -2 * (float)cw0;
	        a2 = 1 - (float)(alpha / A);
        }
    }
}
