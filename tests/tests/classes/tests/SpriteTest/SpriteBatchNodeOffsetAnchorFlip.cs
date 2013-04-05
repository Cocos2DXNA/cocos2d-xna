using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cocos2d;

namespace tests
{
    public class SpriteBatchNodeOffsetAnchorFlip : SpriteTestDemo
    {
        public SpriteBatchNodeOffsetAnchorFlip()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;

            for (int i = 0; i < 3; i++)
            {
                CCSpriteFrameCache cache = CCSpriteFrameCache.SharedSpriteFrameCache;
                cache.AddSpriteFramesWithFile("animations/grossini.plist");
                cache.AddSpriteFramesWithFile("animations/grossini_gray.plist", "animations/grossini_gray");

                //
                // Animation using Sprite batch
                //
                CCSprite sprite = new CCSprite("grossini_dance_01.png");
                sprite.Position = (new CCPoint(s.Width / 4 * (i + 1), s.Height / 2));

                CCSprite point = new CCSprite("Images/r1");
                point.Scale = 0.25f;
                point.Position = sprite.Position;
                AddChild(point, 200);

                switch (i)
                {
                    case 0:
                        sprite.AnchorPoint = new CCPoint(0, 0);
                        break;
                    case 1:
                        sprite.AnchorPoint = new CCPoint(0.5f, 0.5f);
                        break;
                    case 2:
                        sprite.AnchorPoint = (new CCPoint(1, 1));
                        break;
                }

                point.Position = sprite.Position;

                CCSpriteBatchNode spritebatch = CCSpriteBatchNode.Create("animations/grossini");
                AddChild(spritebatch);

                var animFrames = new List<CCSpriteFrame>();
                string tmp = "";
                for (int j = 0; j < 14; j++)
                {
                    string temp = "";
                    if ( i + 1<10)
                    {
                        temp = "0" + (i+1);
                    }
                    else
                    {
                        temp = (i + 1).ToString();
                    }
                    tmp = string.Format("grossini_dance_{0}.png", temp);
                    CCSpriteFrame frame = cache.SpriteFrameByName(tmp);
                    animFrames.Add(frame);
                }

                CCAnimation animation = CCAnimation.Create(animFrames, 0.3f);
                sprite.RunAction(new CCRepeatForever (new CCAnimate (animation)));

                animFrames = null;

                CCFlipY flip = CCFlipY.Create(true);
                CCFlipY flip_back = CCFlipY.Create(false);
                CCDelayTime delay = new CCDelayTime (1);
                CCFiniteTimeAction seq = CCSequence.FromActions((CCFiniteTimeAction)delay, (CCFiniteTimeAction)flip, (CCFiniteTimeAction)delay.Copy(null), (CCFiniteTimeAction)flip_back);
                sprite.RunAction(new CCRepeatForever ((CCActionInterval)seq));

                spritebatch.AddChild(sprite, i);
            }
        }

        public override string title()
        {
            return "SpriteBatchNode offset + anchor + flip";
        }
        public override string subtitle()
        {
            return "issue #1078";
        }
    }
}
