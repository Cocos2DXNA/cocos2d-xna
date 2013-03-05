using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cocos2d;

namespace tests
{
    public class PauseTest : ActionManagerTest
    {
        string s_pPathGrossini = "Images/grossini";
        int kTagGrossini = 1;

        public override string title()
        {
            return "Pause Test";
        }

        public override void OnEnter()
        {
            //
            // This test MUST be done in 'onEnter' and not on 'init'
            // otherwise the paused action will be resumed at 'onEnter' time
            //
            base.OnEnter();

            CCSize s = CCDirector.SharedDirector.WinSize;

            CCLabelTTF l = CCLabelTTF.Create("After 5 seconds grossini should move", "arial", 16);
            AddChild(l);
            l.Position = (new CCPoint(s.Width / 2, 245));


            //
            // Also, this test MUST be done, after [super onEnter]
            //
            CCSprite grossini = CCSprite.Create(s_pPathGrossini);
            AddChild(grossini, 0, kTagGrossini);
            grossini.Position = (new CCPoint(200, 200));

            CCAction action = CCMoveBy.Create(1, new CCPoint(150, 0));

            CCDirector.SharedDirector.ActionManager.AddAction(action, grossini, true);

            Schedule(unpause, 3);
        }

        public void unpause(float dt)
        {
            Unschedule(unpause);
            CCNode node = GetChildByTag(kTagGrossini);
            CCDirector.SharedDirector.ActionManager.ResumeTarget(node);
        }
    }
}
