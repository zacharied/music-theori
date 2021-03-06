﻿using System;

namespace theori.Audio.Effects
{
    public class Flanger : Dsp
    {
        private float m_length, m_time;
        private int m_min, m_max;
        private int m_bufferLength, m_bufferOffset;
        private float[] m_sampleBuffer;

        public Flanger(int sampleRate)
            : base(sampleRate)
        {
        }
        
        public void SetDelay(double delay)
        {
	        m_length = (int)(delay * SampleRate);
        }
        
        public void SetDelayRange(int min, int max)
        {
	        float mult = SampleRate / 44100.0f;
	        m_min = (int)(min * mult);
	        m_max = (int)(max * mult);

	        m_bufferLength = m_max * 2;
            Array.Resize(ref m_sampleBuffer, m_bufferLength);
        }

        protected override void ProcessImpl(Span<float> buffer)
        {
	        if(m_bufferLength <= 0)
		        return;

            int numSamples = buffer.Length / 2;

	        for(int i = 0; i < numSamples; i++)
	        {
		        // Determine where we want to sample past samples
		        float f =  ((float)m_time / (float)m_length) % 1;
		        f = MathL.Abs(f * 2 - 1);
		        int d = (int)(m_min + ((m_max - 1) - m_min) * f);

		        // TODO: clean up?
		        int samplePos = ((int)m_bufferOffset - (int)d * 2) % (int)m_bufferLength;
		        if (samplePos < 0)
			        samplePos = m_bufferLength + samplePos;

		        // Inject new sample
		        m_sampleBuffer[m_bufferOffset + 0] = buffer[i*2];
		        m_sampleBuffer[m_bufferOffset + 1] = buffer[i*2+1];

		        // Apply delay
		        buffer[i * 2] = MathL.Lerp(buffer[i * 2], (m_sampleBuffer[samplePos] + buffer[i * 2]) * 0.5f, Mix);
		        buffer[i * 2 + 1] = MathL.Lerp(buffer[i * 2 + 1], (m_sampleBuffer[samplePos + 1] + buffer[i * 2 + 1]) * 0.5f, Mix);

		        m_bufferOffset += 2;
		        if(m_bufferOffset >= m_bufferLength)
			        m_bufferOffset = 0;
		        m_time++;
	        }
        }
    }
}
