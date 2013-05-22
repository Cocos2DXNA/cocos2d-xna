using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class PageTurn3DDemo : CCPageTurn3D
    {
        public new static CCActionInterval actionWithDuration(float t)
        {
            CCDirector.SharedDirector.SetDepthTest(true);
            return new CCPageTurn3D (new CCGridSize(15, 10), t);
        }
    }
}
