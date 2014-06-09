
using System;
namespace Cocos2D
{
    /// <summary>
    ///  Object than contains the delegate and priority of the event handler.
    /// </summary>
    public class CCTouchHandler
    {
        protected int m_nEnabledSelectors;
        protected int m_nPriority;
        protected ICCTouchDelegate m_pDelegate;

        /// <summary>
        /// delegate
        /// </summary>
        public ICCTouchDelegate Delegate
        {
            get { return m_pDelegate; }
            set { m_pDelegate = value; if (value != null) { m_nPriority = value.TouchPriority; } }
        }

        /// <summary>
        /// priority
        /// </summary>
        public int Priority
        {
            get { return m_nPriority; }
            set { m_nPriority = value; }
        }

        /// <summary>
        /// enabled selectors 
        /// </summary>
        public int EnabledSelectors
        {
            get { return m_nEnabledSelectors; }
            set { m_nEnabledSelectors = value; }
        }

        /// <summary>
        /// initializes a TouchHandler with a delegate and a priority 
        /// </summary>
        protected virtual bool InitWithDelegate(ICCTouchDelegate pDelegate, int nPriority)
        {
            m_pDelegate = pDelegate;
            m_nPriority = nPriority;
            m_nEnabledSelectors = 0;

            return true;
        }

        public CCTouchHandler(ICCTouchDelegate pDelegate)
        {
            m_pDelegate = pDelegate;
            m_nPriority = pDelegate.TouchPriority;
            m_nEnabledSelectors = 0;
        }

        public CCTouchHandler(ICCTouchDelegate pDelegate, int nPriority) : this(pDelegate)
        {
            m_nPriority = nPriority;
        }

        /// <summary>
        /// allocates a TouchHandler with a delegate and a priority 
        /// </summary>
        [Obsolete("Use the CCTouchHandler constructors", true)]
        public static CCTouchHandler Create(ICCTouchDelegate pDelegate, int nPriority)
        {
            var pHandler = new CCTouchHandler(pDelegate, nPriority);
            return pHandler;
        }
    }
}