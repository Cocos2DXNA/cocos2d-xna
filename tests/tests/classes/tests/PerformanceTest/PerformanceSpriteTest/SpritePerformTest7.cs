using cocos2d;

namespace tests
{
    public class SpritePerformTest7 : SpriteMainScene
    {
        public override void doTest(CCSprite sprite)
        {
            performanceActions20(sprite);
        }

        public override string title()
        {
            return string.Format("G {0} actions 80% out", subtestNumber);
        }

        private void performanceActions20(CCSprite pSprite)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;
            if (Random.Float_0_1() < 0.2f)
                pSprite.Position = new CCPoint((Random.Next() % (int) size.Width), (Random.Next() % (int) size.Height));
            else
                pSprite.Position = new CCPoint(-1000, -1000);

            float period = 0.5f + (Random.Next() % 1000) / 500.0f;
            CCRotateBy rot = new CCRotateBy (period, 360.0f * Random.Float_0_1());
            var rot_back = (CCActionInterval) rot.Reverse();
            CCAction permanentRotation = new CCRepeatForever (CCSequence.FromActions(rot, rot_back));
            pSprite.RunAction(permanentRotation);

            float growDuration = 0.5f + (Random.Next() % 1000) / 500.0f;
            CCActionInterval grow = CCScaleBy.Create(growDuration, 0.5f, 0.5f);
            CCAction permanentScaleLoop = new CCRepeatForever (new CCSequence (grow, grow.Reverse()));
            pSprite.RunAction(permanentScaleLoop);
        }
    }
}