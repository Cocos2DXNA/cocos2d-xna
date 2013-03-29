
namespace cocos2d
{
    public class CCProgressTo : CCActionInterval
    {
        protected float m_fFrom;
        protected float m_fTo;

        /// <summary>
        /// Initializes with a duration and a percent
        /// </summary>
        public bool InitWithDuration(float duration, float fPercent)
        {
            if (base.InitWithDuration(duration))
            {
                m_fTo = fPercent;

                return true;
            }

            return false;
        }

        public override object Copy(ICopyable pZone)
        {
            CCProgressTo pCopy;
            if (pZone != null)
            {
                //in case of being called at sub class
                pCopy = (CCProgressTo) (pZone);
            }
            else
            {
                pCopy = new CCProgressTo();
                pZone =  (pCopy);
            }

            base.Copy(pZone);

            pCopy.InitWithDuration(m_fDuration, m_fTo);

            return pCopy;
        }

        public override void StartWithTarget(CCNode target)
        {
            base.StartWithTarget(target);
            m_fFrom = ((CCProgressTimer) (target)).Percentage;
            // XXX: Is this correct ?
            // Adding it to support CCRepeat
            if (m_fFrom == 100)
            {
                m_fFrom = 0;
            }
        }

        public override void Update(float time)
        {
            ((CCProgressTimer) m_pTarget).Percentage = m_fFrom + (m_fTo - m_fFrom) * time;
        }

        /// <summary>
        /// Creates and initializes with a duration and a percent
        /// </summary>
        public static CCProgressTo Create(float duration, float fPercent)
        {
            var pProgressTo = new CCProgressTo();
            pProgressTo.InitWithDuration(duration, fPercent);

            return pProgressTo;
        }
    }
}