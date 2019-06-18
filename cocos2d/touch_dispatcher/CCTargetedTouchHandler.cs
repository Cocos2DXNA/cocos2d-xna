
using System;
using System.Collections.Generic;

namespace Cocos2D
{
    /// <summary>
    /// Implementors of this interface will handle touches in serial order, one touch at at time.
    /// </summary>
    public class CCTargetedTouchHandler : CCTouchHandler
    {
        protected bool m_bConsumesTouches;
        protected List<CCTouch> m_pClaimedTouches;

        /// <summary>
        /// whether or not the touches are consumed by this handler. A consumed touch will not
        /// propagate to the other touch handlers.
        /// </summary>
        public bool ConsumesTouches
        {
            get { return m_bConsumesTouches; }
            set { m_bConsumesTouches = value; }
        }

        /// <summary>
        /// MutableSet that contains the claimed touches 
        /// </summary>
        public List<CCTouch> ClaimedTouches
        {
            get { return m_pClaimedTouches; }
        }

        public CCTargetedTouchHandler(ICCTargetedTouchDelegate pDelegate, int nPriority, bool bSwallow) : base(pDelegate, nPriority)
        {
            m_pClaimedTouches = new List<CCTouch>();
            m_bConsumesTouches = bSwallow;
        }
        /// <summary>
        ///  initializes a TargetedTouchHandler with a delegate, a priority and whether or not it swallows touches or not
        /// </summary>
        [Obsolete("Use the constructor", true)]
        public bool InitWithDelegate(ICCTargetedTouchDelegate pDelegate, int nPriority, bool bSwallow)
        {
            if (base.InitWithDelegate(pDelegate, nPriority))
            {
                m_pClaimedTouches = new List<CCTouch>();
                m_bConsumesTouches = bSwallow;

                return true;
            }

            return false;
        }

        /// <summary>
        /// allocates a TargetedTouchHandler with a delegate, a priority and whether or not it swallows touches or not 
        /// </summary>
        public static CCTargetedTouchHandler HandlerWithDelegate(ICCTargetedTouchDelegate pDelegate, int nPriority, bool bSwallow)
        {
            var pHandler = new CCTargetedTouchHandler(pDelegate, nPriority, bSwallow);
            return pHandler;
        }
    }
}