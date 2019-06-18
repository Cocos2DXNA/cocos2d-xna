using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cocos2D
{
    public class CCLightningStreak : CCMotionStreak
    {
        private CCLightningTrack _Track;
        private float _CurrentTime = 0f;
        private bool _Cyclical = false;
        private float _StreakLifetime = 1f;

        public CCLightningStreak()
        {
            BlendFunc = CCBlendFunc.Additive;
        }

        public CCLightningStreak(CCPoint start, CCPoint end, float fadeTime, float minSegLength, float streakWidth, CCColor3B color, string pathToTexture)
            : base(fadeTime, minSegLength, streakWidth, color, pathToTexture)
        {
            _Track = new CCLightningTrack(start, end);
        }

        public CCLightningStreak(CCPoint start, CCPoint end, float fadeTime, float minSegLength, float streakWidth, CCColor3B color, CCTexture2D texture)
            : base(fadeTime, minSegLength, streakWidth, color, texture)
        {
            _Track = new CCLightningTrack(start, end);
        }

        public CCLightningStreak(CCPoint start, CCPoint end, float sway, float fadeTime, float minSegLength, float streakWidth, CCColor3B color, string pathToTexture)
            : base(fadeTime, minSegLength, streakWidth, color, pathToTexture)
        {
            _Track = new CCLightningTrack(start, end);
            _Track.Sway = sway;
        }

        public CCLightningStreak(CCPoint start, CCPoint end, float sway, float fadeTime, float minSegLength, float streakWidth, CCColor3B color, CCTexture2D texture)
            : base(fadeTime, minSegLength, streakWidth, color, texture)
        {
            _Track = new CCLightningTrack(start, end);
            _Track.Sway = sway;
        }

        public virtual float Duration
        {
            get
            {
                return (_StreakLifetime);
            }
            set
            {
                _StreakLifetime = value;
            }
        }

        /// <summary>
        /// Set to true if you want to re-start the strike when the 'time' has reached 1. The default is
        /// false, which will stop the strike and fade out the segments.
        /// </summary>
        public virtual bool Cyclical
        {
            get
            {
                return (_Cyclical);
            }
            set
            {
                _Cyclical = value;
            }
        }

        private bool _IsComplete = false;

        public override void Update(float delta)
        {
            _CurrentTime += delta;
            if (_CurrentTime > Duration)
            {
                if (_IsComplete)
                {
                    if (_Cyclical)
                    {
                        // Automatic recycle?
                        _CurrentTime = 0f;
                        _IsComplete = false;
                    }
                    else
                    {
                        // Don't update the position as this strike is just fading now
                        base.Update(delta);
                        return;
                    }
                }
                else
                {
                    _CurrentTime = Duration;
                    _IsComplete = true;
                }
            }
            Position = _Track.GetPoint(_CurrentTime / Duration);
            base.Update(delta);
        }
    }
}
