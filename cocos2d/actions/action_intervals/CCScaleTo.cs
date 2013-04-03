namespace cocos2d
{
    public class CCScaleTo : CCActionInterval
    {
        protected float m_fDeltaX;
        protected float m_fDeltaY;
        protected float m_fEndScaleX;
        protected float m_fEndScaleY;
        protected float m_fScaleX;
        protected float m_fScaleY;
        protected float m_fStartScaleX;
        protected float m_fStartScaleY;

		public CCScaleTo (float duration, float s)
		{
			InitWithDuration(duration, s);
		}
		
		public CCScaleTo (float duration, float sx, float sy)
		{
			InitWithDuration(duration, sx, sy);
		}

		protected CCScaleTo (CCScaleTo scaleTo) : base (scaleTo)
		{
			InitWithDuration(scaleTo.m_fDuration, scaleTo.m_fEndScaleX, scaleTo.m_fEndScaleY);
		}

        protected bool InitWithDuration(float duration, float s)
        {
            if (base.InitWithDuration(duration))
            {
                m_fEndScaleX = s;
                m_fEndScaleY = s;

                return true;
            }

            return false;
        }

        protected bool InitWithDuration(float duration, float sx, float sy)
        {
            if (base.InitWithDuration(duration))
            {
                m_fEndScaleX = sx;
                m_fEndScaleY = sy;

                return true;
            }

            return false;
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (CCScaleTo) (pZone);
				base.Copy(pZone);
				
				pCopy.InitWithDuration(m_fDuration, m_fEndScaleX, m_fEndScaleY);
				
				return pCopy;
            }
            else
            {
                return new CCScaleTo(this);
            }


        }

        public override void StartWithTarget(CCNode target)
        {
            base.StartWithTarget(target);
            m_fStartScaleX = target.ScaleX;
            m_fStartScaleY = target.ScaleY;
            m_fDeltaX = m_fEndScaleX - m_fStartScaleX;
            m_fDeltaY = m_fEndScaleY - m_fStartScaleY;
        }

        public override void Update(float time)
        {
            if (m_pTarget != null)
            {
                m_pTarget.ScaleX = m_fStartScaleX + m_fDeltaX * time;
                m_pTarget.ScaleY = m_fStartScaleY + m_fDeltaY * time;
            }
        }

    }
}