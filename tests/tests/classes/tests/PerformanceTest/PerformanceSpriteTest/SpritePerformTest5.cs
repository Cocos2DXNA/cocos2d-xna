using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;
using Random = Cocos2D.CCRandom;

namespace tests
{
    public class SpritePerformTest5 : SpriteMainScene
    {
        public override void doTest(CCSprite sprite)
        {
            performanceout20(sprite);
        }

        public override string title()
        {
            return string.Format("E {0} 80% out", subtestNumber);
        }

        private void performanceout20(CCSprite pSprite)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;

            if (CCRandom.Float_0_1() < 0.2f)
                pSprite.Position = new CCPoint((CCRandom.Next() % (int)size.Width), (CCRandom.Next() % (int)size.Height));
            else
                pSprite.Position = new CCPoint(-1000, -1000);
        }
    }
}
