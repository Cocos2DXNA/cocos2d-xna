using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cocos2D
{
    public class CCLightningTrack
    {
        private float m_Sway = 65f;
        private float m_Jaggedness = 1f / 65f;
        private CCPoint _Start;
        private CCPoint _End;
        private List<CCPoint> _Track;
        private float _TrackLength = 0f;
        private Dictionary<float, CCLightningTrack> _Branches;

        public CCLightningTrack(CCPoint start, CCPoint end)
        {
            _Start = start;
            _End = end;
            _Track = CreateBolt(start, end);
            CCPoint dir = end - start;
            _Length = dir.Normalize();
            _uDir = dir;
        }

        /// <summary>
        /// Returns the computed segment track of the lightning bolt.
        /// </summary>
        public List<CCPoint> Track
        {
            get
            {
                return (_Track);
            }
        }

        /// <summary>
        /// This determines how much each segment of the lightning strike will deviate from its past neighbor.
        /// The value range is [1,+inf). The jaggedness of the bolt is inversely proportional to the sway.
        /// </summary>
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
                if (_Track != null)
                {
                    _Track = CreateBolt(_Start, _End);
                }
            }
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
            List<float> seglength = new List<float>();
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

                // defines an envelope. Points near the middle of the bolt can be farther from the central line.
                float envelope = pos > 0.95f ? 20f * (1f - pos) : 1f;

                float displacement = -m_Sway + 2f * m_Sway * CCMacros.CCRandomBetween0And1();
                displacement -= (displacement - prevDisplacement) * (1 - scale);
                displacement *= envelope;

                CCPoint point = source + tangent * pos + normal * displacement;
                // Core segment
                results.Add(prevPoint);
                results.Add(point);
                // Length of this segment
                float ll = (point - prevPoint).Length;
                seglength.Add(ll);
                _TrackLength += ll;
                // Continue to the next segment.
                prevPoint = point;
                prevDisplacement = displacement;
            }

            results.Add(prevPoint);
            results.Add(dest);
            _SegmentLengths = seglength.ToArray();
            return results;
        }

        private float _Length;
        private CCPoint _uDir;
        private float[] _SegmentLengths;

        // Returns the point where the bolt is at a given fraction of the way through the bolt. Passing
        // zero will return the start of the bolt, and passing 1 will return the end.
        public virtual CCPoint GetPoint(float position)
        {
//            var start = _Start;
//            float length = CCPoint.Distance(start, _End);
//            CCPoint dir = (_End - start) / length;
            float u = position;
            position = u * _TrackLength;

            float rem = position;
            for (int i = 0; i < _SegmentLengths.Length; i++)
            {
                if (rem - _SegmentLengths[i] <= 0f)
                {
                    // Query is on this segment
                    CCPoint dir = _Track[i * 2 + 1] - _Track[i * 2];
                    dir.Normalize();
                    CCPoint pt = _Track[i * 2] + dir * rem;
                    // CCLog.Log("GetPoint() position = {0}, segment={1}, pt={2}, end={3}", position, i, pt, _End);
                    return (pt);
                }
                else
                {
                    // Query after this segment
                    rem -= _SegmentLengths[i];
                }
            }
            // New algorithm did not work, so use the old algorithm
            position = u * _Length;
            for (int i = 0; i < _Track.Count; i += 2)
            {
                if (CCPoint.Dot(_Track[i] - _Start, _uDir) >= position)
                {
                    float lineStartPos = CCPoint.Dot(_Track[i] - _Start, _uDir);
                    float lineEndPos = CCPoint.Dot(_Track[i + 1] - _Start, _uDir);
                    float linePos = (position - lineStartPos) / (lineEndPos - lineStartPos);

                    return CCPoint.Lerp(_Track[i], _Track[i + 1], linePos);
                }
            }
            // Here means we are beyond the length of the track, which may happen due to floating point 
            // precision error. Just return the end.
            return (_End);
        }
    }
}
