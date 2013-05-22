using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class WavesTiles3DDemo : CCWavesTiles3D
    {
        public new static CCActionInterval actionWithDuration(float t)
        {
            return new CCWavesTiles3D(4, 120, new CCGridSize(15, 10), t);
        }
    }
}
