namespace Cocos2D
{

    public class CCAction : ICCCopyable
    {
        public const int kInvalidTag = -1;

        protected int m_nTag = kInvalidTag;
        protected CCNode m_pOriginalTarget;
        protected CCNode m_pTarget;

        public CCAction()
        {
        }

        protected CCAction(CCAction action)
        {
            m_nTag = action.m_nTag;
            m_pOriginalTarget = action.m_pOriginalTarget;
            m_pTarget = action.m_pTarget;
        }

        public CCNode Target
        {
            get { return m_pTarget; }
            set { m_pTarget = value; }
        }

        public CCNode OriginalTarget
        {
            get { return m_pOriginalTarget; }
        }

        public int Tag
        {
            get { return m_nTag; }
            set { m_nTag = value; }
        }

        public virtual CCAction Copy()
        {
            return (CCAction) Copy(null);
        }

        /// <summary>
        /// Copy/Duplicatae protocol for making a self copy of this object instance. If null is 
        /// given as the parameter then selfie of this instance is returned. Otherwise, the state
        /// of this instance is copied to the given target.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public virtual object Copy(ICCCopyable zone)
        {
            if (zone != null)
            {
                CCAction action = (CCAction)zone;
                action.m_pOriginalTarget = m_pOriginalTarget;
                action.m_pTarget = m_pTarget;
                action.m_nTag = m_nTag;
                return zone;
            }
            else
            {
                return new CCAction(this);
            }
        }

        public virtual bool IsDone
        {
            get { return true; }
        }

        protected internal virtual void StartWithTarget(CCNode target)
        {
            m_pOriginalTarget = m_pTarget = target;
        }

        public virtual void Stop()
        {
            m_pTarget = null;
        }

        public virtual void Step(float dt)
        {
#if DEBUG
            CCLog.Log("[Action step]. override me");
#endif
        }

        public virtual void Update(float time)
        {
#if DEBUG
            CCLog.Log("[Action update]. override me");
#endif
        }
    }
}