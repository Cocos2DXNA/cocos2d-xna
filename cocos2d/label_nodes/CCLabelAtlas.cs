using System.Diagnostics;
using System;

namespace Cocos2D
{
    public class CCLabelAtlas : CCAtlasNode, ICCLabelProtocol
    {
        protected char m_cMapStartChar;
        protected string m_sString = "";

        #region ICCLabelProtocol Members
        public string Text
        {
            get { return m_sString; }
            set 
            {            
                // TODO: Check for null????
                int len = value.Length;
                if (len > m_pTextureAtlas.TotalQuads)
                {
                    m_pTextureAtlas.ResizeCapacity(len);
                }
                
                m_sString = value;
                
                UpdateAtlasValues();
                
                ContentSize = new CCSize(len * m_uItemWidth, m_uItemHeight);
                
                m_uQuadsToDraw = len;

            }
        }

        [Obsolete("Use Label Property")]
        public void SetString(string label)
        {
            Text = label;
        }

        [Obsolete("Use Label Property")]
        public string GetString() 
        {
            return Text;
        }

        #endregion

        internal CCLabelAtlas()
        {
            SetIgnoreContentScaleFactor(true);
        }

        public CCLabelAtlas (string label, string fntFile)
        {
            InitWithString(label, fntFile);
        }

        public CCLabelAtlas (string label, string charMapFile, int itemWidth, int itemHeight, char startCharMap)
        {
            InitWithString(label, charMapFile, itemWidth, itemHeight, startCharMap);
        }

        public bool InitWithString(string theString, string fntFile)
        {
            string data = CCFileUtils.GetFileData(fntFile);

            PlistDocument doc = new PlistDocument(data);
            var dict = doc.Root as PlistDictionary;

            Debug.Assert(dict["version"].AsInt == 1, "Unsupported version. Upgrade cocos2d version");

            string textureFilename = dict["textureFilename"].AsString;
            int width = (int)Math.Ceiling(dict["itemWidth"].AsInt / CCMacros.CCContentScaleFactor());
            int height = (int)Math.Ceiling(dict["itemHeight"].AsInt / CCMacros.CCContentScaleFactor());
            var startChar = (char) dict["firstChar"].AsInt;

            return InitWithString(theString, textureFilename, width, height, startChar);
        }

        public bool InitWithString(string label, string charMapFile, int itemWidth, int itemHeight, char startCharMap)
        {
            Debug.Assert(label != null);
            if (base.InitWithTileFile(charMapFile, itemWidth, itemHeight, label.Length))
            {
                m_cMapStartChar = startCharMap;
                Text = (label);
                return true;
            }
            return false;
        }

        public bool InitWithString(string label, CCTexture2D texture, int itemWidth, int itemHeight, char startCharMap)
        {
            Debug.Assert(label != null);
            if (base.InitWithTexture(texture, itemWidth, itemHeight, label.Length))
            {
                m_cMapStartChar = startCharMap;
                Text = (label);
                return true;
            }
            return false;
        }

        public override void UpdateAtlasValues()
        {
            int n = m_sString.Length;

            CCTexture2D texture = m_pTextureAtlas.Texture;

            float textureWide = texture.PixelsWide;
            float textureHigh = texture.PixelsHigh;

            float itemWidthInPixels = m_uItemWidth * CCMacros.CCContentScaleFactor();
            float itemHeightInPixels = m_uItemHeight * CCMacros.CCContentScaleFactor();
            if (m_bIgnoreContentScaleFactor)
            {
                itemWidthInPixels = m_uItemWidth;
                itemHeightInPixels = m_uItemHeight;
            }

            for (int i = 0; i < n; i++)
            {
                var a = (char) (m_sString[i] - m_cMapStartChar);
                float row = (a % m_uItemsPerRow);
                float col = (a / m_uItemsPerRow);

#if CC_FIX_ARTIFACTS_BY_STRECHING_TEXEL
    // Issue #938. Don't use texStepX & texStepY
            float left		= (2 * row * itemWidthInPixels + 1) / (2 * textureWide);
            float right		= left + (itemWidthInPixels * 2 - 2) / (2 * textureWide);
            float top		= (2 * col * itemHeightInPixels + 1) / (2 * textureHigh);
            float bottom	= top + (itemHeightInPixels * 2 - 2) / (2 * textureHigh);
#else
                float left = row * itemWidthInPixels / textureWide;
                float right = left + itemWidthInPixels / textureWide;
                float top = col * itemHeightInPixels / textureHigh;
                float bottom = top + itemHeightInPixels / textureHigh;
#endif
                // ! CC_FIX_ARTIFACTS_BY_STRECHING_TEXEL

                CCV3F_C4B_T2F_Quad quad = new CCV3F_C4B_T2F_Quad();
              
                quad.TopLeft.TextureCoordinate.X = left;
                quad.TopLeft.TextureCoordinate.Y = top;
                quad.TopRight.TextureCoordinate.X = right;
                quad.TopRight.TextureCoordinate.Y = top;
                quad.BottomLeft.TextureCoordinate.X = left;
                quad.BottomLeft.TextureCoordinate.Y = bottom;
                quad.BottomRight.TextureCoordinate.X = right;
                quad.BottomRight.TextureCoordinate.Y = bottom;

                quad.BottomLeft.Position.X = i * m_uItemWidth;
                quad.BottomLeft.Position.Y = 0.0f;
                quad.BottomLeft.Position.Z = 0.0f;
                quad.BottomRight.Position.X = i * m_uItemWidth + m_uItemWidth;
                quad.BottomRight.Position.Y = 0.0f;
                quad.BottomRight.Position.Z = 0.0f;
                quad.TopLeft.Position.X = i * m_uItemWidth;
                quad.TopLeft.Position.Y = m_uItemHeight;
                quad.TopLeft.Position.Z = 0.0f;
                quad.TopRight.Position.X = i * m_uItemWidth + m_uItemWidth;
                quad.TopRight.Position.Y = m_uItemHeight;
                quad.TopRight.Position.Z = 0.0f;


                quad.TopLeft.Color = quad.TopRight.Color = quad.BottomLeft.Color = quad.BottomRight.Color =
                    new CCColor4B(_displayedColor.R, _displayedColor.G, _displayedColor.B, _displayedOpacity);

                m_pTextureAtlas.UpdateQuad(ref quad, i);
            }
        }
    }
}