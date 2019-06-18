using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cocos2D
{
    public class CCParallaxScrollNode : CCNode
    {
        /// <summary>
        /// This is the default speed used in box2d - 32 pixels/sec
        /// </summary>
        private const float kDefaultPTMRatio = 32f;

        private List<CCParallaxScrollOffset> m_ScrollOffsets;
        private CCSpriteBatchNode m_Batch;
        private CCSpriteSheet m_SpriteSheet;

        /// <summary>
        /// Scaling ratio for real world size to game world size.
        /// </summary>
        public float PTMRatio { get; set; }

        /// <summary>
        /// Scrolling node with no sprite batch support.
        /// </summary>
        public CCParallaxScrollNode()
        {
            m_ScrollOffsets = new List<CCParallaxScrollOffset>();
            PTMRatio = kDefaultPTMRatio;
        }

        public CCParallaxScrollNode(CCSpriteSheet sheet)
        {
            m_SpriteSheet = sheet;
            m_ScrollOffsets = new List<CCParallaxScrollOffset>();
            m_Batch = new CCSpriteBatchNode(sheet.Frames[0].Texture);
            PTMRatio = kDefaultPTMRatio;
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
            PTMRatio = kDefaultPTMRatio;
        }
        public CCParallaxScrollNode(string textureName, string plistName)
            : this(textureName)
        {
            m_SpriteSheet = new CCSpriteSheet(plistName, textureName);
        }

        public CCParallaxScrollNode(string textureName, Stream plistFile)
            : this(textureName)
        {
            m_SpriteSheet = new CCSpriteSheet(plistFile, textureName);
        }

        public void AddChild(CCSprite node, int z, CCPoint r, CCPoint p, CCPoint s)
        {
            AddChild(node, z, r, p, s, CCPoint.Zero);
        }

        public void AddChild(CCSprite node, int z, CCPoint r, CCPoint p, CCPoint s, CCPoint v)
        {
            node.AnchorPoint = CCPoint.Zero;
            CCParallaxScrollOffset obj = new CCParallaxScrollOffset(node, r, p, s, v);
            m_ScrollOffsets.Add(obj);
            if (m_Batch != null)
            {
                m_Batch.AddChild(node, z);
            }
            else
            {
                base.AddChild(node, z);
            }
        }

        public void RemoveChild(CCSprite node, bool cleanup)
        {
            int removeAt = -1;
            for (int i = 0; i < m_ScrollOffsets.Count; i++)
            {
                CCParallaxScrollOffset scrollOffset = m_ScrollOffsets[i];
                if (scrollOffset.Child == node)
                {
                    removeAt = i;
                    break;
                }
            }
            if (removeAt != -1)
            {
                m_ScrollOffsets.RemoveAt(removeAt);
            }
            if (m_Batch != null)
            {
                m_Batch.RemoveChild(node, cleanup);
            }
        }
        public override void RemoveAllChildrenWithCleanup(bool cleanup)
        {
            m_ScrollOffsets.Clear();
            if (m_Batch != null)
            {
                m_Batch.RemoveAllChildrenWithCleanup(cleanup);
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

            for (int i = m_ScrollOffsets.Count - 1; i >= 0; i--)
            {
                CCParallaxScrollOffset scrollOffset = m_ScrollOffsets[i];
                CCNode child = scrollOffset.Child;

                CCPoint relVel = scrollOffset.RelativeVelocity * PTMRatio;
                CCPoint totalVel = vel2 + relVel;

                CCPoint offset = (totalVel * dt);
                offset = offset * scrollOffset.Ratio;

                child.Position = child.Position + offset;

                if ((vel2.X < 0f && child.Position.X + child.ContentSize.Width < 0) ||
                    (vel2.X > 0 && child.Position.X > screen.Width))
                {

                    child.Position = child.Position + new CCPoint((vel2.X < 0f ? 1 : -1) * Math.Abs(scrollOffset.Offset.X), 0f);
                }

                // Positive y indicates upward movement in cocos2d
                if ((vel2.Y < 0 && child.Position.Y + child.ContentSize.Height < 0f) ||
                    (vel2.Y > 0 && child.Position.Y > screen.Height))
                {
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
                CCParallaxScrollOffset scrollOffset = m_ScrollOffsets[i];
                CCNode child = scrollOffset.Child;
                float offset = y * scrollOffset.Ratio.Y;//ccpCompMult(pos, scrollOffset.ratio);
                child.Position = new CCPoint(child.Position.X, scrollOffset.OriginalPosition.Y + offset);
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
            AddInfiniteScrollWithObjects(objects, z, ratio, pos, dir);
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
            AddInfiniteScrollWithObjects(objects, z, ratio, pos, new CCPoint(1, 0));
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
            AddInfiniteScrollWithObjects(objects, z, ratio, pos, new CCPoint(0, 1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="z"></param>
        /// <param name="p"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        public void AddInfiniteScrollWithObjects(CCSprite[] objects, int z, CCPoint ratio, CCPoint pos, CCPoint dir)
        {
            // NOTE: corrects for 1 pixel at end of each sprite to avoid thin lines appearing

            // Calculate total width and height
            float totalWidth = 0;
            float totalHeight = 0;
            for (int i = 0; i < objects.Length; i++)
            {
                CCSprite obj = objects[i];
                totalWidth += obj.ContentSize.Width - 2;//1;
                totalHeight += obj.ContentSize.Height - 2;//1;
            }

            // Position objects, add to parallax
            CCPoint currPos = pos;
            for (int i = 0; i < objects.Length; i++)
            {
                CCSprite obj = objects[i];
                AddChild(obj, z, ratio, currPos, new CCPoint(totalWidth, totalHeight));
                CCPoint nextPosOffset = new CCPoint(dir.X * (obj.ContentSize.Width - 2/*1*/), dir.Y * (obj.ContentSize.Height - 2/*1*/));
                currPos = currPos + nextPosOffset;
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