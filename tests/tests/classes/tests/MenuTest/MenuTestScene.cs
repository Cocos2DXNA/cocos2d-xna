/****************************************************************************
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2008-2009 Jason Booth
Copyright (c) 2011-2012 openxlive.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    // From issue #433
    public class CocosExtensions
    {
        public static CCMenuItem CreateScaledMenuItemLabel(CCSize buttonSize, int labelPadding, float strokeSize, CCColor3B textColor, CCColor3B strokeColor, CCColor3B backColor, string labelText, Action action)
        {
            var menuItem = action != null ? new CCMenuItem(o => action()) : new CCMenuItem();

            var labelSize = new CCSize(buttonSize.Width - labelPadding, buttonSize.Height - labelPadding);

            // Add background
            var blockSprite = new CCSprite("images/Block_Back");
            blockSprite.ScaleTo(buttonSize);
            blockSprite.Color = backColor;

            menuItem.AddChild(blockSprite);
            menuItem.ContentSize = buttonSize;

            // Add label
            var labelTtf = new CCLabelTTF(labelText, "arial-24", 10);
            labelTtf.Color = textColor;

            // Add Stroke to label
            // if (strokeSize > 0) labelTtf.AddStroke(strokeSize, strokeColor);

            if (labelTtf.ContentSize.Width > labelSize.Width)
            {
                labelTtf.ScaleTo(labelSize);
            }

            menuItem.AddChild(labelTtf);

            return menuItem;
        }

        public static void AddJiggle(CCNode targetSprite)
        {
            var swingTime = CCRandom.Next(10, 31) / 100f;

            var rotateLeftAction = new CCRotateBy(swingTime, 1);
            var rotateRightAction = new CCRotateBy(swingTime * 2, -2);
            var rotateBackAction = new CCRotateBy(swingTime, 1);

            var rotateSequence = new CCSequence(rotateLeftAction, rotateRightAction, rotateBackAction);
            var repeatAction = new CCRepeatForever(rotateSequence);

            targetSprite.RunAction(repeatAction);
        }
    }

    public class MenuTestScene : TestScene
    {
        public override void runThisTest()
        {
            CCLayer pLayer1 = new MenuLayer1();
            CCLayer pLayer2 = new MenuLayer2();
            CCLayer pLayer3 = new MenuLayer3();
            CCLayer pLayer4 = new MenuLayer4();
            CCLayer pLayer5 = new MenuLayerPriorityTest();
            CCLayer pLayer6 = new MenuLayer5();

            CCLayerMultiplex layer = new CCLayerMultiplex(pLayer1, pLayer2, pLayer3, pLayer4, pLayer5, pLayer6);
            AddChild(layer, 0);

            CCDirector.SharedDirector.ReplaceScene(this);
        }
        protected override void NextTestCase()
        {
        }
        protected override void PreviousTestCase()
        {
        }
        protected override void RestTestCase()
        {
        }
    }
    enum kTag
    {
        kTagMenu = 1,
        kTagMenu0 = 0,
        kTagMenu1 = 1,
    }
}
