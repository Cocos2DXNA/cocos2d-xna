using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;
using Random = Cocos2D.CCRandom;

namespace tests
{
    public class SpritePerformTest1 : SpriteMainScene
    {

        public override void doTest(CCSprite sprite)
        {
            performancePosition(sprite);
        }

        public override string title()
        {
            return string.Format("A {0} position", subtestNumber);
        }

        private void performancePosition(CCSprite pSprite)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;
            pSprite.Position = new CCPoint((CCRandom.Next() % (int)size.Width), (CCRandom.Next() % (int)size.Height));
        }
    }
}
