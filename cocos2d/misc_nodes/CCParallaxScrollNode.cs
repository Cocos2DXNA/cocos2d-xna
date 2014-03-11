using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cocos2D
{
    public class CCParallaxScrollNode : CCNode
    {
        private List<CCParallaxScrollOffset> m_ScrollOffsets;
        private CCSpriteBatchNode m_Batch;
        private CCSpriteSheet m_SpriteSheet;

        /// <summary>
        /// Scaling ratio for real world size to game world size.
        /// </summary>
        public float PTMRatio { get; set; }

        public CCParallaxScrollNode(CCSpriteSheet sheet)
        {
            m_SpriteSheet = sheet;
            m_ScrollOffsets = new List<CCParallaxScrollOffset>();
            m_Batch = new CCSpriteBatchNode(sheet.Frames[0].Texture);
        }
        /// <summary>
        /// Constructs the infinite scrolling background node with the given texture sheet, which is the root texture
        /// of the child sprite batch node.
        /// </summary>
        /// <param name="textureName"></param>
        public CCParallaxScrollNode(string textureName)
        {
            m_ScrollOffsets = new List<CCParallaxScrollOffset>();
            m_Batch = new CCSpriteBatchNode(textureName);
            PTMRatio = 1f / 32f;
        }
        public CCParallaxScrollNode(string textureName, string plistName) : this(textureName)
        {
            m_SpriteSheet = new CCSpriteSheet(plistName, textureName);
        }

        public CCParallaxScrollNode(string textureName, Stream plistFile) : this(textureName)
        {
            m_SpriteSheet = new CCSpriteSheet(plistFile, textureName);
        }

        public void AddChild(CCSprite node, int z, CCPoint r, CCPoint p, CCPoint s) 
        {
        addChild(node, z, r, p, s, ccp(0,0));
        }

        public void AddChild(CCSprite node, int z, CCPoint r, CCPoint p, CCPoint s, CCPoint v) 
        {
node->setAnchorPoint(ccp(0,0));
        CCParallaxScrollOffset *obj = CCParallaxScrollOffset::scrollWithNode(node, r, p, s, v);
        ccArrayAppendObjectWithResize(scrollOffsets, obj);
        if (batch) {
            batch->addChild(node, z);
        } else {
            base.AddChild(node, z);
        }
        }

        public void RemoveChild(CCSprite node, bool cleanup)
        {
            for (int i = 0; i < scrollOffsets->num; i++)
            {
                CCParallaxScrollOffset* scrollOffset = (CCParallaxScrollOffset*)scrollOffsets->arr[i];
                if (scrollOffset->getChild()->isEqual(node))
                {
                    ccArrayRemoveObjectAtIndex(scrollOffsets, i);
                    break;
                }
            }
            if (batch)
            {
                batch->removeChild(node, cleanup);
            }
        }
        public void RemoveAllChildrenWithCleanup(bool cleanup)
        {
            ccArrayRemoveAllObjects(scrollOffsets);
            if (batch)
            {
                batch->removeAllChildrenWithCleanup(cleanup);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vel"></param>
        /// <param name="dt"></param>
        public void UpdateWithVelocity(CCPoint vel, float dt)
        {
            CCSize screen = CCDirector.SharedDirector.WinSize;

            CCPoint vel2 = vel * PTMRatio;

            for (int i=m_ScrollOffsets.Count - 1; i >= 0; i--) {
            CCParallaxScrollOffset scrollOffset = m_ScrollOffsets[i];
            CCNode child = scrollOffset.getChild();

            CCPoint relVel = scrollOffset.RelativeVelocity * PTMRatio;
            CCPoint totalVel = vel2 + relVel;

            CCPoint offset = (totalVel * dt) * scrollOffset.Ratio;

            child.Position = child.Position + offset;

            if ( (vel2.X < 0f && child.Position.X + child.ContentSize.Width < 0) ||
                (vel2.X > 0 && child.Position.X > screen.Width) ) {

                child.Position = child.Position + new CCPoint((vel2.X < 0f ? 1 : -1) * Math.Abs(scrollOffset.Offset.X), 0f);
            }

            // Positive y indicates upward movement in cocos2d
            if ( (vel2.Y < 0 && child.Position.Y + child.ContentSize.Height < 0f) ||
                (vel2.Y > 0 && child.Position.Y > screen.Height) ) {
                child.Position = child.Position + new CCPoint(0f, (vel2.Y < 0f ? 1f : -1f) * Math.Abs(scrollOffset.Offset.Y));
            }
        }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="dt"></param>
        public void UpdateWithYPosition(float y, float dt)
        {
            for (int i = m_ScrollOffsets.Count - 1; i >= 0; i--)
            {
                CCParallaxScrollOffset* scrollOffset = (CCParallaxScrollOffset*)scrollOffsets->arr[i];
                CCNode* child = scrollOffset->getChild();
                float offset = y * scrollOffset->getRatio().y;//ccpCompMult(pos, scrollOffset.ratio);
                child->setPosition(ccp(child->getPosition().x, scrollOffset->getOrigPosition().y + offset));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="ratio"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <param name="objects"></param>
        public void AddInfiniteScrollWithZ(int z, CCPoint ratio, CCPoint pos, CCPoint dir, CCSprite[] objects)
        {
CCArray* argArray = CCArray::arrayWithCapacity(2);
        for (CCSprite *arg = firstObject; arg != 0; arg = va_arg(args, CCSprite*)) {
            argArray->addObject(arg);
        }

        addInfiniteScrollWithObjects(argArray, z, ratio, pos, dir);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="ratio"></param>
        /// <param name="pos"></param>
        /// <param name="objects"></param>
        public void AddInfiniteScrollXWithZ(int z, CCPoint ratio, CCPoint pos, CCSprite[] objects)
        {
CCArray* argArray = CCArray::arrayWithCapacity(2);
        for (CCSprite *arg = firstObject; arg != 0; arg = va_arg(args, CCSprite*)) {
            argArray->addObject(arg);
        }
        va_end(args);

        addInfiniteScrollWithObjects(argArray, z, ratio, pos, ccp(1, 0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="ratio"></param>
        /// <param name="pos"></param>
        /// <param name="objects"></param>
        public void AddInfiniteScrollYWithZ(int z, CCPoint ratio, CCPoint pos, CCSprite[] objects)
        {
CCArray* argArray = CCArray::arrayWithCapacity(2);
        for (CCSprite *arg = firstObject; arg != 0; arg = va_arg(args, CCSprite*)) {
            argArray->addObject(arg);
        }
        va_end(args);

        addInfiniteScrollWithObjects(argArray, z, ratio, pos, ccp(0, 1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="z"></param>
        /// <param name="p"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        public void AddInfiniteScrollWithObjects(CCNode[] objects, int z, CCPoint p, CCPoint pos, CCPoint dir)
        {
// NOTE: corrects for 1 pixel at end of each sprite to avoid thin lines appearing

        // Calculate total width and height
        float totalWidth = 0;
        float totalHeight = 0;
        for (int i = 0; i < objects->count(); i++) {
            CCSprite *object = (CCSprite *)(objects->objectAtIndex(i));
            totalWidth += object->getContentSize().width - 2;//1;
            totalHeight += object->getContentSize().height - 2;//1;
        }

        // Position objects, add to parallax
        CCPoint currPos = pos;
        for (int i = 0; i < objects->count(); i++) {
            CCSprite *object = (CCSprite *)(objects->objectAtIndex(i));
            addChild(object, z, ratio, currPos, ccp(totalWidth, totalHeight));
            CCPoint nextPosOffset = ccp(dir.x * (object->getContentSize().width - 2/*1*/), dir.y * (object->getContentSize().height - 2/*1*/));
            currPos = ccpAdd(currPos, nextPosOffset);
        }
        }

void UpdateWithOffset(CCPoint delta)
 {
 CCSize screen = CCDirector::sharedDirector~~>getWinSize;
 CCPoint offsetPTM = ccpMult;
 for {
 CCParallaxScrollOffset scrollOffset = scrollOffsets~~>arr[i];
 CCNode *child = scrollOffset~~>getChild;
 CCPoint offset = ccpMult), 1);
 CCPoint newPos = ccpAdd, offset);
 CCPoint oldPos = child>getPosition;
 if && .width < 0)) {
 newPos.x = fabs.x);
 } else if && ) {
 newPos.x = fabs.x);
 }
 // Positive y indicates upward movement in cocos2d
 if && .y + child>getContentSize.height < 0)) {
 newPos.y= fabs.y);
 } else if .y > screen.height)) {
 newPos.y = fabs.y);
 }
 child>setPosition;
 }
 }
public void updateWithPosition(CCPoint pos)
 {
 CCSize screen = CCDirector::sharedDirector~~>getWinSize;
 float fractpartX, fractpartY, intpart, xScrollOffset, yScrollOffset;
 for {
 CCParallaxScrollOffset *scrollOffset = scrollOffsets~~>arr[i];
 CCNodechild = scrollOffset~~>getChild;
 CCPoint offset = ccpMult), 1);
 CCPoint newPos = ccpAdd, offset);
 CCPoint oldPos = child>getPosition;
 xScrollOffset = scrollOffset~~>getScrollOffset.x;
 yScrollOffset = scrollOffset~~>getScrollOffset.y;
 fractpartX = modff;
 newPos.x = fractpartX*xScrollOffset;
 fractpartY = modff;
 newPos.y = fractpartY*yScrollOffset;
 if && .width < 0)) {
 newPos.x*= fabs(xScrollOffset);
 } else if ((newPos.x > oldPos.x) && (newPos.x > screen.width)) {
 newPos.x = fabs;
 }
 // Positive y indicates upward movement in cocos2d
 if && .height < 0)) {
 newPos.y += fabs;
 } else if && ) {
 newPos.y= fabs(yScrollOffset);
 }

child->setPosition(newPos.x, newPos.y);
 }
 }


        public CCSpriteBatchNode BatchNode
        {
            get
            {
                return (m_Batch);
            }
            set
            {
                m_Batch = value;
            }
        }
        public List<CCParallaxScrollOffset> ScrollOffsets
        {
            get
            {
                return (m_ScrollOffsets);
            }
            set
            {
                m_ScrollOffsets = value;
            }
        }
    }
}
