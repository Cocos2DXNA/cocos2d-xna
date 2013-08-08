﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cocos2D
{
    public class CCDrawNode : CCNode
    {
        private CCRawList<VertexPositionColor> m_pVertices;
        private CCBlendFunc m_sBlendFunc;
        private bool m_bDirty;

        public CCDrawNode()
        {
            Init();
        }

        public CCBlendFunc BlendFunc
        {
            get { return m_sBlendFunc; }
            set { m_sBlendFunc = value; }
        }

        public override bool Init()
        {
            base.Init();

            m_sBlendFunc = CCBlendFunc.AlphaBlend;
            m_pVertices = new CCRawList<VertexPositionColor>(512);

            return true;
        }

        /** draw a dot at a position, with a given radius and color */

        public void DrawDot(CCPoint pos, float radius, CCColor4F color)
        {
            var cl = new Color(color.R, color.G, color.B, color.A);

            var a = new VertexPositionColor(new Vector3(pos.X - radius, pos.Y - radius, 0), cl); //{-1.0, -1.0}
            var b = new VertexPositionColor(new Vector3(pos.X - radius, pos.Y + radius, 0), cl); //{-1.0,  1.0}
            var c = new VertexPositionColor(new Vector3(pos.X + radius, pos.Y + radius, 0), cl); //{ 1.0,  1.0}
            var d = new VertexPositionColor(new Vector3(pos.X + radius, pos.Y - radius, 0), cl); //{ 1.0, -1.0}

            m_pVertices.Add(a);
            m_pVertices.Add(b);
            m_pVertices.Add(c);

            m_pVertices.Add(a);
            m_pVertices.Add(c);
            m_pVertices.Add(d);

            m_bDirty = true;
        }

        /** draw a segment with a radius and color */

        public void DrawSegment(CCPoint from, CCPoint to, float radius, CCColor4F color)
        {
            var cl = new Color(color.R, color.G, color.B, color.A);

            var a = from;
            var b = to;

            var n = CCPoint.Normalize(CCPoint.Perp(a - b));
            var t = CCPoint.Perp(n);

            var nw = n * radius;
            var tw = t * radius;
            var v0 = b - (nw + tw);
            var v1 = b + (nw - tw);
            var v2 = b - nw;
            var v3 = b + nw;
            var v4 = a - nw;
            var v5 = a + nw;
            var v6 = a - (nw - tw);
            var v7 = a + (nw + tw);

            m_pVertices.Add(new VertexPositionColor(v0, cl)); //__t(v2fneg(v2fadd(n, t)))
            m_pVertices.Add(new VertexPositionColor(v1, cl)); //__t(v2fsub(n, t))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))}

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v1, cl)); //__t(v2fsub(n, t))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_pVertices.Add(new VertexPositionColor(v6, cl)); //__t(v2fsub(t, n))
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_pVertices.Add(new VertexPositionColor(v6, cl)); //__t(v2fsub(t, n))
            m_pVertices.Add(new VertexPositionColor(v7, cl)); //__t(v2fadd(n, t))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_bDirty = true;
        }

        /** draw a polygon with a fill color and line color */

        private struct ExtrudeVerts
        {
            public CCPoint offset;
            public CCPoint n;
        }

        public void DrawCircle(CCPoint center, float radius, CCColor4B color)
        {
            DrawCircle(center, radius, CCMacros.CCDegreesToRadians(360f), 360, color);
        }

        public void DrawCircle(CCPoint center, float radius, float angle, int segments, CCColor4B color)
        {
            float increment = MathHelper.Pi * 2.0f / segments;
            double theta = 0.0;

            CCPoint v1;
            CCPoint v2 = CCPoint.Zero;
            List<CCPoint> verts = new List<CCPoint>();

            for (int i = 0; i < segments; i++)
            {
                v1 = center + new CCPoint((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius;
                v2 = center + new CCPoint((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment)) * radius;
                verts.Add(v1);
                theta += increment;
            }
            CCColor4F cf = new CCColor4F(color.R/255f, color.G/255f, color.B/255f, color.A/255f);
            DrawPolygon(verts.ToArray(), verts.Count, cf, 0, new CCColor4F(0f, 0f, 0f, 0f));
        }

        public void DrawRect(CCRect rect, CCColor4B color)
        {
            float x1 = rect.MinX;
            float y1 = rect.MinY;
            float x2 = rect.MaxX;
            float y2 = rect.MaxY;
            CCPoint[] pt = new CCPoint[] { 
                new CCPoint(x1,y1), new CCPoint(x2,y1), new CCPoint(x2,y2), new CCPoint(x1,y2)
            };
            CCColor4F cf = new CCColor4F(color.R/255f, color.G/255f, color.B/255f, color.A/255f);
            DrawPolygon(pt, 4, cf, 0, new CCColor4F(0f, 0f, 0f, 0f));
        }

        public void DrawPolygon(CCPoint[] verts, int count, CCColor4F fillColor, float borderWidth,
                                CCColor4F borderColor)
        {
            var extrude = new ExtrudeVerts[count];

            for (int i = 0; i < count; i++)
            {
                var v0 = verts[(i - 1 + count) % count];
                var v1 = verts[i];
                var v2 = verts[(i + 1) % count];

                var n1 = CCPoint.Normalize(CCPoint.Perp(v1 - v0));
                var n2 = CCPoint.Normalize(CCPoint.Perp(v2 - v1));

                var offset = (n1 + n2) * (1.0f / (CCPoint.Dot(n1, n2) + 1.0f));
                extrude[i] = new ExtrudeVerts() {offset = offset, n = n2};
            }

            bool outline = (fillColor.A > 0.0f && borderWidth > 0.0f);

            float inset = (!outline ? 0.5f : 0.0f);
            
            for (int i = 0; i < count - 2; i++)
            {
                var v0 = verts[0] - (extrude[0].offset * inset);
                var v1 = verts[i + 1] - (extrude[i + 1].offset * inset);
                var v2 = verts[i + 2] - (extrude[i + 2].offset * inset);

                m_pVertices.Add(new VertexPositionColor(v0, fillColor)); //__t(v2fzero)
                m_pVertices.Add(new VertexPositionColor(v1, fillColor)); //__t(v2fzero)
                m_pVertices.Add(new VertexPositionColor(v2, fillColor)); //__t(v2fzero)
            }

            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var v0 = verts[i];
                var v1 = verts[j];

                var n0 = extrude[i].n;

                var offset0 = extrude[i].offset;
                var offset1 = extrude[j].offset;

                if (outline)
                {
                    var inner0 = (v0 - (offset0 * borderWidth));
                    var inner1 = (v1 - (offset1 * borderWidth));
                    var outer0 = (v0 + (offset0 * borderWidth));
                    var outer1 = (v1 + (offset1 * borderWidth));

                    m_pVertices.Add(new VertexPositionColor(inner0, borderColor)); //__t(v2fneg(n0))
                    m_pVertices.Add(new VertexPositionColor(inner1, borderColor)); //__t(v2fneg(n0))
                    m_pVertices.Add(new VertexPositionColor(outer1, borderColor)); //__t(n0)

                    m_pVertices.Add(new VertexPositionColor(inner0, borderColor)); //__t(v2fneg(n0))
                    m_pVertices.Add(new VertexPositionColor(outer0, borderColor)); //__t(n0)
                    m_pVertices.Add(new VertexPositionColor(outer1, borderColor)); //__t(n0)
                }
                else
                {
                    var inner0 = (v0 - (offset0 * 0.5f));
                    var inner1 = (v1 - (offset1 * 0.5f));
                    var outer0 = (v0 + (offset0 * 0.5f));
                    var outer1 = (v1 + (offset1 * 0.5f));

                    m_pVertices.Add(new VertexPositionColor(inner0, fillColor)); //__t(v2fzero)
                    m_pVertices.Add(new VertexPositionColor(inner1, fillColor)); //__t(v2fzero)
                    m_pVertices.Add(new VertexPositionColor(outer1, fillColor)); //__t(n0)

                    m_pVertices.Add(new VertexPositionColor(inner0, fillColor)); //__t(v2fzero)
                    m_pVertices.Add(new VertexPositionColor(outer0, fillColor)); //__t(n0)
                    m_pVertices.Add(new VertexPositionColor(outer1, fillColor)); //__t(n0)
                }
            }

            m_bDirty = true;
        }

        /** Clear the geometry in the node's buffer. */

        public void Clear()
        {
            m_pVertices.Clear();
        }

        public override void Draw()
        {
            if (m_bDirty)
            {
                //TODO: Set vertices to buffer
                m_bDirty = false;
            }

            CCDrawManager.TextureEnabled = false;
            CCDrawManager.BlendFunc(m_sBlendFunc);
            CCDrawManager.DrawPrimitives(PrimitiveType.TriangleList, m_pVertices.Elements, 0, m_pVertices.Count / 3);
        }
    }
}
