using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class ZoomFlipXRightOver : CCTransitionZoomFlipX
    {
        public ZoomFlipXRightOver (float t, CCScene s) : base(t, s, CCTransitionOrientation.RightOver)
        { }

    }
}
