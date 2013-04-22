
using System;

namespace cocos2d
{
    public class CCEaseOut : CCEaseRateAction
    {

        public CCEaseOut (CCActionInterval pAction, float fRate) : base (pAction, fRate)
        { }

        public CCEaseOut (CCEaseOut easeOut) : base (easeOut)
        { }

        public override void Update(float time)
        {
            m_pOther.Update((float) (Math.Pow(time, 1 / m_fRate)));
        }

        public override object Copy(ICopyable pZone)
        {

            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (CCEaseOut) (pZone);
                pCopy.InitWithAction((CCActionInterval) (m_pOther.Copy()), m_fRate);
                
                return pCopy;
            }
            else
            {
                return new CCEaseOut(this);
            }

        }

        public override CCFiniteTimeAction Reverse()
        {
            return new CCEaseOut((CCActionInterval)m_pOther.Reverse(), 1 / m_fRate);
        }

    }
}