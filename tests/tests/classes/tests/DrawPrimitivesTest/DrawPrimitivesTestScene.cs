using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class DrawPrimitivesTestScene : TestScene
    {
        private static int sceneIdx = -1;
        private static int MAX_LAYER = 6;

        public override void runThisTest()
        {
            CCLayer pLayer = nextTestAction();
            AddChild(pLayer);
            CCDirector.SharedDirector.ReplaceScene(this);
        }

        public static CCLayer createTestLayer(int nIndex)
        {
            switch (nIndex)
            {
                case 0:
                    return new DrawPrimitivesTest();
                case 1:
                    return new DrawNodeTest();
                case 2:
                    return new DrawPrimitivesWithRenderTextureTest();
                case 3:
                    return new DrawPrimtivesLightningTest();
                case 4:
                    return new DrawPrimitivesRoundRectTest();
                case 5:
                    return new DrawPrimitivesRoundRectSpriteTest();
            }
            return null;
        }

        protected override void NextTestCase()
        {
            nextTestAction();
        }

        protected override void PreviousTestCase()
        {
            backTestAction();
        }

        protected override void RestTestCase()
        {
            restartTestAction();
        }

        public static CCLayer nextTestAction()
        {
            sceneIdx++;
            sceneIdx = sceneIdx % MAX_LAYER;
            CCLayer pLayer = createTestLayer(sceneIdx);
            return pLayer;
        }

        public static CCLayer backTestAction()
        {
            sceneIdx--;
            int total = MAX_LAYER;
            if (sceneIdx < 0)
                sceneIdx += total;
            CCLayer pLayer = createTestLayer(sceneIdx);
            return pLayer;
        }

        public static CCLayer restartTestAction()
        {
            CCLayer pLayer = createTestLayer(sceneIdx);
            return pLayer;
        }
    }
}
