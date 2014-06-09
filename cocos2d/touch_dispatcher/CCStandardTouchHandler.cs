
using System;
namespace Cocos2D
{
    /// <summary>
    /// It forwardes each event to the delegate.
    /// </summary>
    public class CCStandardTouchHandler : CCTouchHandler
    {
        public CCStandardTouchHandler(ICCStandardTouchDelegate pDelegate, int nPriority) : base(pDelegate, nPriority)
        {

        }
        /// <summary>
        ///  initializes a TouchHandler with a delegate and a priority
        /// </summary>
        [Obsolete("Use the constructor", true)]
        public virtual bool InitWithDelegate(ICCStandardTouchDelegate pDelegate, int nPriority)
        {
            return base.InitWithDelegate(pDelegate, nPriority);
        }

        /// <summary>
        /// allocates a TouchHandler with a delegate and a priority
        /// </summary>
        public static CCStandardTouchHandler HandlerWithDelegate(ICCStandardTouchDelegate pDelegate, int nPriority)
        {
            var pHandler = new CCStandardTouchHandler(pDelegate, nPriority);
            return pHandler;
        }
    }
}