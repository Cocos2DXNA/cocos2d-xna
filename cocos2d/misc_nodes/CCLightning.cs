using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cocos2D
{
    public struct CCLightningBolt 
    {
        public CCPoint Start;
        public CCPoint End;
        public float Width;
        public CCColor4F BoltColor;
        public CCColor4F BoltGlowColor;
        public float StrikeTime;
        public float FadeTime;
    }

    public class CCLightning : CCDrawNode
    {
        private float m_Sway = 65f;
        private float m_Jaggedness = 1f/65f;

        /// <summary>
        /// Typical blue for lightning.
        /// </summary>
        public static readonly CCColor4F LightningBlue = new CCColor4F(0.9f, 0.8f, 1f, 1f);
        public static readonly CCColor4F LightningBlueGlow = CCColor3B.Yellow; // new CCColor4F(0.35f, 0.9f, 0.9f, 1f);

        /// <summary>
        /// The current status of the bolts being drawn.
        /// </summary>
        internal class BoltStatus 
        {
            internal CCLightningBolt Bolt;
            internal List<CCPoint> Segments;
            internal float CurrentTime;
            internal int LastSegmentIndex;
            internal List<int> DrawNodeVertexIndex;
        }

        public float Sway
        {
            get
            {
                return (m_Sway);
            }
            set
            {
                if (value < 1f)
                {
                    m_Sway = 1f;
                }
                else
                {
                    m_Sway = value;
                }
                m_Jaggedness = 1.0f / m_Sway;
            }
        }

        private float m_GlowSize = .75f;
        public virtual float GlowSize
        {
            get
            {
                return (m_GlowSize);
            }
            set
            {
                m_GlowSize = value;
            }
        }

        public virtual bool DrawShadowSegments
        {
            get;
            set;
        }

        /// <summary>
        /// The bolts to draw in this node.
        /// </summary>
        private List<BoltStatus> _Bolts = new List<BoltStatus>();

        public CCLightning()
        {
            BlendFunc = CCBlendFunc.NonPremultiplied;
            FilterPrimitivesByAlpha = true;
            Sway = 65f;
            DrawShadowSegments = false;
        }

        public override void  OnEnter()
        {
 	        base.OnEnter();
            ScheduleUpdate();
        }

        public override void  Update(float dt)
        {
 	        base.Update(dt);
            bool bDidDraw = false;
            foreach (BoltStatus bs in _Bolts)
            {
                bs.CurrentTime += dt;
                if (bs.CurrentTime > bs.Bolt.StrikeTime)
                {
                    // Fading it out
                    float time = bs.CurrentTime - bs.Bolt.StrikeTime;
                    if (time < bs.Bolt.FadeTime)
                    {
                        float fade = 1f - time / bs.Bolt.FadeTime;
                        for (int i = 0; i < bs.DrawNodeVertexIndex.Count; i++)
                        {
                            FadeToSegment(bs.DrawNodeVertexIndex[i], fade);
                        }
                        bDidDraw = true;
                    }
                    else
                    {
                        for (int i = 0; i < bs.DrawNodeVertexIndex.Count; i++)
                        {
                            FadeToSegment(bs.DrawNodeVertexIndex[i], 0f);
                        }
                    }
                }
                else
                {
                    bDidDraw = true;
                    int idx = (int)System.Math.Round(bs.CurrentTime / bs.Bolt.StrikeTime * (float)(bs.Segments.Count - 1));
                    if (idx > bs.LastSegmentIndex)
                    {
                        for (int j = bs.LastSegmentIndex; j < idx && j < bs.Segments.Count; j++)
                        {
                            CCPoint p1 = bs.Segments[j];
                            CCPoint p2 = bs.Segments[j + 1];
                            // Side segments
                            CCPoint u = p2 - p1;
                            u.Normalize();
                            if (DrawShadowSegments)
                            {
                                CCPoint perp1 = new CCPoint(-u.Y, u.X);
                                CCPoint pA = p2 + perp1 * GlowSize;
                                CCPoint pB = p1 + perp1 * GlowSize;
                                bs.DrawNodeVertexIndex.Add(DrawSegment(pA, pB, bs.Bolt.Width, bs.Bolt.BoltGlowColor * 0.25f));
                                // Next side segment
                                perp1 = new CCPoint(u.Y, u.X);
                                pA = p2 + perp1 * GlowSize;
                                pB = p1 + perp1 * GlowSize;
                                bs.DrawNodeVertexIndex.Add(DrawSegment(pA, pB, bs.Bolt.Width, bs.Bolt.BoltGlowColor * 0.25f));
                            }
                            // Main segment
                            bs.DrawNodeVertexIndex.Add(DrawSegment(p1, p2, bs.Bolt.Width, bs.Bolt.BoltColor));
                        }
                        bs.LastSegmentIndex = idx;
                    }
                }
            }
            if (!bDidDraw)
            {
                Clear();
            }
        }

        /// <summary>
        /// Removes the drawn vertices and clears out the lightning. New lightning will be drawn
        /// upon the next update.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _Bolts.Clear();
        }

        public void AddBolt(CCLightningBolt bolt) 
        {
            _Bolts.Add(new BoltStatus() { 
                Bolt = bolt, 
                Segments = CreateBolt(bolt.Start, bolt.End),
                CurrentTime = 0f,
                LastSegmentIndex = 0,
                DrawNodeVertexIndex = new List<int>()
            });
        }

        /// <summary>
        /// Creates the full set of bolt segments between the start and end points. Based upon the
        /// algorithm found at http://gamedevelopment.tutsplus.com/tutorials/how-to-generate-shockingly-good-2d-lightning-effects--gamedev-2681
        /// by Michael Hoffman.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
		protected virtual List<CCPoint> CreateBolt(CCPoint source, CCPoint dest)
		{
			var results = new List<CCPoint>();
			CCPoint tangent = dest - source;
			CCPoint normal = CCPoint.Normalize(new CCPoint(tangent.Y, -tangent.X));
			float length = tangent.Length;

			List<float> positions = new List<float>();
			positions.Add(0);

			for (int i = 0; i < length / 4; i++)
				positions.Add(CCMacros.CCRandomBetween0And1());

			positions.Sort();

			CCPoint prevPoint = source;
			float prevDisplacement = 0;
			for (int i = 1; i < positions.Count; i++)
			{
				float pos = positions[i];

				// used to prevent sharp angles by ensuring very close positions also have small perpendicular variation.
				float scale = (length * m_Jaggedness) * (pos - positions[i - 1]);

				// defines an envelope. Points near the middle of the bolt can be further from the central line.
				float envelope = pos > 0.95f ? 20f * (1f - pos) : 1f;

                float displacement = -m_Sway + 2f * m_Sway * CCMacros.CCRandomBetween0And1();
				displacement -= (displacement - prevDisplacement) * (1 - scale);
				displacement *= envelope;

				CCPoint point = source + tangent * pos + normal * displacement;
                // Core segment
				results.Add(prevPoint);
                results.Add(point);
                // Continue to the next segment.
                prevPoint = point;
				prevDisplacement = displacement;
			}

			results.Add(prevPoint);
            results.Add(dest);//, thickness));

			return results;
		}

		// Returns the point where the bolt is at a given fraction of the way through the bolt. Passing
		// zero will return the start of the bolt, and passing 1 will return the end.
		private CCPoint GetPoint(BoltStatus b, float position)
		{
			var start = b.Bolt.Start;
			float length = CCPoint.Distance(start, b.Bolt.End);
			CCPoint dir = (b.Bolt.End - start) / length;
			position *= length;

            for (int i = 0; i < b.Segments.Count; i += 2)
            {
                if (CCPoint.Dot(b.Segments[i] - start, dir) >= position)
                {
                    float lineStartPos = CCPoint.Dot(b.Segments[i] - start, dir);
                    float lineEndPos = CCPoint.Dot(b.Segments[i + 1] - start, dir);
                    float linePos = (position - lineStartPos) / (lineEndPos - lineStartPos);

                    return CCPoint.Lerp(b.Segments[i], b.Segments[i + 1], linePos);
                }
            }
            return (CCPoint.Zero);
        }
    }
}
