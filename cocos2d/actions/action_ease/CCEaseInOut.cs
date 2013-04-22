
using System;

namespace cocos2d
{
    public class CCEaseInOut : CCEaseRateAction
    {
        public CCEaseInOut (CCActionInterval pAction, float fRate) : base (pAction, fRate)
        { }

        public CCEaseInOut (CCEaseInOut easeInOut) : base (easeInOut)
        { }

        public override void Update(float time)
        {
            time *= 2;

            if (time < 1)
            {
                m_pOther.Update(0.5f * (float) Math.Pow(time, m_fRate));
            }
            else
            {
                m_pOther.Update(1.0f - 0.5f * (float) Math.Pow(2 - time, m_fRate));
            }
        }

        public override object Copy(ICopyable pZone)
        {

            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as CCEaseInOut;
                pCopy.InitWithAction((CCActionInterval) (m_pOther.Copy()), m_fRate);
                
                return pCopy;
            }
            else
            {
                return new CCEaseInOut(this);
            }

        }

        public override CCFiniteTimeAction Reverse()
        {
            return new CCEaseInOut((CCActionInterval) m_pOther.Reverse(), m_fRate);
        }


    }
}