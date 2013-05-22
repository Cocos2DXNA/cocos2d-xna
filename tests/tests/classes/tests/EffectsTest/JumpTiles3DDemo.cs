using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class JumpTiles3DDemo : CCJumpTiles3D
    {
        public new static CCActionInterval actionWithDuration(float t)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;
            return new CCJumpTiles3D(2, 30, new CCGridSize(15, 10), t);
        }
    }
}
