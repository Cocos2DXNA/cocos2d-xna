using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cocos2d;

namespace tests
{
    public class AtlasBitmapColor : AtlasDemo
    {
        ccColor3B ccBLUE = new ccColor3B
        {
            r = 0,
            g = 0,
            b = 255
        };

        ccColor3B ccRED = new ccColor3B
        {
            r = 255,
            g = 0,
            b = 0
        };

        ccColor3B ccGREEN = new ccColor3B
        {
            r = 0,
            g = 255,
            b = 0
        };
        public AtlasBitmapColor()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;

            CCLabelBMFont label = null;
            label = CCLabelBMFont.Create("Blue", "fonts/bitmapFontTest5.fnt");
            label.Color = ccBLUE;
            AddChild(label);
            label.Position = new CCPoint(s.Width / 2, s.Height / 4);
            label.AnchorPoint = new CCPoint(0.5f, 0.5f);

            label = CCLabelBMFont.Create("Red", "fonts/bitmapFontTest5.fnt");
            AddChild(label);
            label.Position = new CCPoint(s.Width / 2, 2 * s.Height / 4);
            label.AnchorPoint = new CCPoint(0.5f, 0.5f);
            label.Color = ccRED;

            label = CCLabelBMFont.Create("G", "fonts/bitmapFontTest5.fnt");
            AddChild(label);
            label.Position = new CCPoint(s.Width / 2, 3 * s.Height / 4);
            label.AnchorPoint = new CCPoint(0.5f, 0.5f);
            label.Color = ccGREEN;
            label.SetString("Green");
        }

        public override string title()
        {
            return "CCLabelBMFont";
        }

        public override string subtitle()
        {
            return "Testing color";
        }
    }
}
