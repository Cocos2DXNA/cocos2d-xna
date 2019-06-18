using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class Lens3DDemo : CCLens3D
    {
        public new static CCActionInterval actionWithDuration(float t)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;
            return new CCLens3D(t, new CCGridSize(15, 10), new CCPoint(size.Width / 4, size.Height / 4), 240);
        }
    }
}
