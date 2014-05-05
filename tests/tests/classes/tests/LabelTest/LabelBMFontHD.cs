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
}
