
using System.Collections.Generic;

namespace Cocos2D
{

    /// <summary>
    /// Delegates that implement this interface handle groups of touches in a single pass.
    /// </summary>
    public interface ICCStandardTouchDelegate : ICCTouchDelegate
    {
        // optional
        void TouchesBegan(List<CCTouch> pTouches);
        void TouchesMoved(List<CCTouch> pTouches);
        void TouchesEnded(List<CCTouch> pTouches);
        void TouchesCancelled(List<CCTouch> pTouches);
    }
}