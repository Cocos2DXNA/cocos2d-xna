using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class SplitRowsDemo : CCSplitRows
    {
        public new static CCActionInterval actionWithDuration(float t)
        {
            return new CCSplitRows(9, t);
        }
    }
}
