using System.Diagnostics;

namespace cocos2d
{
    public class CCLabelAtlas : CCAtlasNode, ICCLabelProtocol
    {
        protected char m_cMapStartChar;
        protected string m_sString = "";

        #region ICCLabelProtocol Members

        public void SetString(string label)
        {
            int len = label.Length;
            if (len > m_pTextureAtlas.TotalQuads)
            {
                m_pTextureAtlas.ResizeCapacity(len);
            }

            m_sString = label;

            UpdateAtlasValues();

            ContentSize = new CCSize(len * m_uItemWidth, m_uItemHeight);

            m_uQuadsToDraw = len;
        }

        public string GetString()
        {
            return m_sString;
        }

        #endregion

        public static CCLabelAtlas Create(string label, string fntFile)
        {
            var pRet = new CCLabelAtlas();
            pRet.InitWithString(label, fntFile);
            return pRet;
        }

        public static CCLabelAtlas Create(string label, string charMapFile, int itemWidth, int itemHeight, char startCharMap)
        {
            var pRet = new CCLabelAtlas();
            pRet.InitWithString(label, charMapFile, itemWidth, itemHeight, startCharMap);
            return pRet;
        }

        public bool InitWithString(string theString, string fntFile)
        {
            string data = CCFileUtils.GetFileData(fntFile);

            PlistDocument doc = PlistDocument.Create(data);
            var dict = doc.Root as PlistDictionary;

            Debug.Assert(dict["version"].AsInt == 1, "Unsupported version. Upgrade cocos2d version");

            string textureFilename = dict["textureFilename"].AsString;
            int width = dict["itemWidth"].AsInt / ccMacros.CC_CONTENT_SCALE_FACTOR();
            int height = dict["itemHeight"].AsInt / ccMacros.CC_CONTENT_SCALE_FACTOR();
            var startChar = (char) dict["firstChar"].AsInt;


            return InitWithString(theString, textureFilename, width, height, startChar);
        }

        public bool InitWithString(string label, string charMapFile, int itemWidth, int itemHeight, char startCharMap)
        {
            Debug.Assert(label != null);
            if (base.InitWithTileFile(charMapFile, itemWidth, itemHeight, label.Length))
            {
                m_cMapStartChar = startCharMap;
                SetString(label);
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

            float itemWidthInPixels = m_uItemWidth * ccMacros.CC_CONTENT_SCALE_FACTOR();
            float itemHeightInPixels = m_uItemHeight * ccMacros.CC_CONTENT_SCALE_FACTOR();

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

                ccV3F_C4B_T2F_Quad quad;
              
                quad.tl.texCoords.U = left;
                quad.tl.texCoords.V = top;
                quad.tr.texCoords.U = right;
                quad.tr.texCoords.V = top;
                quad.bl.texCoords.U = left;
                quad.bl.texCoords.V = bottom;
                quad.br.texCoords.U = right;
                quad.br.texCoords.V = bottom;

                quad.bl.vertices.X = i * m_uItemWidth;
                quad.bl.vertices.Y = 0.0f;
                quad.bl.vertices.Z = 0.0f;
                quad.br.vertices.X = i * m_uItemWidth + m_uItemWidth;
                quad.br.vertices.Y = 0.0f;
                quad.br.vertices.Z = 0.0f;
                quad.tl.vertices.X = i * m_uItemWidth;
                quad.tl.vertices.Y = m_uItemHeight;
                quad.tl.vertices.Z = 0.0f;
                quad.tr.vertices.X = i * m_uItemWidth + m_uItemWidth;
                quad.tr.vertices.Y = m_uItemHeight;
                quad.tr.vertices.Z = 0.0f;

                quad.tl.colors = quad.tr.colors = quad.bl.colors = quad.br.colors = new CCColor4B(m_tColor.R, m_tColor.G, m_tColor.B, m_cOpacity);

                m_pTextureAtlas.UpdateQuad(ref quad, i);
            }
        }
    }
}