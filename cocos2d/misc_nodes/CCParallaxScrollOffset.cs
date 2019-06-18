using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cocos2D
{
    public class CCParallaxScrollOffset
    {
        public CCPoint Offset { get; set; }
        public CCPoint OriginalPosition { get; set; }
        public CCPoint RelativeVelocity { get; set; }
        public CCPoint Ratio { get; set; }
        public CCPoint Position { get; set; }
        public CCPoint CurrentPosition { get; set; }
        public CCNode Child { get; set; }

        public CCParallaxScrollOffset(CCNode node, CCPoint r, CCPoint p, CCPoint s)
            : this(node, r, p, s, CCPoint.Zero)
        {
        }

        public CCParallaxScrollOffset(CCNode node, CCPoint r, CCPoint p, CCPoint s, CCPoint v)
        {
            Child = node;
            Ratio = r;
            Offset = s;
            RelativeVelocity = v;
            Child.Position = p;
            OriginalPosition = p;
            //currPosition = p;
            Child.AnchorPoint = CCPoint.AnchorLowerLeft;
        }
    }
}
