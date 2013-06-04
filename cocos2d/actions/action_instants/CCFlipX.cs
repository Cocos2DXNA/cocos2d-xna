﻿namespace Cocos2D
{
    public class CCFlipX : CCActionInstant
    {
        private bool m_bFlipX;

        protected CCFlipX()
        {
        }
        public CCFlipX(bool x)
        {
            InitWithFlipX(x);
        }

		protected CCFlipX(CCFlipX flipX) : base (flipX)
		{
			InitWithFlipX(m_bFlipX);
		}

        protected virtual bool InitWithFlipX(bool x)
        {
            m_bFlipX = x;
            return true;
        }

        public override void StartWithTarget(CCNode target)
        {
            base.StartWithTarget(target);
            ((CCSprite) (target)).FlipX = m_bFlipX;
        }

        public override CCFiniteTimeAction Reverse()
        {
            return new CCFlipX(!m_bFlipX);
        }

        public override object Copy(ICCCopyable pZone)
        {
            if (pZone != null)
            {
                var pRet = (CCFlipX) (pZone);
				base.Copy(pZone);
				pRet.InitWithFlipX(m_bFlipX);
				return pRet;
            }
            else
            {
                return new CCFlipX(this);
            }

        }
    }
}