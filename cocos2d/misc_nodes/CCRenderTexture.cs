using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cocos2D
{
    public partial class CCRenderTexture : CCNode
    {
        private bool m_bFirstUsage = true;
        protected SurfaceFormat m_ePixelFormat;
        private RenderTarget2D m_pRenderTarget2D;
        protected CCSprite m_pSprite;
        protected CCTexture2D m_pTexture;

        public CCRenderTexture()
        {
            m_ePixelFormat = SurfaceFormat.Color;
        }

        public CCSprite Sprite
        {
            get { return m_pSprite; }
            set { m_pSprite = value; }
        }

        public CCRenderTexture(int w, int h)
        {
            InitWithWidthAndHeight(w, h, SurfaceFormat.Color, DepthFormat.None, RenderTargetUsage.DiscardContents);
        }

        public CCRenderTexture(int w, int h, SurfaceFormat format)
        {
            InitWithWidthAndHeight(w, h, format, DepthFormat.None, RenderTargetUsage.DiscardContents);
        }

        public CCRenderTexture(int w, int h, SurfaceFormat format, DepthFormat depthFormat, RenderTargetUsage usage)
        {
            InitWithWidthAndHeight(w, h, format, depthFormat, usage);
        }

        private void TextureReInit()
        {
            m_pRenderTarget2D = null;
            m_pTexture = null;
            if (m_pSprite != null)
            {
                m_pSprite.RemoveFromParent();
                m_pSprite = null;
            }
            MakeTexture();
        }

        private void MakeTexture()
        {
            m_pTexture = new CCTexture2D();
            m_pTexture.OnReInit = TextureReInit;
            m_pTexture.IsAntialiased = false;

            m_pRenderTarget2D = CCDrawManager.CreateRenderTarget(m_Width, m_Height, m_ColorFormat, m_DepthFormat, m_Usage);
            m_pTexture.InitWithTexture(m_pRenderTarget2D, m_ColorFormat, true, false);

            m_bFirstUsage = true;

            m_pSprite = new CCSprite(m_pTexture);
            //m_pSprite.scaleY = -1;
            m_pSprite.BlendFunc = CCBlendFunc.AlphaBlend;

            AddChild(m_pSprite);
        }

        private SurfaceFormat m_ColorFormat;
        private DepthFormat m_DepthFormat;
        private RenderTargetUsage m_Usage;
        private int m_Width, m_Height;

        protected virtual bool InitWithWidthAndHeight(int w, int h, SurfaceFormat colorFormat, DepthFormat depthFormat, RenderTargetUsage usage)
        {
            m_Width = (int)Math.Ceiling(w * CCMacros.CCContentScaleFactor());
            m_Height = (int)Math.Ceiling(h * CCMacros.CCContentScaleFactor());
            m_ColorFormat = colorFormat;
            m_DepthFormat = depthFormat;
            m_Usage = usage;
            MakeTexture();
            return true;
        }

        public virtual void Begin()
        {
            if (m_pTexture.IsDisposed)
            {
                TextureReInit();
            }
            // Save the current matrix
            CCDrawManager.PushMatrix();

            CCSize texSize = m_pTexture.ContentSizeInPixels;

            // Calculate the adjustment ratios based on the old and new projections
            CCDirector director = CCDirector.SharedDirector;
            CCSize size = director.WinSizeInPixels;
            float widthRatio = size.Width / texSize.Width;
            float heightRatio = size.Height / texSize.Height;

            CCDrawManager.SetRenderTarget(m_pTexture);

            CCDrawManager.SetViewPort(0, 0, (int) texSize.Width, (int) texSize.Height);

            Matrix projection = Matrix.CreateOrthographicOffCenter(
                -1.0f / widthRatio, 1.0f / widthRatio,
                -1.0f / heightRatio, 1.0f / heightRatio,
                -1, 1
                );

            CCDrawManager.MultMatrix(ref projection);

            if (m_bFirstUsage)
            {
                CCDrawManager.Clear(Color.Transparent);
                m_bFirstUsage = false;
            }
        }

        public void BeginWithClear(float r, float g, float b, float a)
        {
            Begin();
            CCDrawManager.Clear(new Color(r, g, b, a));
        }

        public void BeginWithClear(float r, float g, float b, float a, float depthValue)
        {
            Begin();
            CCDrawManager.Clear(new Color(r, g, b, a), depthValue);
        }

        public void BeginWithClear(float r, float g, float b, float a, float depthValue, int stencilValue)
        {
            Begin();
            CCDrawManager.Clear(new Color(r, g, b, a), depthValue, stencilValue);
        }

        public void ClearDepth(float depthValue)
        {
            Begin();
            CCDrawManager.Clear(ClearOptions.DepthBuffer, Color.White, depthValue, 0);
            End();
        }

        public void ClearStencil(int stencilValue)
        {
            Begin();
            CCDrawManager.Clear(ClearOptions.Stencil, Color.White, 0, stencilValue);
            End();
        }

        public virtual void End()
        {
            CCDrawManager.PopMatrix();

            CCDirector director = CCDirector.SharedDirector;

            CCDrawManager.SetRenderTarget((CCTexture2D) null);

            director.Projection = director.Projection;
        }

        public void Clear(float r, float g, float b, float a)
        {
            BeginWithClear(r, g, b, a);
            End();
        }

        public bool SaveToStream(Stream stream, CCImageFormat format)
        {
            if (format == CCImageFormat.Png)
            {
                m_pTexture.SaveAsPng(stream, m_pTexture.PixelsWide, m_pTexture.PixelsHigh);
            }
            else if (format == CCImageFormat.Jpg)
            {
                m_pTexture.SaveAsJpeg(stream, m_pTexture.PixelsWide, m_pTexture.PixelsHigh);
            }
            else
            {
                return false;
            }
            
            return true;
        }
    }
}