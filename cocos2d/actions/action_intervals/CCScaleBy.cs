namespace cocos2d
{
    public class CCScaleBy : CCScaleTo
    {

		public CCScaleBy (float duration, float s) : base (duration, s)
		{
			InitWithDuration(duration, s);
		}
		
		public CCScaleBy (float duration, float sx, float sy) : base (duration, sx, sy)
		{
			InitWithDuration(duration, sx, sy);
		}

		protected CCScaleBy (CCScaleBy scaleBy) : base (scaleBy)
		{
			InitWithDuration(scaleBy.m_fDuration, scaleBy.m_fEndScaleX, scaleBy.m_fEndScaleY);
		}

        public override void StartWithTarget(CCNode target)
        {
            base.StartWithTarget(target);
            m_fDeltaX = m_fStartScaleX * m_fEndScaleX - m_fStartScaleX;
            m_fDeltaY = m_fStartScaleY * m_fEndScaleY - m_fStartScaleY;
        }

        public override CCFiniteTimeAction Reverse()
        {
            return new CCScaleBy (m_fDuration, 1 / m_fEndScaleX, 1 / m_fEndScaleY);
        }

        public override object Copy(ICopyable pZone)
        {

            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (CCScaleBy) (pZone);
				base.Copy(pZone);
				
				pCopy.InitWithDuration(m_fDuration, m_fEndScaleX, m_fEndScaleY);
				
				return pCopy;
			}
            else
            {
                return new CCScaleBy(this);
            }

        }

    }
}