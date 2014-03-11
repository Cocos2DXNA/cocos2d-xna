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

CCParallaxScrollOffset ScrollWithNode(CCNode *node, const CCPoint &r, const CCPoint &p, const CCPoint &s)
    {
        CCParallaxScrollOffset *pobSprite = new CCParallaxScrollOffset();
        if (pobSprite && pobSprite->initWithNode(node, r, p, s))
        {
            pobSprite->autorelease();
            return pobSprite;
        }
        CC_SAFE_DELETE(pobSprite);
        return NULL;
    }

    CCParallaxScrollOffset ScrollWithNode(CCNode *node, const CCPoint &r, const CCPoint &p, const CCPoint &s, const CCPoint &v)
    {
        CCParallaxScrollOffset *pobSprite = new CCParallaxScrollOffset();
        if (pobSprite && pobSprite->initWithNode(node, r, p, s, v))
        {
            pobSprite->autorelease();
            return pobSprite;
        }
        CC_SAFE_DELETE(pobSprite);
        return NULL;
    }

    public CCParallaxScrollOffset(CCNode *node, const CCPoint &r, const CCPoint &p, const CCPoint &s)
: this(node, r, p, s, CCPoint.Zero)
    {
    }

    public CCParallaxScrollOffset(CCNode *node, const CCPoint &r, const CCPoint &p, const CCPoint &s, const CCPoint &v)
    {
            child = node;
            ratio = r;
            scrollOffset = s;
            relVelocity = v;
            child->setPosition(p);
            origPosition = p;
            //currPosition = p;
            child->setAnchorPoint(ccp(0, 0));
    }
    }
}
