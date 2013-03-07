using cocos2d;

namespace tests
{
    public class CameraCenterTest : TestCocosNodeDemo
    {
        public CameraCenterTest()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;

            CCSprite sprite;
            CCOrbitCamera orbit;

            // LEFT-TOP
            sprite = CCSprite.Create("Images/white-512x512");
            AddChild(sprite, 0);
            sprite.Position = new CCPoint(s.Width / 5 * 1, s.Height / 5 * 1);
            sprite.Color = CCTypes.CCRed;
            sprite.TextureRect = new CCRect(0, 0, 120, 50);
            orbit = CCOrbitCamera.Create(10, 1, 0, 0, 360, 0, 0);
            sprite.RunAction(CCRepeatForever.Create(orbit));


            // LEFT-BOTTOM
            sprite = CCSprite.Create("Images/white-512x512");
            AddChild(sprite, 0, 40);
            sprite.Position = (new CCPoint(s.Width / 5 * 1, s.Height / 5 * 4));
            sprite.Color = CCTypes.CCBlue;
            sprite.TextureRect = new CCRect(0, 0, 120, 50);
            orbit = CCOrbitCamera.Create(10, 1, 0, 0, 360, 0, 0);
            sprite.RunAction(CCRepeatForever.Create(orbit));


            // RIGHT-TOP
            sprite = CCSprite.Create("Images/white-512x512");
            AddChild(sprite, 0);
            sprite.Position = (new CCPoint(s.Width / 5 * 4, s.Height / 5 * 1));
            sprite.Color = CCTypes.CCYellow;
            sprite.TextureRect = new CCRect(0, 0, 120, 50);
            orbit = CCOrbitCamera.Create(10, 1, 0, 0, 360, 0, 0);
            sprite.RunAction(CCRepeatForever.Create(orbit));

            // RIGHT-BOTTOM
            sprite = CCSprite.Create("Images/white-512x512");
            AddChild(sprite, 0, 40);
            sprite.Position = (new CCPoint(s.Width / 5 * 4, s.Height / 5 * 4));
            sprite.Color = CCTypes.CCGreen;
            sprite.TextureRect = new CCRect(0, 0, 120, 50);
            orbit = CCOrbitCamera.Create(10, 1, 0, 0, 360, 0, 0);
            sprite.RunAction(CCRepeatForever.Create(orbit));

            // CENTER
            sprite = CCSprite.Create("Images/white-512x512");
            AddChild(sprite, 0, 40);
            sprite.Position = (new CCPoint(s.Width / 2, s.Height / 2));
            sprite.Color = CCTypes.CCWhite;
            sprite.TextureRect = new CCRect(0, 0, 120, 50);
            orbit = CCOrbitCamera.Create(10, 1, 0, 0, 360, 0, 0);
            sprite.RunAction(CCRepeatForever.Create(orbit));
        }

        public override string title()
        {
            return "Camera Center test";
        }

        public override string subtitle()
        {
            return "Sprites should rotate at the same speed";
        }
    }
}