using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace tests
{
    public class LabelBMFontHD : AtlasDemo
    {
        public LabelBMFontHD()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;

            // CCLabelBMFont
            CCLabelBMFont label1 = new CCLabelBMFont("TESTING RETINA DISPLAY", "fonts/konqa32.fnt");
            AddChild(label1);
            label1.Position = new CCPoint(s.Width / 2, s.Height / 2);
        }

        public override string title()
        {
            return "Testing Retina Display BMFont";
        }

        public override string subtitle()
        {
            return "loading arista16 or arista16-hd";
        }
    }

    public class LabelBMFontHDMemoryLeak : AtlasDemo
    {
        public LabelBMFontHDMemoryLeak()
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ScheduleUpdate();
        }

        private float elapsed = 0f;
        private CCLabelBMFont label1;

        public override void Update(float dt)
        {
            elapsed += dt;
            if (elapsed > 0.5f)
            {
                elapsed = 0f;
                // CCLabelBMFont
                if (label1 != null)
                {
                    RemoveChild(label1);
                }
                CCSize s = CCDirector.SharedDirector.WinSize;
                float x = s.Width * CCMacros.CCRandomBetween0And1();
                float y = s.Height * CCMacros.CCRandomBetween0And1();
                label1 = new CCLabelBMFont(string.Format("{0:N2},{1:N2} @ MEMORY LEAK", x, y), "fonts/konqa32.fnt");
                AddChild(label1);
                label1.Position = new CCPoint(x, y);
            }
        }
        public override string title()
        {
            return "Testing LabelBMFont";
        }

        public override string subtitle()
        {
            return "Looking for memory leaks";
        }
    }

    public class LabelBMFontHDMemoryLeak2 : AtlasDemo
    {
        public LabelBMFontHDMemoryLeak2()
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ScheduleUpdate();
        }

        private float elapsed = 0f;
        private CCNode label1;

        public override void Update(float dt)
        {
            elapsed += dt;
            if (elapsed > 0.5f)
            {
                elapsed = 0f;
                // CCLabelBMFont
                if (label1 != null)
                {
                    RemoveChild(label1);
                }
                CCNode node = new CCNode();
                CCSize s = CCDirector.SharedDirector.WinSize;
                float x = s.Width * CCMacros.CCRandomBetween0And1();
                float y = s.Height * CCMacros.CCRandomBetween0And1();
                label1 = new CCLabelBMFont(string.Format("{0:N2},{1:N2} @ Child Mem Leak", x, y), "fonts/konqa32.fnt");
                node.AddChild(label1);
                label1.Position = new CCPoint(x, y);
                AddChild(node);
                label1 = node;
            }
        }
        public override string title()
        {
            return "Testing LabelBMFont";
        }

        public override string subtitle()
        {
            return "Looking for memory leaks with child label in node";
        }
    }

    public class LabelBMFontHDMemoryLeak3 : AtlasDemo
    {
        public LabelBMFontHDMemoryLeak3()
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ScheduleUpdate();
        }

        private float elapsed = 0f;
        private CCNode label1;

        public override void Update(float dt)
        {
            elapsed += dt;
            if (elapsed > 0.5f)
            {
                elapsed = 0f;
                // CCLabelBMFont
                if (label1 != null)
                {
                    RemoveChild(label1);
                }
                CCNode node = new CCNode();
                CCSize s = CCDirector.SharedDirector.WinSize;
                float x = s.Width * CCMacros.CCRandomBetween0And1();
                float y = s.Height * CCMacros.CCRandomBetween0And1();
                label1 = new CCLabelBMFont(string.Format("{0:N2},{1:N2} @ Mem Leak Ctor", x,y), "fonts/konqa32.fnt", 255f, CCTextAlignment.Right, CCPoint.Zero);
                node.AddChild(label1);
                label1.Position = new CCPoint(x, y);
                AddChild(node);
                label1 = node;
            }
        }
        public override string title()
        {
            return "Testing LabelBMFont";
        }

        public override string subtitle()
        {
            return "Looking for memory leaks with full ctor";
        }
    }

    public class LabelBMFontHDMemoryLeak4 : AtlasDemo
    {
        public LabelBMFontHDMemoryLeak4()
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ScheduleUpdate();
        }

        private float elapsed = 0f;
        private CCNode label1;

        public override void Update(float dt)
        {
            elapsed += dt;
            if (elapsed > 1f)
            {
                elapsed = 0f;
                // CCLabelBMFont
                if (label1 != null)
                {
                    RemoveChild(label1);
                }
                CCNode node = new CCNode();
                CCSize s = CCDirector.SharedDirector.WinSize;
                float x = s.Width * CCMacros.CCRandomBetween0And1();
                float y = s.Height * CCMacros.CCRandomBetween0And1();
                label1 = new CCLabelBMFont(string.Format("{0:N2},{1:N2} @ Mem Leak Ctor", x, y), "fonts/konqa32.fnt", 255f, CCTextAlignment.Right, CCPoint.Zero);
                node.AddChild(label1);
                label1.Position = new CCPoint(x, y);
                AddChild(node);
                label1 = node;
                // Start - test case for memory leak mentioned at https://cocos2dxna.codeplex.com/discussions/544032
                node.Scale = 2f;
                //--> This action causes the leak
                CCScaleTo acScale = new CCScaleTo(0.1f, 1);
                CCDelayTime acShow = new CCDelayTime(0.1f);
                CCSplitRows acFadeOut = new CCSplitRows(0.1f, 20);
                CCRemoveSelf acRemove = new CCRemoveSelf(true);
                CCSequence seq = new CCSequence(acScale, acShow, acFadeOut, acRemove);
                node.RunAction(seq);
            }
        }
        public override string title()
        {
            return "Testing LabelBMFont";
        }

        public override string subtitle()
        {
            return "Looking for memory leaks with full ctor";
        }
    }
}
