using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class ZoomFlipXLeftOver : CCTransitionZoomFlipX
    {
        public ZoomFlipXLeftOver (float t, CCScene s) : base(t, s, CCTransitionOrientation.LeftOver)
        { }

    }
}
