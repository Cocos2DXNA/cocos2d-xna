
namespace Cocos2D
{
    public interface ICCTouchDelegate
    {
        /// <summary>
        /// Used to prioritize touch dispatching. Higher values mean the delegate will receive touches sooner 
        /// than lower valued delegates.
        /// </summary>
        int TouchPriority { get; }
        /// <summary>
        /// A visibility parameter that determines if touches are delivered to a delegate.
        /// </summary>
        bool VisibleForTouches { get; set; }
    }
}