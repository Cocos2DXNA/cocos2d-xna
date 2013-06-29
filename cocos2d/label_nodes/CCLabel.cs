using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Cocos2D
{
    public partial class CCLabel : CCLabelBMFont
    {
        public static CCTexture2D m_pTexture;
        protected static bool m_bTextureDirty = true;

        protected string m_FontName;
        protected float m_FontSize;
        protected bool m_bFontDirty;

        public string FontName
        {
            get { return m_FontName; }
            set
            {
                if (m_FontName != value)
                {
                    m_FontName = value;
                    m_bFontDirty = true;
                }
            }
        }

        public float FontSize
        {
            get { return m_FontSize; }
            set
            {
                if (m_FontSize != value)
                {
                    m_FontSize = value;
                    m_bFontDirty = true;
                }
            }
        }

        public static void InitializeTTFAtlas(int width, int height)
        {
            m_nWidth = width;
            m_nHeight = height;
            m_nDepth = 4;

            m_pTexture = new CCTexture2D();
            m_pData = new int[width * height];

            m_pNodes.Clear();
            m_pNodes.Add(new ivec3() { x = 1, y = 1, z = m_nWidth - 2 });
        }

        public CCLabel()
        {
        }

        public CCLabel(string text, string fontName, float fontSize) :
            this(text, fontName, fontSize, CCSize.Zero, CCTextAlignment.Left, CCVerticalTextAlignment.Top)
        { }

        public CCLabel(string text, string fontName, float fontSize, CCTextAlignment hAlignment) :
            this(text, fontName, fontSize, CCSize.Zero, hAlignment, CCVerticalTextAlignment.Top)
        { }

        public CCLabel(string text, string fontName, float fontSize, CCTextAlignment hAlignment, CCVerticalTextAlignment vAlignment) :
            this(text, fontName, fontSize, CCSize.Zero, hAlignment, vAlignment)
        { }

        public CCLabel(string text, string fontName, float fontSize, CCSize dimensions) :
            this(text, fontName, fontSize, dimensions, CCTextAlignment.Left, CCVerticalTextAlignment.Top)
        {
        }

        public CCLabel(string text, string fontName, float fontSize, CCSize dimensions, CCTextAlignment hAlignment) :
            this(text, fontName, fontSize, dimensions, hAlignment, CCVerticalTextAlignment.Top)
        {
        }

        public CCLabel(string text, string fontName, float fontSize, CCSize dimensions, CCTextAlignment hAlignment, CCVerticalTextAlignment vAlignment)
        {
            InitWithString(text, fontName, fontSize, dimensions, hAlignment, vAlignment);
        }

        public bool InitWithString(string text, string fontName, float fontSize)
        {
            return InitWithString(text, fontName, fontSize, CCSize.Zero, CCTextAlignment.Left, CCVerticalTextAlignment.Top);
        }

        public bool InitWithString(string text, string fontName, float fontSize, CCSize dimensions, CCTextAlignment hAlignment)
        {
            return InitWithString(text, fontName, fontSize, dimensions, hAlignment, CCVerticalTextAlignment.Top);
        }

        public bool InitWithString(string text, string fontName, float fontSize, CCSize dimensions, CCTextAlignment hAlignment, CCVerticalTextAlignment vAlignment)
        {
            InitializeFont(fontName, fontSize, text);

            return base.InitWithString(text, GetFontKey(fontName, fontSize), dimensions, hAlignment, vAlignment, CCPoint.Zero, m_pTexture);
        }

        public override void Draw()
        {
            if (m_bFontDirty)
            {
                m_pConfiguration = InitializeFont(m_FontName, m_FontSize, Text);
                m_bFontDirty = false;
            }

            if (m_bTextureDirty)
            {
                m_pTexture.InitWithRawData(m_pData, SurfaceFormat.Color, m_nWidth, m_nHeight, true);
                m_bTextureDirty = false;
            }

            base.Draw();
        }

        private static string GetFontKey(string fontName, float fontSize)
        {
            return String.Format("ttf-{0}-{1}", fontName, fontSize);
        }

        #region Skyline Bottom Left

        private struct ivec3
        {
            public int x;
            public int y;
            public int z;
        }

        public struct ivec4
        {
            public int x;
            public int y;
            public int width;
            public int height;
        }

        private static CCRawList<ivec3> m_pNodes = new CCRawList<ivec3>();
        private static int m_nUsed;
        private static int m_nWidth;
        private static int m_nHeight;
        private static int m_nDepth;
        private static int[] m_pData;

        private static int Fit(int index, int width, int height)
        {
            var node = m_pNodes[index];

            var x = node.x;
            var y = node.y;
            var widthLeft = width;
            var i = index;

            if ((x + width) > (m_nWidth - 1))
            {
                return -1;
            }

            while (widthLeft > 0)
            {
                node = m_pNodes[i];

                if (node.y > y)
                {
                    y = node.y;
                }

                if ((y + height) > (m_nHeight - 1))
                {
                    return -1;
                }

                widthLeft -= node.z;

                ++i;
            }
            return y;
        }

        private static void Merge()
        {
            var nodes = m_pNodes.Elements;
            for (int i = 0, count = m_pNodes.Count; i < count - 1; ++i)
            {
                if (nodes[i].y == nodes[i + 1].y)
                {
                    nodes[i].z += nodes[i + 1].z;
                    m_pNodes.RemoveAt(i + 1);
                    --count;
                    --i;
                }
            }
        }

        public static ivec4 AllocateRegion(int width, int height)
        {
            ivec3 node, prev;
            ivec4 region = new ivec4() { x = 0, y = 0, width = width, height = height };
            int i;

            int bestHeight = int.MaxValue;
            int bestIndex = -1;
            int bestWidth = int.MaxValue;

            for (i = 0; i < m_pNodes.Count; ++i)
            {
                int y = Fit(i, width, height);

                if (y >= 0)
                {
                    node = m_pNodes[i];
                    if (((y + height) < bestHeight) || (((y + height) == bestHeight) && (node.z < bestWidth)))
                    {
                        bestHeight = y + height;
                        bestIndex = i;
                        bestWidth = node.z;
                        region.x = node.x;
                        region.y = y;
                    }
                }
            }

            if (bestIndex == -1)
            {
                region.x = -1;
                region.y = -1;
                region.width = 0;
                region.height = 0;
                return region;
            }

            //New node
            node.x = region.x;
            node.y = region.y + height;
            node.z = width;
            m_pNodes.Insert(bestIndex, node);

            for (i = bestIndex + 1; i < m_pNodes.Count; ++i)
            {
                node = m_pNodes[i];
                prev = m_pNodes[i - 1];

                if (node.x < (prev.x + prev.z))
                {
                    int shrink = prev.x + prev.z - node.x;
                    node.x += shrink;
                    node.z -= shrink;
                    if (node.z <= 0)
                    {
                        m_pNodes.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        m_pNodes[i] = node;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            Merge();

            m_nUsed += width * height;

            return region;
        }

        public static void SetRegionData(ivec4 region, int[] data, int stride)
        {
            var x = region.x;
            var y = region.y;
            var width = region.width;
            var height = region.height;

            Debug.Assert(x > 0);
            Debug.Assert(y > 0);
            Debug.Assert(x < (m_nWidth - 1));
            Debug.Assert((x + width) <= (m_nWidth - 1));
            Debug.Assert(y < (m_nHeight - 1));
            Debug.Assert((y + height) <= (m_nHeight - 1));

            var depth = m_nDepth;
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; j++)
                {
                    var b = (byte)((data[i * stride + j] & 0xFF0000) >> 16);
                    m_pData[((y + i) * m_nWidth + x) + j] = b << 24 | b << 16 | b << 8 | b;
                }
                //    Array.Copy(data, (i * stride), m_pData, ((y + i) * m_nWidth + x), width);
                //                Buffer.BlockCopy(data, (i * stride), m_pData, ((y + i) * m_nWidth + x) * depth, width * depth);
            }
        }

        #endregion
    }

}
