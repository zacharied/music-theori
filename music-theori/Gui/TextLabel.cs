﻿using System;
using System.Diagnostics;
using System.Numerics;

using theori.Graphics;

namespace theori.Gui
{
    public class TextLabel : GuiElement
    {
        private string m_text;
        public string Text
        {
            get => m_text;
            set
            {
                if (m_text == value) return;
                m_text = value ?? "";

                CheckRasterizer().Text = m_text;
                SetSizeFromRasterizer();
            }
        }

        private Font m_font;
        public Font Font
        {
            get => m_font;
            set
            {
                if (m_font == value) return;
                m_font = value;

                CheckRasterizer().Font = m_font;
                SetSizeFromRasterizer();
            }
        }

        private float m_size = 16.0f;
        public float FontSize
        {
            get => m_size;
            set
            {
                if (m_size == value) return;
                m_size = value;

                CheckRasterizer().Size = m_size;
                SetSizeFromRasterizer();
            }
        }

        private Vector4 m_color = Vector4.One;
        public Vector4 Color
        {
            get => m_color;
            set
            {
                if (value == m_color)
                    return;
                m_color = value;
            }
        }

        public Anchor TextAlignment = Anchor.TopLeft;

        private TextRasterizer m_staticRasterizer;

        public TextLabel(Font font, float size = 16.0f, string text = "")
        {
            m_font = font;
            m_text = text ?? "";
            m_size = size;

            m_staticRasterizer = CheckRasterizer();
            SetSizeFromRasterizer();
        }

        protected override void DisposeManaged()
        {
            m_staticRasterizer.Dispose();
        }

        /// <summary>
        /// Returns true if a new rasterizer was created, false otherwise.
        /// </summary>
        /// <returns></returns>
        private TextRasterizer CheckRasterizer()
        {
            if (m_staticRasterizer == null)
                m_staticRasterizer = new TextRasterizer(m_font, m_size, m_text);
            return m_staticRasterizer;
        }

        private void SetSizeFromRasterizer()
        {
            Debug.Assert(m_staticRasterizer != null, "no rasterizer to get size from");

            var texture = m_staticRasterizer.Texture;
            Size = new Vector2(texture.Width, texture.Height);
        }

        public override void Render(GuiRenderQueue rq)
        {
            base.Render(rq);

            if (m_text == null) return;

            Vector2 offset = Vector2.Zero;
            switch ((Anchor)((int)TextAlignment & 0x0F))
            {
                case Anchor.Top: break;
                case Anchor.Middle: offset.Y = (int)(-DrawSize.Y / 2); break;
                case Anchor.Bottom: offset.Y = -DrawSize.Y; break;
            }

            switch ((Anchor)((int)TextAlignment & 0xF0))
            {
                case Anchor.Left: break;
                case Anchor.Center: offset.X = (int)(-DrawSize.X / 2); break;
                case Anchor.Right: offset.X = -DrawSize.X; break;
            }

            Rect rect = new Rect(offset, DrawSize);
            rq.DrawRect(CompleteTransform, rect, m_staticRasterizer.Texture, Color);
        }
    }
}
