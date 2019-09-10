﻿using System;

using theori.Graphics;

namespace theori.Core30.Layers
{
    public class TransitionCurtain : Disposable
    {
        enum State
        {
            Opened,
            Closed,

            Opening,
            Closing,
        }

        private readonly BasicSpriteRenderer m_renderer;

        private readonly float m_speed = 5.0f;

        private State m_state = State.Opened;
        private float m_transition = 0.0f, m_hold = 0.0f, m_pulse = 0.0f;

        private Action? m_onComplete;

        public TransitionCurtain()
        {
            m_renderer = new BasicSpriteRenderer();
        }

        protected override void DisposeManaged()
        {
            m_renderer.Dispose();
        }

        public bool Close(float holdTime = 0.0f, Action? onClosed = null)
        {
            if (m_state != State.Opened || m_onComplete != null) return false;

            m_onComplete = onClosed;
            m_state = State.Closing;

            m_hold = holdTime;
            m_pulse = 0.0f;

            return true;
        }

        public bool Open(Action? onOpened = null)
        {
            if (m_state != State.Closed || m_onComplete != null) return false;

            m_onComplete = onOpened;
            m_state = State.Opening;

            return true;
        }

        public void Update(float delta, float total)
        {
            switch (m_state)
            {
                case State.Closed:
                {
                    m_hold -= delta;
                    m_pulse += delta * MathL.Pi;

                    if (m_hold <= 0)
                    {
                        m_hold = 0.0f;

                        m_onComplete?.Invoke();
                        m_onComplete = null;
                    }
                } break;

                case State.Opening:
                {
                    m_transition -= delta * m_speed;
                    if (m_transition <= 0.0f)
                    {
                        m_transition = 0.0f;
                        m_state = State.Opened;

                        m_onComplete?.Invoke();
                        m_onComplete = null;
                    }
                } break;

                case State.Closing:
                {
                    m_transition += delta * m_speed;
                    if (m_transition >= 1.0f)
                    {
                        m_transition = 1.0f;
                        m_state = State.Closed;
                    }
                } break;
            }
        }

        public void Render()
        {
            if (m_state == State.Opened) return; // don't draw if nothing will be drawn

            float actual = MathL.Min(m_transition, 1);
            Render(actual, m_pulse);
        }

        protected virtual void Render(float animTimer, float idleTimer)
        {
            m_renderer.BeginFrame();
            {
                int width = Window.Width, height = Window.Height;
                int originx = width / 2, originy = height / 2;

                float bgRotation = animTimer * 45;
                float bgDist = (width / 2) * (1 - animTimer);
                float bgWidth = width;
                float bgHeight = height * 4;

                m_renderer.Rotate(bgRotation);
                m_renderer.Translate(originx, originy);

                m_renderer.SetColor(160, 150, 150);
                m_renderer.FillRect(bgDist, -bgHeight / 2, bgWidth, bgHeight);
                m_renderer.SetColor(90, 90, 100);
                m_renderer.FillRect(-bgDist - bgWidth, -bgHeight / 2, bgWidth, bgHeight);

                m_renderer.ResetTransform();
                m_renderer.Rotate(360 * (1 - animTimer));
                m_renderer.Scale(1 + 9 * (1 - animTimer) + MathL.Abs(MathL.Sin(idleTimer)) * 0.1f);
                m_renderer.Translate(originx, originy);

                float iconSize = Math.Min(Window.Width, Window.Height) * 0.3f;
                m_renderer.SetImageColor(255, 255, 255, 255 * animTimer);
                m_renderer.Image(Host.StaticResources.GetTexture("textures/theori-logo-large"), -iconSize / 2, -iconSize / 2, iconSize, iconSize);
            }
            m_renderer.EndFrame();
        }
    }
}