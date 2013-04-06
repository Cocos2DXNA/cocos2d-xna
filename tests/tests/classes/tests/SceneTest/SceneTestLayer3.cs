using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cocos2d;

namespace tests
{
    public class SceneTestLayer3 : CCLayerColor
    {
        string s_pPathGrossini = "Images/grossini";

        public SceneTestLayer3()
        {
            Init();
        }

        public override bool Init()
        {
            InitWithColor(new CCColor4B(0, 0, 255, 255));

            CCMenuItemFont item1 = CCMenuItemFont.Create("(3) Touch to pushScene (self)", item0Clicked);
            CCMenuItemFont item2 = CCMenuItemFont.Create("(3) Touch to popScene", item1Clicked);
            CCMenuItemFont item3 = CCMenuItemFont.Create("(3) Touch to popToRootScene", item2Clicked);

            CCMenu menu = CCMenu.Create(item1, item2, item3);
            menu.AlignItemsVertically();

            AddChild(menu);

            CCSize s = CCDirector.SharedDirector.WinSize;
            CCSprite sprite = new CCSprite(s_pPathGrossini);
            AddChild(sprite);
            
            sprite.Position = new CCPoint(s.Width /2, 40);
            CCActionInterval rotate = new CCRotateBy (2, 360);
            CCAction repeat = new CCRepeatForever (rotate);
            sprite.RunAction(repeat);

            Schedule(testDealloc);

            return true;
        }

        public virtual void testDealloc(float dt)
        {

        }

        public void item0Clicked(object pSender)
        {
            var newScene = CCScene.Create();
            newScene.AddChild(new SceneTestLayer3());
            CCDirector.SharedDirector.PushScene(CCTransitionFade.Create(0.5f, newScene, new CCColor3B(0, 255, 255)));
        }

        public void item1Clicked(object pSender)
        {
            CCDirector.SharedDirector.PopScene();
        }

        public void item2Clicked(object pSender)
        {
            CCDirector.SharedDirector.PopToRootScene();
        }

    }
}
