using Cocos2D;

namespace tests
{
    public class TextLayer : CCLayerColor
    {
        //UxString	m_strTitle;
        private static int MAX_LAYER = 22;

        public TextLayer()
        {
            InitWithColor(CCTypes.CreateColor(32, 32, 32, 255));

            var node = new CCNode();
            CCActionInterval effect = getAction();
            node.RunAction(effect);
            AddChild(node, 1, EffectTestScene.kTagBackground);

            var bg = new CCSprite(TestResource.s_back3);
            /*node.*/AddChild(bg, 0);
            bg.AnchorPoint = new CCPoint(0.5f, 0.5f);
            bg.Position = CCDirector.SharedDirector.WinSize.Center;

            var grossini = new CCSprite(TestResource.s_pPathSister2);
            node.AddChild(grossini, 1);
            grossini.Position = new CCPoint(CCVisibleRect.Left.X + CCVisibleRect.VisibleRect.Size.Width / 3,
                                            CCVisibleRect.Center.Y);
            CCActionInterval sc = new CCScaleBy(2, 5);
            CCFiniteTimeAction sc_back = sc.Reverse();
            grossini.RunAction(new CCRepeatForever((new CCSequence(sc, sc_back))));

            var tamara = new CCSprite(TestResource.s_pPathSister1);
            node.AddChild(tamara, 1);
            tamara.Position = new CCPoint(CCVisibleRect.Left.X + 2 * CCVisibleRect.VisibleRect.Size.Width / 3,
                                          CCVisibleRect.Center.Y);
            CCActionInterval sc2 = new CCScaleBy(2, 5);
            CCFiniteTimeAction sc2_back = sc2.Reverse();
            tamara.RunAction(new CCRepeatForever((new CCSequence(sc2, sc2_back))));

            var label = new CCLabelTTF(EffectTestScene.effectsList[EffectTestScene.actionIdx], "arial", 32);

            label.Position = new CCPoint(CCVisibleRect.Center.X, CCVisibleRect.Top.Y - 80);
            AddChild(label);
            label.Tag = EffectTestScene.kTagLabel;

            var item1 = new CCMenuItemImage(TestResource.s_pPathB1, TestResource.s_pPathB2, backCallback);
            var item2 = new CCMenuItemImage(TestResource.s_pPathR1, TestResource.s_pPathR2, restartCallback);
            var item3 = new CCMenuItemImage(TestResource.s_pPathF1, TestResource.s_pPathF2, nextCallback);

            var menu = new CCMenu(item1, item2, item3);

            menu.Position = CCPoint.Zero;
            item1.Position = new CCPoint(CCVisibleRect.Center.X - item2.ContentSize.Width * 2,
                                         CCVisibleRect.Bottom.Y + item2.ContentSize.Height / 2);
            item2.Position = new CCPoint(CCVisibleRect.Center.X, CCVisibleRect.Bottom.Y + item2.ContentSize.Height / 2);
            item3.Position = new CCPoint(CCVisibleRect.Center.X + item2.ContentSize.Width * 2,
                                         CCVisibleRect.Bottom.Y + item2.ContentSize.Height / 2);

            AddChild(menu, 1);

            Schedule(checkAnim);
        }

        public CCActionInterval createEffect(int nIndex, float t)
        {
            // This fixes issue https://github.com/totallyevil/cocos2d-xna/issues/148
            // TransitionTests and TileTests may have set the DepthTest to true so we need
            // to make sure we reset it.
            CCDirector.SharedDirector.SetDepthTest(false);

            switch (nIndex)
            {
                case 0:
                    return Shaky3DDemo.actionWithDuration(t);
                case 1:
                    return Waves3DDemo.actionWithDuration(t);
                case 2:
                    return FlipX3DDemo.actionWithDuration(t);
                case 3:
                    return FlipY3DDemo.actionWithDuration(t);
                case 4:
                    return Lens3DDemo.actionWithDuration(t);
                case 5:
                    return Ripple3DDemo.actionWithDuration(t);
                case 6:
                    return LiquidDemo.actionWithDuration(t);
                case 7:
                    return WavesDemo.actionWithDuration(t);
                case 8:
                    return TwirlDemo.actionWithDuration(t);
                case 9:
                    return ShakyTiles3DDemo.actionWithDuration(t);
                case 10:
                    return ShatteredTiles3DDemo.actionWithDuration(t);
                case 11:
                    return ShuffleTilesDemo.actionWithDuration(t);
                case 12:
                    return FadeOutTRTilesDemo.actionWithDuration(t);
                case 13:
                    return FadeOutBLTilesDemo.actionWithDuration(t);
                case 14:
                    return FadeOutUpTilesDemo.actionWithDuration(t);
                case 15:
                    return FadeOutDownTilesDemo.actionWithDuration(t);
                case 16:
                    return TurnOffTilesDemo.actionWithDuration(t);
                case 17:
                    return WavesTiles3DDemo.actionWithDuration(t);
                case 18:
                    return JumpTiles3DDemo.actionWithDuration(t);
                case 19:
                    return SplitRowsDemo.actionWithDuration(t);
                case 20:
                    return SplitColsDemo.actionWithDuration(t);
                case 21:
                    return PageTurn3DDemo.actionWithDuration(t);
            }

            return null;
        }

        public CCActionInterval getAction()
        {
            CCActionInterval pEffect = createEffect(EffectTestScene.actionIdx, 3);

            return pEffect;
        }

        public void checkAnim(float dt)
        {
            CCNode s2 = GetChildByTag(EffectTestScene.kTagBackground);
            if (s2.NumberOfRunningActions() == 0 && s2.Grid != null)
                s2.Grid = null;
            ;
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public void restartCallback(object pSender)
        {
            /*newOrientation();*/
            newScene();
        }

        public void nextCallback(object pSender)
        {
            // update the action index
            EffectTestScene.actionIdx++;
            EffectTestScene.actionIdx = EffectTestScene.actionIdx % MAX_LAYER;

            /*newOrientation();*/
            newScene();
        }

        public void backCallback(object pSender)
        {
            // update the action index
            EffectTestScene.actionIdx--;
            int total = MAX_LAYER;
            if (EffectTestScene.actionIdx < 0)
                EffectTestScene.actionIdx += total;

            /*newOrientation();*/
            newScene();
        }

        public void newScene()
        {
            CCScene s = new EffectTestScene();
            CCNode child = node();
            s.AddChild(child);
            CCDirector.SharedDirector.ReplaceScene(s);
        }

        public static TextLayer node()
        {
            var pLayer = new TextLayer();

            return pLayer;
        }
    }
}