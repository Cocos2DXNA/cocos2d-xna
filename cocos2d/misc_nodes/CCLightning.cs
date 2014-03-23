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
        public float StrikeTime;
        public float FadeTime;
    }

    public class CCLightning : CCDrawNode
    {
        public const float Sway = 80;
        public const float Jaggedness = 1 / Sway;

        /// <summary>
        /// Typical blue for lightning.
        /// </summary>
        public static readonly CCColor4F LightningBlue = new CCColor4F(0.9f, 0.8f, 1f, 1f);

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

        /// <summary>
        /// The bolts to draw in this node.
        /// </summary>
        private List<BoltStatus> _Bolts = new List<BoltStatus>();

        public CCLightning()
        {
        }

        public override void  OnEnter()
        {
 	        base.OnEnter();
            ScheduleUpdate();
        }

        public override void  Update(float dt)
        {
 	        base.Update(dt);
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
                    }
                }
                else
                {
                    int idx = (int)Math.Round(bs.CurrentTime / bs.Bolt.StrikeTime * (float)(bs.Segments.Count - 1));
                    if (idx > bs.LastSegmentIndex)
                    {
                        for (int j = bs.LastSegmentIndex; j < idx && j < bs.Segments.Count; j++)
                        {
                            bs.DrawNodeVertexIndex.Add(DrawSegment(bs.Segments[j], bs.Segments[j + 1], bs.Bolt.Width, bs.Bolt.BoltColor));
                        }
                        bs.LastSegmentIndex = idx;
                    }
                }
            }
        }

        /// <summary>
        /// Removes the drawn vertices and clears out the lightning. New lightning will be drawn
        /// upon the next update.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            foreach (BoltStatus bs in _Bolts)
            {
                bs.CurrentTime = 0f;
                bs.Segments = CreateBolt(bs.Bolt.Start, bs.Bolt.End);
                bs.LastSegmentIndex = 0;
                bs.DrawNodeVertexIndex.Clear();
            }
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

		protected static List<CCPoint> CreateBolt(CCPoint source, CCPoint dest)
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
				float scale = (length * Jaggedness) * (pos - positions[i - 1]);

				// defines an envelope. Points near the middle of the bolt can be further from the central line.
				float envelope = pos > 0.95f ? 20 * (1 - pos) : 1;

                float displacement = -Sway + 2f * Sway * CCMacros.CCRandomBetween0And1();
				displacement -= (displacement - prevDisplacement) * (1 - scale);
				displacement *= envelope;

				CCPoint point = source + tangent * pos + normal * displacement;
				results.Add(prevPoint);
                results.Add(point); // , thickness));
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
