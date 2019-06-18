using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if (WINDOWS && !WINRT)
using BitMiracle.LibTiff.Classic;
#endif

namespace Cocos2D
{
    public enum CCImageFormat
    {
        Jpg = 0,
        Png,
        Tiff,
        Webp,
        Gif,
        RawData,
        UnKnown
    }

    internal enum CCTextureCacheType
    {
        None,
        AssetFile,
        Data,
        RawData,
        String
    }

    internal struct CCStringCache
    {
        public String Text;
        public CCSize Dimensions;
        public CCTextAlignment HAlignment;
        public CCVerticalTextAlignment VAlignment;
        public String FontName;
        public float FontSize;
    }

    internal struct CCTextureCacheInfo
    {
        public CCTextureCacheType CacheType;
        public Object Data;
    }

    public class CCTexture2D : CCGraphicsResource
    {
        public static SurfaceFormat DefaultAlphaPixelFormat = SurfaceFormat.Color;
        public static bool OptimizeForPremultipliedAlpha = true;
        public static bool PreserveSourceSurfaceFormat = true;

        private CCTextureCacheInfo m_CacheInfo;
        private Texture2D m_Texture2D;
        private bool m_bHasMipmaps;
        private bool m_bHasPremultipliedAlpha;
        private SurfaceFormat m_ePixelFormat;
        private SamplerState m_samplerState;
        private CCSize m_tContentSize;
        private int m_uPixelsHigh;
        private int m_uPixelsWide;

        private bool m_bManaged;
        private bool m_bAntialiased;

        public Action OnReInit;

        public CCTexture2D()
        {
            m_samplerState = SamplerState.LinearClamp;
            IsAntialiased = true;  // We will set this to true by default

            RefreshAntialiasSetting ();
        }

        public bool IsTextureDefined
        {
            get { return (m_Texture2D != null && !m_Texture2D.IsDisposed); }
        }

        public Texture2D XNATexture
        {
            get
            {
                if (m_Texture2D == null || m_Texture2D.IsDisposed)
                {
                    Reinit();
                }
                return m_Texture2D;
            }
        }

        /// <summary>
        ///     pixel format of the texture
        /// </summary>
        public SurfaceFormat PixelFormat
        {
            get { return m_ePixelFormat; }
            set { m_ePixelFormat = value; }
        }

        /// <summary>
        ///     width in pixels
        /// </summary>
        public int PixelsWide
        {
            get { return m_uPixelsWide; }
            set { m_uPixelsWide = value; }
        }

        /// <summary>
        ///     hight in pixels
        /// </summary>
        public int PixelsHigh
        {
            get { return m_uPixelsHigh; }
            set { m_uPixelsHigh = value; }
        }

        /// <summary>
        ///     texture name
        /// </summary>
        public Texture2D Name
        {
            get { return XNATexture; }
        }

        /// <summary>
        ///     content size
        /// </summary>
        public CCSize ContentSizeInPixels
        {
            get { return m_tContentSize; }
            set { m_tContentSize = value; }
        }

        public CCSize ContentSize
        {
            get { return m_tContentSize.PixelsToPoints(); }
        }

        /// <summary>
        ///     whether or not the texture has their Alpha premultiplied
        /// </summary>
        public bool HasPremultipliedAlpha
        {
            get { return m_bHasPremultipliedAlpha; }
            set { m_bHasPremultipliedAlpha = value; }
        }

        public SamplerState SamplerState
        {
            get { return m_samplerState; }
            set { m_samplerState = value; }
        }

        public bool IsAntialiased
        {
            get { return m_bAntialiased; }

            set
            {
                if (m_bAntialiased != value)
                {
                    m_bAntialiased = value;

                    RefreshAntialiasSetting ();
                }
            }
        }

        void RefreshAntialiasSetting ()
        {
            var saveState = m_samplerState;

            if (m_bAntialiased && m_samplerState.Filter != TextureFilter.Linear)
            {
                if (m_samplerState == SamplerState.PointClamp)
                {
                    m_samplerState = SamplerState.LinearClamp;
                    return;
                }

                m_samplerState = new SamplerState
                {
                    Filter = TextureFilter.Linear
                };
            }
            else if (!m_bAntialiased && m_samplerState.Filter != TextureFilter.Point)
            {
                if (m_samplerState == SamplerState.LinearClamp)
                {
                    m_samplerState = SamplerState.PointClamp;
                    return;
                }
                
                m_samplerState = new SamplerState
                {
                    Filter = TextureFilter.Point
                };
            }
            else
            {
                return;
            }

            m_samplerState.AddressU = saveState.AddressU;
            m_samplerState.AddressV = saveState.AddressV;
            m_samplerState.AddressW = saveState.AddressW;
        }

        public uint BytesPerPixelForFormat
        {
            //from MG: Microsoft.Xna.Framework.Graphics.GraphicsExtensions
            get
            {
                switch (m_ePixelFormat)
                {
                    case SurfaceFormat.Dxt1:
#if !WINDOWS && !WINDOWS_PHONE && !XBOX
                    case SurfaceFormat.Dxt1a:
                    case SurfaceFormat.RgbPvrtc2Bpp:
                    case SurfaceFormat.RgbaPvrtc2Bpp:
                    case SurfaceFormat.RgbEtc1:
#endif
                        // One texel in DXT1, PVRTC 2bpp and ETC1 is a minimum 4x4 block, which is 8 bytes
                        return 8;

                    case SurfaceFormat.Dxt3:
                    case SurfaceFormat.Dxt5:
#if !WINDOWS && !WINDOWS_PHONE && !XBOX
                    case SurfaceFormat.RgbPvrtc4Bpp:
                    case SurfaceFormat.RgbaPvrtc4Bpp:
#endif
                        // One texel in DXT3, DXT5 and PVRTC 4bpp is a minimum 4x4 block, which is 16 bytes
                        return 16;

                    case SurfaceFormat.Alpha8:
                        return 1;

                    case SurfaceFormat.Bgr565:
                    case SurfaceFormat.Bgra4444:
                    case SurfaceFormat.Bgra5551:
                    case SurfaceFormat.HalfSingle:
                    case SurfaceFormat.NormalizedByte2:
                        return 2;

                    case SurfaceFormat.Color:
                    case SurfaceFormat.Single:
                    case SurfaceFormat.Rg32:
                    case SurfaceFormat.HalfVector2:
                    case SurfaceFormat.NormalizedByte4:
                    case SurfaceFormat.Rgba1010102:
                        return 4;

                    case SurfaceFormat.HalfVector4:
                    case SurfaceFormat.Rgba64:
                    case SurfaceFormat.Vector2:
                        return 8;

                    case SurfaceFormat.Vector4:
                        return 16;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (m_Texture2D != null && !m_Texture2D.IsDisposed && !m_bManaged)
            {
                m_Texture2D.Dispose();
            }
            m_Texture2D = null;
        }

        public virtual int TotalBytes
        {
            get
            {
                int size = m_uPixelsHigh * m_uPixelsWide;
                if (IsTextureDefined)
                {
#if !XNA
                    size *= XNATexture.Format.GetSize();
#else
                    size *= (int)BytesPerPixelForFormat;
#endif
                }
                return (size);
            }
        }

        public override string ToString()
        {
            int size = TotalBytes;
            size = size >> 10; // KB
            return String.Format("[CCTexture2D | Dimensions = {0} x {1} | {2} | {3} | {4} KB)]", m_uPixelsWide, m_uPixelsHigh, m_bManaged ? "Managed" : "Native", IsTextureDefined ? "Valid" : "InValid", size);
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
            if (m_Texture2D != null)
            {
                m_Texture2D.SaveAsJpeg(stream, width, height);
            }
        }

        public void SaveAsPng(Stream stream, int width, int height)
        {
            if (m_Texture2D != null)
            {
                m_Texture2D.SaveAsPng(stream, width, height);
            }
        }

        [Obsolete("Use IsAntialiased property.")]
        public void SetAntiAliasTexParameters()
        {
            IsAntialiased = true;	
        }

        [Obsolete("Use IsAntialiased property.")]
        public void SetAliasTexParameters()
        {
            IsAntialiased = false;	
        }

        #region Initialization

        public bool Init(int pixelsWide, int pixelsHigh)
        {
            return Init(pixelsWide, pixelsHigh, DefaultAlphaPixelFormat, true, false);
        }

        public bool Init(int pixelsWide, int pixelsHigh, SurfaceFormat pixelFormat)
        {
            return Init(pixelsWide, pixelsHigh, pixelFormat, true, false);
        }

        public bool Init(int pixelsWide, int pixelsHigh, SurfaceFormat pixelFormat, bool premultipliedAlpha)
        {
            return Init(pixelsWide, pixelsHigh, pixelFormat, premultipliedAlpha, false);
        }

        public bool Init(int pixelsWide, int pixelsHigh, SurfaceFormat pixelFormat, bool premultipliedAlpha, bool mipMap)
        {
            try
            {
                var texture = new Texture2D(CCDrawManager.GraphicsDevice, pixelsWide, pixelsHigh, mipMap, pixelFormat);

                if (InitWithTexture(texture, pixelFormat, premultipliedAlpha, false))
                {
                    m_CacheInfo.CacheType = CCTextureCacheType.None;
                    m_CacheInfo.Data = null;

                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public bool InitWithData(byte[] data)
        {
            return InitWithData(data, DefaultAlphaPixelFormat);
        }

        public bool InitWithData(byte[] data, SurfaceFormat pixelFormat)
        {
            return InitWithData(data, DefaultAlphaPixelFormat, false);
        }

        public bool InitWithData(byte[] data, SurfaceFormat pixelFormat, bool mipMap)
        {
            if (data == null)
            {
                return (false);
            }
#if WINDOWS_PHONE8
            /*
            byte[] cloneOfData = new byte[data.Length];
            data.CopyTo(cloneOfData, 0);
            data = cloneOfData;
             */
#endif

            var texture = LoadTexture(new MemoryStream(data, false));

            if (texture != null)
            {
                if (InitWithTexture(texture, pixelFormat, true, false))
                {
                    m_CacheInfo.CacheType = CCTextureCacheType.Data;
                    m_CacheInfo.Data = data;

                    if (mipMap)
                    {
                        GenerateMipmap();
                    }

                    return true;
                }
            }

            return false;
        }

        public bool InitWithStream(Stream stream)
        {
            return InitWithStream(stream, DefaultAlphaPixelFormat);
        }

        public bool InitWithStream(Stream stream, SurfaceFormat pixelFormat)
        {
            Texture2D texture;
            try
            {
                texture = LoadTexture(stream);
                
                InitWithTexture(texture, pixelFormat, false, false);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool InitWithRawData<T>(T[] data, SurfaceFormat pixelFormat, int pixelsWide, int pixelsHigh,
                                       bool premultipliedAlpha)
            where T : struct
        {
            return InitWithRawData(data, pixelFormat, pixelsWide, pixelsHigh,
                                   premultipliedAlpha, false, new CCSize(pixelsWide, pixelsHigh));
        }

        public bool InitWithRawData<T>(T[] data, SurfaceFormat pixelFormat, int pixelsWide, int pixelsHigh,
                                       bool premultipliedAlpha, bool mipMap)
            where T : struct
        {
            return InitWithRawData(data, pixelFormat, pixelsWide, pixelsHigh,
                                   premultipliedAlpha, mipMap, new CCSize(pixelsWide, pixelsHigh));
        }

        public bool InitWithRawData<T>(T[] data, SurfaceFormat pixelFormat, int pixelsWide, int pixelsHigh,
                                       bool premultipliedAlpha, bool mipMap, CCSize contentSize) where T : struct
        {
            try
            {
                var texture = LoadRawData(data, pixelsWide, pixelsHigh, pixelFormat, mipMap);

                if (InitWithTexture(texture, pixelFormat, premultipliedAlpha, false))
                {
                    m_tContentSize = contentSize;

                    m_CacheInfo.CacheType = CCTextureCacheType.RawData;
                    m_CacheInfo.Data = data;

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool InitWithString(string text, string fontName, float fontSize)
        {
            return InitWithString(text, CCSize.Zero, CCTextAlignment.Center, CCVerticalTextAlignment.Top, fontName,
                                  fontSize);
        }

        public bool InitWithString(string text, CCSize dimensions, CCTextAlignment hAlignment,
                                   CCVerticalTextAlignment vAlignment, string fontName,
                                   float fontSize)
        {
            try
            {
                Debug.Assert(dimensions.Width >= 0 || dimensions.Height >= 0);

                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }

                float loadedSize = fontSize;

                SpriteFont font = CCSpriteFontCache.SharedInstance.TryLoadFont(fontName, fontSize, out loadedSize);

                if (font == null)
                {
                    CCLog.Log("Failed to load default font. No font supported.");
                    return (false);
                }

                float scale = 1f;
                
                if (loadedSize != 0)
                {
                    scale = fontSize / loadedSize * CCSpriteFontCache.FontScale;
                }

                if (dimensions.Equals(CCSize.Zero))
                {
                    Vector2 temp = font.MeasureString(text);
                    dimensions.Width = temp.X * scale;
                    dimensions.Height = temp.Y * scale;
                }

                var textList = new List<String>();
                var nextText = new StringBuilder();

                string[] lineList = text.Split('\n');

                StringBuilder next = new StringBuilder();
                string last = null;
                for (int j = 0; j < lineList.Length; ++j)
                {
                    string[] wordList = lineList[j].Split(' ');
                    for (int i = 0; i < wordList.Length; )
                    {
                        // Run through the list of words to create a sentence that fits in the dimensions.Width space
                        while (i < wordList.Length)
                        {
                            if ((font.MeasureString(next.ToString()).X * scale) > dimensions.Width)
                            {
                                i--;
                                break;
                            }
                            last = next.ToString();
                            if (next.Length > 0)
                            {
                                next.Append(' ');
                            }
                            next.Append(wordList[i]);
                            i++;
                        }
                        if (i == wordList.Length || i == -1) // -1 means the default width was too small for the string.
                        {
                            string nstr = next.ToString();
                            if ((font.MeasureString(nstr).X * scale) > dimensions.Width)
                            {
                                // Last line could have bleed into the margin
                                if(last != null && last.Length > 0)
                                    textList.Add(last); // Single word label has a null last which can cause problems
                                textList.Add(wordList[wordList.Length - 1]); // last word bleeds
                            }
                            else if(nstr.Length > 0)
                            {
                                textList.Add(nstr);
                            }
                        }
                        else if(last.Length > 0)
                        {
                            textList.Add(last);
                        }
                        last = null;
                        next.Length = 0;
                    }

                    textList.Add(nextText.ToString());
#if XBOX || XBOX360
                    nextText.Length = 0;
#else
                    nextText.Clear();
#endif
                }

                if (textList.Count == 0 && text.Length > 0)
                {
                    textList.Add(text);
                }

                if (dimensions.Height == 0)
                {
                    dimensions.Height = textList.Count * font.LineSpacing * scale;
                }

                //*  for render to texture
                RenderTarget2D renderTarget = CCDrawManager.CreateRenderTarget(
                    (int)dimensions.Width, (int)dimensions.Height,
                    DefaultAlphaPixelFormat, RenderTargetUsage.DiscardContents
                    );

                try
                {
                    CCDrawManager.SetRenderTarget(renderTarget);
                }
                catch (Exception)
                {
                    CCTextureCache.SharedTextureCache.RemoveUnusedTextures();
                    CCDrawManager.SetRenderTarget(renderTarget);
                }
                CCDrawManager.Clear(Color.Transparent);

                SpriteBatch sb = CCDrawManager.spriteBatch;
                sb.Begin();

                float textHeight = textList.Count * font.LineSpacing * scale;
                float nextY = 0;

                if (vAlignment == CCVerticalTextAlignment.Bottom)
                {
                    nextY = dimensions.Height - textHeight;
                }
                else if (vAlignment == CCVerticalTextAlignment.Center)
                {
                    nextY = (dimensions.Height - textHeight) / 2.0f;
                }

                for (int j = 0; j < textList.Count; ++j)
                {
                    string line = textList[j];

                    var position = new Vector2(0, nextY);

                    if (hAlignment == CCTextAlignment.Right)
                    {
                        position.X = dimensions.Width - font.MeasureString(line).X * scale;
                    }
                    else if (hAlignment == CCTextAlignment.Center)
                    {
                        position.X = (dimensions.Width - font.MeasureString(line).X * scale) / 2.0f;
                    }

                    sb.DrawString(font, line, position, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);

                    nextY += font.LineSpacing * scale;
                }

                sb.End();

                CCDrawManager.graphicsDevice.RasterizerState = RasterizerState.CullNone;
                CCDrawManager.graphicsDevice.DepthStencilState = DepthStencilState.Default;

                CCDrawManager.SetRenderTarget((RenderTarget2D)null);

                if (InitWithTexture(renderTarget, renderTarget.Format, true, false))
                {
                    m_CacheInfo.CacheType = CCTextureCacheType.String;
                    m_CacheInfo.Data = new CCStringCache()
                    {
                        Dimensions = dimensions,
                        Text = text,
                        FontName = fontName,
                        FontSize = fontSize,
                        HAlignment = hAlignment,
                        VAlignment = vAlignment
                    };

                    return true;
                }
            }
            catch (Exception ex)
            {
                CCLog.Log(ex.ToString());
            }
            return false;
        }

        public bool InitWithTexture(Texture2D texture)
        {
            return InitWithTexture(texture, texture.Format, true, false);
        }

        internal bool InitWithTexture(Texture2D texture, SurfaceFormat format, bool premultipliedAlpha, bool managed)
        {
            m_bManaged = managed;

            if (null == texture)
            {
                return false;
            }

            if (OptimizeForPremultipliedAlpha && !premultipliedAlpha)
            {
                m_Texture2D = ConvertToPremultiplied(texture, format);

                if (!m_bManaged)
                {
                    texture.Dispose();
                    m_bManaged = false;
                }
            }
            else
            {
                if (texture.Format != format)
                {
                    m_Texture2D = ConvertSurfaceFormat(texture, format);

                    if (!m_bManaged)
                    {
                        texture.Dispose();
                        m_bManaged = false;
                    }
                }
                else
                {
                    m_Texture2D = texture;
                }
            }

            m_ePixelFormat = texture.Format;
            m_uPixelsWide = texture.Width;
            m_uPixelsHigh = texture.Height;
            m_tContentSize.Width = texture.Width;
            m_tContentSize.Height = texture.Height;
            m_bHasMipmaps = texture.LevelCount > 1;
            m_bHasPremultipliedAlpha = premultipliedAlpha;

            return true;
        }

        public bool InitWithFile(string file)
        {
            m_bManaged = false;

            Texture2D texture = null;

            m_CacheInfo.CacheType = CCTextureCacheType.AssetFile;
            m_CacheInfo.Data = file;

            //TODO: may be move this functional to CCContentManager?

            var contentManager = CCContentManager.SharedContentManager;

            var loadedFile = file;

            // first try to download xnb
            if (Path.HasExtension(loadedFile))
            {
                loadedFile = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
            }

            // use WeakReference. Link for regular textures are stored in CCTextureCache
            texture = contentManager.TryLoad<Texture2D>(loadedFile, true);

            if (texture != null)
            {
                // usually xnb texture prepared as PremultipliedAlpha
                return InitWithTexture(texture, PreserveSourceSurfaceFormat ? texture.Format : DefaultAlphaPixelFormat, true, true);
            }

            // try load raw image
            if (loadedFile != file)
            {
                texture = contentManager.TryLoad<Texture2D>(file, true);

                if (texture != null)
                {
                    // not premultiplied alpha
                    return InitWithTexture(texture, PreserveSourceSurfaceFormat ? texture.Format : DefaultAlphaPixelFormat, false, true);
                }
            }

            // try load not supported format (for example tga)
            try
            {
                using (var stream = contentManager.GetAssetStream(file))
                {
                    return InitWithStream(stream, DefaultAlphaPixelFormat);
                }
            }
            catch (Exception)
            {
            }

            CCLog.Log("Texture {0} was not found.", file);
            return false;
        }

        public override void Reinit()
        {
			CCLog.Log("reinit called on {1} '{0}' {2}", ToString(), 
                m_CacheInfo.CacheType, 
				(m_CacheInfo.CacheType == CCTextureCacheType.AssetFile || m_CacheInfo.CacheType == CCTextureCacheType.String) ? m_CacheInfo.Data : string.Empty);

            Texture2D textureToDispose = null;
            if (m_Texture2D != null && !m_Texture2D.IsDisposed && !m_bManaged)
            {
                textureToDispose = m_Texture2D;
                // m_Texture2D.Dispose();
            }

            m_bManaged = false;
            m_Texture2D = null;

            if (m_CacheInfo.CacheType != CCTextureCacheType.None)
            {
                switch (m_CacheInfo.CacheType)
                {
                    case CCTextureCacheType.None:
                        return;

                    case CCTextureCacheType.AssetFile:
                        InitWithFile((string)m_CacheInfo.Data);
                        break;

                    case CCTextureCacheType.Data:
                        InitWithData((byte[])m_CacheInfo.Data, m_ePixelFormat, m_bHasMipmaps);
                        break;

                    case CCTextureCacheType.RawData:
#if NETFX_CORE
                    var methodInfo = typeof(CCTexture2D).GetType().GetTypeInfo().GetDeclaredMethod("InitWithRawData");
#else
                        var methodInfo = typeof(CCTexture2D).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m => m.Name == "InitWithRawData" && m.IsGenericMethod && m.GetParameters().Length == 7);
#endif
                        var genericMethod = methodInfo.MakeGenericMethod(m_CacheInfo.Data.GetType().GetElementType());
                        genericMethod.Invoke(this, new object[]
                        {
                            m_CacheInfo.Data,
                            m_ePixelFormat, m_uPixelsWide, m_uPixelsHigh, 
                            m_bHasPremultipliedAlpha, m_bHasMipmaps, m_tContentSize
                        });

                        //                    InitWithRawData((byte[])m_CacheInfo.Data, m_ePixelFormat, m_uPixelsWide, m_uPixelsHigh,
                        //                                    m_bHasPremultipliedAlpha, m_bHasMipmaps, m_tContentSize);
                        break;

                    case CCTextureCacheType.String:
                        var si = (CCStringCache)m_CacheInfo.Data;
                        InitWithString(si.Text, si.Dimensions, si.HAlignment, si.VAlignment, si.FontName, si.FontSize);
                        if (m_bHasMipmaps)
                        {
                            m_bHasMipmaps = false;
                            GenerateMipmap();
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (textureToDispose != null && !textureToDispose.IsDisposed)
            {
                textureToDispose.Dispose();
            }
            if (OnReInit != null)
            {
                OnReInit();
            }
        }

        #endregion

        #region Conversion

        public void GenerateMipmap()
        {
            if (!m_bHasMipmaps)
            {
                var target = new RenderTarget2D(CCDrawManager.GraphicsDevice, PixelsWide, PixelsHigh, true, PixelFormat,
                                                DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                CCDrawManager.SetRenderTarget(target);

                SpriteBatch sb = CCDrawManager.spriteBatch;

                sb.Begin();
                sb.Draw(m_Texture2D, Vector2.Zero, Color.White);
                sb.End();

                if (!m_bManaged)
                {
                    m_Texture2D.Dispose();
                }

                m_bManaged = false;
                m_Texture2D = target;

                m_bHasMipmaps = true;
            }
        }

        private Texture2D ConvertSurfaceFormat(Texture2D texture, SurfaceFormat format)
        {
            if (texture.Format == format)
            {
                return texture;
            }

            var renderTarget = new RenderTarget2D(
                CCDrawManager.GraphicsDevice,
                texture.Width, texture.Height, m_bHasMipmaps, format,
                DepthFormat.None, 0, RenderTargetUsage.DiscardContents
                );

            CCDrawManager.SetRenderTarget(renderTarget);
            CCDrawManager.Clear(Color.Transparent);
            CCDrawManager.spriteBatch.Begin(SpriteSortMode.Immediate, HasPremultipliedAlpha ? BlendState.AlphaBlend : BlendState.NonPremultiplied);
            CCDrawManager.spriteBatch.Draw(texture, new Vector2(0, 0), Color.White);
            CCDrawManager.spriteBatch.End();
            CCDrawManager.SetRenderTarget((CCTexture2D)null);

            return renderTarget;
        }

        private Texture2D ConvertToPremultiplied(Texture2D texture, SurfaceFormat format)
        {
            //Jake Poznanski - Speeding up XNA Content Load
            //http://jakepoz.com/jake_poznanski__speeding_up_xna.html

            //Setup a render target to hold our final texture which will have premulitplied alpha values
            var result = new RenderTarget2D(
                CCDrawManager.graphicsDevice,
                texture.Width, texture.Height, m_bHasMipmaps, format,
                DepthFormat.None, 0, RenderTargetUsage.DiscardContents
                );

            CCDrawManager.SetRenderTarget(result);

            CCDrawManager.Clear(Color.Transparent);

            var spriteBatch = CCDrawManager.spriteBatch;

            if (format != SurfaceFormat.Alpha8)
            {
                //Multiply each color by the source alpha, and write in just the color values into the final texture
                var blendColor = new BlendState();
                blendColor.ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green |
                                                ColorWriteChannels.Blue;

                blendColor.AlphaDestinationBlend = Blend.Zero;
                blendColor.ColorDestinationBlend = Blend.Zero;

                blendColor.AlphaSourceBlend = Blend.SourceAlpha;
                blendColor.ColorSourceBlend = Blend.SourceAlpha;

                spriteBatch.Begin(SpriteSortMode.Immediate, blendColor);
                spriteBatch.Draw(texture, texture.Bounds, Color.White);
                spriteBatch.End();
            }

            //Now copy over the alpha values from the PNG source texture to the final one, without multiplying them
            var blendAlpha = new BlendState();

            blendAlpha.ColorWriteChannels = ColorWriteChannels.Alpha;

            blendAlpha.AlphaDestinationBlend = Blend.Zero;
            blendAlpha.ColorDestinationBlend = Blend.Zero;

            blendAlpha.AlphaSourceBlend = Blend.One;
            blendAlpha.ColorSourceBlend = Blend.One;

            spriteBatch.Begin(SpriteSortMode.Immediate, blendAlpha);
            spriteBatch.Draw(texture, texture.Bounds, Color.White);
            spriteBatch.End();

            //Release the GPU back to drawing to the screen
            CCDrawManager.SetRenderTarget((CCTexture2D) null);

            return result;
        }

        #endregion

        #region Loading Texture

        private Texture2D LoadTexture(Stream stream)
        {
            return LoadTexture(stream, CCImageFormat.UnKnown);
        }

        private Texture2D LoadRawData<T>(T[] data, int width, int height, SurfaceFormat pixelFormat, bool mipMap) where T : struct
        {
            var result = new Texture2D(CCDrawManager.GraphicsDevice, width, height, mipMap, pixelFormat);
            result.SetData(data);
            return result;
        }

        private Texture2D LoadTexture(Stream stream, CCImageFormat imageFormat)
        {
            Texture2D result = null;

            if (imageFormat == CCImageFormat.UnKnown)
            {
                imageFormat = DetectImageFormat(stream);
            }

            if (imageFormat == CCImageFormat.Tiff)
            {
                result = LoadTextureFromTiff(stream);
            }

            if (imageFormat == CCImageFormat.Jpg || imageFormat == CCImageFormat.Png || imageFormat == CCImageFormat.Gif)
            {
                result = Texture2D.FromStream(CCDrawManager.GraphicsDevice, stream);
            }

            return result;
        }

        public static CCImageFormat DetectImageFormat(Stream stream)
        {
            var data = new byte[8];

            var pos = stream.Position;
            var dataLen = stream.Read(data, 0, 8);
            stream.Position = pos;

            if (dataLen >= 8)
            {
                if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47
                    && data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
                {
                    return CCImageFormat.Png;
                }
            }

            if (dataLen >= 3)
            {
                if (data[0] == 0x47 && data[1] == 0x49 && data[1] == 0x46)
                {
                    return CCImageFormat.Gif;
                }
            }

            if (dataLen >= 2)
            {
                if ((data[0] == 0x49 && data[1] == 0x49) || (data[0] == 0x4d && data[1] == 0x4d))
                {
                    return CCImageFormat.Tiff;
                }
            }

            if (dataLen >= 2)
            {
                if (data[0] == 0xff && data[1] == 0xd8)
                {
                    return CCImageFormat.Jpg;
                }
            }

            return CCImageFormat.UnKnown;
        }

        private Texture2D LoadTextureFromTiff(Stream stream)
        {
#if (WINDOWS && !WINRT)
            var tiff = Tiff.ClientOpen("file.tif", "r", stream, new TiffStream());

            var w = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            var h = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            var raster = new int[w * h];

            if (tiff.ReadRGBAImageOriented(w, h, raster, Orientation.LEFTTOP))
            {
                var result = new Texture2D(CCDrawManager.GraphicsDevice, w, h, false, SurfaceFormat.Color);
                result.SetData(raster);
                return result;
            }
			else 
			{
				return null;
			}
#elif MACOS || IOS

			return Texture2D.FromStream(CCDrawManager.GraphicsDevice, stream);
#else
			return null;
#endif
        }

        
        #endregion
    }
}

