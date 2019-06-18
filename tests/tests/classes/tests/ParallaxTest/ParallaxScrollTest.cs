using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class ParallaxScrollTest : ParallaxDemo
    {
        private CCParallaxScrollNode parallax = null;

        public ParallaxScrollTest()
        {
            CCSize screen = CCDirector.SharedDirector.WinSize;
            parallax = new CCParallaxScrollNode();
            CCSprite land1 = new CCSprite("Images/land_green");
            CCSprite land2 = new CCSprite("Images/land_green");
            parallax.AddInfiniteScrollXWithZ(0, new CCPoint(0.5f, 0.2f), CCPoint.Zero, new CCSprite[] { land1, land2 });

            CCSprite land3 = new CCSprite("Images/land_grey");
            CCSprite land4 = new CCSprite("Images/land_grey");
            parallax.AddInfiniteScrollXWithZ(-2, new CCPoint(0.05f, 0.2f), new CCPoint(0f, 60f), new CCSprite[] { land3, land4 });

            CCSprite clouds1 = new CCSprite("Images/clouds");
            CCSprite clouds2 = new CCSprite("Images/clouds");
            parallax.AddInfiniteScrollXWithZ(1, new CCPoint(0.1f, 0.1f), new CCPoint(0f, screen.Height - clouds1.ContentSize.Height), new CCSprite[] { clouds1, clouds2 });

            for (int i = 0; i < 10; i++)
            {
                CCSprite mountain = new CCSprite("Images/mountain_grey");
                CCPoint pos = new CCPoint(CCMacros.CCRandomBetween0And1() * land1.ContentSize.Width * 2f, (0.1f + 0.24f * CCMacros.CCRandomBetween0And1()) * screen.Height);

                float speedMountainX = 0.15f + CCMacros.CCRandomBetween0And1() * 0.1f;
                parallax.AddChild(mountain, -1, new CCPoint(speedMountainX, .015f), pos, new CCPoint(land1.ContentSize.Width * 2f, 0));

                mountain.Scale = 0.6f + CCMacros.CCRandomBetween0And1() * 0.4f;
            }

            CCSprite sky = new CCSprite("Images/sky_evening");
            sky.AnchorPoint = CCPoint.AnchorLowerLeft;
            AddChild(parallax);
            AddChild(sky, -1);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ScheduleUpdate();
        }

        public override void Update(float dt)
        {
            parallax.UpdateWithVelocity(new CCPoint(-0.5f * 32f, 0f), dt);
        }
        public override string title()
        {
            return "ParallaxScrollNode Test";
        }

    }
}
