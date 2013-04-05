using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cocos2d;
using System.Diagnostics;

namespace tests
{
    public class SpriteBatchNodeReorder : SpriteTestDemo
    {
        public SpriteBatchNodeReorder()
        {
            List<Object> a = new List<Object>(10);
            CCSpriteBatchNode asmtest = CCSpriteBatchNode.Create("animations/ghosts");

            for (int i = 0; i < 10; i++)
            {
                CCSprite s1 = new CCSprite(asmtest.Texture, new CCRect(0, 0, 50, 50));
                a.Add(s1);
                asmtest.AddChild(s1, 10);
            }

            for (int i = 0; i < 10; i++)
            {
                if (i != 5)
                {
                    asmtest.ReorderChild((CCNode) (a[i]), 9);
                }
            }

            int prev = -1;
            var children = asmtest.Children;
            CCSprite child;
            foreach (var item in children)
            {
                child = (CCSprite)item;
                if (child == null)
                    break;

                int currentIndex = child.AtlasIndex;
                Debug.Assert(prev == currentIndex - 1, "Child order failed");
                ////----UXLOG("children %x - atlasIndex:%d", child, currentIndex);
                prev = currentIndex;
            }

            prev = -1;
            var sChildren = asmtest.Descendants;
            foreach (var item in sChildren)
            {
                child = (CCSprite)item;
                if (child == null)
                    break;

                int currentIndex = child.AtlasIndex;
                Debug.Assert(prev == currentIndex - 1, "Child order failed");
                ////----UXLOG("descendant %x - atlasIndex:%d", child, currentIndex);
                prev = currentIndex;
            }
        }

        public override string title()
        {
            return "SpriteBatchNode: reorder #1";
        }

        public override string subtitle()
        {
            return "Should not crash";
        }
    }
}
