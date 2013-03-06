using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cocos2d;

namespace tests
{
    public class Ripple3DDemo : CCRipple3D
    {
        public new static CCActionInterval actionWithDuration(float t)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;
            return CCRipple3D.Create(new CCPoint(size.Width / 2, size.Height / 2), 240, 4, 160, new CCGridSize(32, 24), t);
        }
    }
}
