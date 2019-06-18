using Microsoft.Xna.Framework;

namespace Cocos2D
{
    public class CCDisplayLinkDirector : CCDirector
    {
        private bool m_bInvalid;

        public override double AnimationInterval
        {
            get { return base.AnimationInterval; }
            set
            {
                m_dAnimationInterval = value;

                if (!m_bInvalid)
                {
                    StopAnimation();
                    StartAnimation();
                }
            }
        }

        public override void StopAnimation()
        {
            m_bInvalid = true;
        }

        public override void StartAnimation()
        {
            m_bInvalid = false;
            CCApplication.SharedApplication.AnimationInterval = m_dAnimationInterval;
        }

        public override void MainLoop(GameTime gameTime)
        {
            if (m_bPurgeDirectorInNextLoop)
            {
                PurgeDirector();
                m_bPurgeDirectorInNextLoop = false;
            }
            else if (!m_bInvalid)
            {
                DrawScene(gameTime);
            }
        }
    }
}