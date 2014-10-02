using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace tests
{
    public class BaseDrawNodeTest : CCLayer
    {
        public virtual void Setup()
        {
        }

        public virtual string title()
        {
            return "Draw Demo";
        }

        public virtual string subtitle()
        {
            return "";
        }

        public override bool Init()
        {
            base.Init();

            CCSize s = CCDirector.SharedDirector.WinSize;

            var label = new CCLabelTTF(title(), "arial", 32);
            AddChild(label, 1);
            label.Position = (new CCPoint(s.Width / 2, s.Height - 50));

            string subtitle_ = subtitle();
            if (subtitle_.Length > 0)
            {
                var l = new CCLabelTTF(subtitle_, "arial", 16);
                AddChild(l, 1);
                l.Position = (new CCPoint(s.Width / 2, s.Height - 80));
            }

            var item1 = new CCMenuItemImage(TestResource.s_pPathB1, TestResource.s_pPathB2, backCallback);
            var item2 = new CCMenuItemImage(TestResource.s_pPathR1, TestResource.s_pPathR2, restartCallback);
            var item3 = new CCMenuItemImage(TestResource.s_pPathF1, TestResource.s_pPathF2, nextCallback);

            var menu = new CCMenu(item1, item2, item3);

            menu.Position = new CCPoint(0, 0);
            item1.Position = new CCPoint(s.Width / 2 - 100, 30);
            item2.Position = new CCPoint(s.Width / 2, 30);
            item3.Position = new CCPoint(s.Width / 2 + 100, 30);

            AddChild(menu, 1);

            return true;
        }

        public void restartCallback(object pSender)
        {
            CCScene s = new DrawPrimitivesTestScene();
            s.AddChild(DrawPrimitivesTestScene.restartTestAction());
            CCDirector.SharedDirector.ReplaceScene(s);
        }

        public void nextCallback(object pSender)
        {
            CCScene s = new DrawPrimitivesTestScene();
            s.AddChild(DrawPrimitivesTestScene.nextTestAction());
            CCDirector.SharedDirector.ReplaceScene(s);
        }

        public void backCallback(object pSender)
        {
            CCScene s = new DrawPrimitivesTestScene();
            s.AddChild(DrawPrimitivesTestScene.backTestAction());
            CCDirector.SharedDirector.ReplaceScene(s);
        }
    }

    #region Lightning Test
    public class DrawPrimtivesLightningTest : BaseDrawNodeTest
    {
        public override string subtitle()
        {
            return "Lightning - Basic Bolt - Touch To Strike";
        }
        public DrawPrimtivesLightningTest() 
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            CCLightning lightning = new CCLightning();
            AddChild(lightning, 10, 55);
            TouchMode = CCTouchMode.OneByOne;
            TouchEnabled = true;
        }

        public override void TouchEnded(CCTouch touch)
        {
            base.TouchEnded(touch);
            CCLightning node = (CCLightning)GetChildByTag(55);
            node.AddBolt(new CCLightningBolt()
            {
                BoltColor = CCLightning.LightningBlue,
                BoltGlowColor = CCLightning.LightningBlue,
                Start = CCDirector.SharedDirector.WinSize.Center,
                End = ConvertToNodeSpace(touch.Location),
                StrikeTime = (2f * CCMacros.CCRandomBetween0And1()),
                FadeTime = 0.15f,
                Width = .5f
            });
        }
        public override bool TouchBegan(CCTouch touch)
        {
            CCPoint pt = ConvertToNodeSpace(touch.Location);
            if (pt.X > 0f && pt.X < CCDirector.SharedDirector.WinSize.Width)
            {
                if (pt.Y > 0f && pt.Y < CCDirector.SharedDirector.WinSize.Height)
                {
                    return (true);
                }
            }
            return (false);
        }
    }
    #endregion

    #region Render Target Test
    public class DrawPrimitivesWithRenderTextureTest : BaseDrawNodeTest
    {
        public DrawPrimitivesWithRenderTextureTest()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            CCRenderTexture text = new CCRenderTexture((int)s.Width, (int)s.Height);

            CCDrawNode draw = new CCDrawNode();
            text.AddChild(draw, 10);
            text.Begin();
            // Draw polygons
            CCPoint[] points = new CCPoint[]
                {
                    new CCPoint(s.Height / 4, 0),
                    new CCPoint(s.Width, s.Height / 5),
                    new CCPoint(s.Width / 3 * 2, s.Height)
                };
            draw.DrawPolygon(points, points.Length, new CCColor4F(1, 0, 0, 0.5f), 4, new CCColor4F(0, 0, 1, 1));
            text.End();
            AddChild(text, 24);
        }
    }
    #endregion

    #region Draw Primitives
    public class DrawPrimitivesTest : BaseDrawNodeTest
    {
        public override void Draw()
        {
            base.Draw();

            CCApplication app = CCApplication.SharedApplication;
            CCSize s = CCDirector.SharedDirector.WinSize;

            CCDrawingPrimitives.Begin();

            // draw a simple line
            CCDrawingPrimitives.DrawLine(new CCPoint(0, 0), new CCPoint(s.Width, s.Height),
                                         new CCColor4B(255, 255, 255, 255));

            // line: color, width, aliased
            CCDrawingPrimitives.DrawLine(new CCPoint(0, s.Height), new CCPoint(s.Width, 0),
                                         new CCColor4B(255, 0, 0, 255));

            // draw big point in the center
            CCDrawingPrimitives.DrawPoint(new CCPoint(s.Width / 2, s.Height / 2), 64, new CCColor4B(0, 0, 255, 128));

            // draw 4 small points
            CCPoint[] points = {new CCPoint(60, 60), new CCPoint(70, 70), new CCPoint(60, 70), new CCPoint(70, 60)};
            CCDrawingPrimitives.DrawPoints(points, 4, 4, new CCColor4B(0, 255, 255, 255));

            // draw a green circle with 10 segments
            CCDrawingPrimitives.DrawCircle(new CCPoint(s.Width / 2, s.Height / 2), 100, 0, 10, false,
                                           new CCColor4B(0, 255, 0, 255));

            // draw a green circle with 50 segments with line to center
            CCDrawingPrimitives.DrawCircle(new CCPoint(s.Width / 2, s.Height / 2), 50, CCMacros.CCDegreesToRadians(90),
                                           50, true, new CCColor4B(0, 255, 255, 255));


            // draw an arc within rectangular region
            CCDrawingPrimitives.DrawArc(new CCRect(200, 200, 100, 200), 0, 180,
                                        new CCColor4B(Microsoft.Xna.Framework.Color.AliceBlue));

            // draw an ellipse within rectangular region
            CCDrawingPrimitives.DrawEllipse(new CCRect(500, 200, 100, 200), new CCColor4B(255, 0, 0, 255));

            // draw an arc within rectangular region
            CCDrawingPrimitives.DrawPie(new CCRect(350, 0, 100, 100), 20, 100,
                                        new CCColor4B(Microsoft.Xna.Framework.Color.AliceBlue));

            // draw an arc within rectangular region
            CCDrawingPrimitives.DrawPie(new CCRect(347, -5, 100, 100), 120, 260,
                                        new CCColor4B(Microsoft.Xna.Framework.Color.Aquamarine));

            // open yellow poly
            CCPoint[] vertices =
                {
                    new CCPoint(0, 0), new CCPoint(50, 50), new CCPoint(100, 50), new CCPoint(100, 100),
                    new CCPoint(50, 100)
                };
            CCDrawingPrimitives.DrawPoly(vertices, 5, false, new CCColor4B(255, 255, 0, 255));

            // filled poly
            CCPoint[] filledVertices =
                {
                    new CCPoint(0, 120), new CCPoint(50, 120), new CCPoint(50, 170),
                    new CCPoint(25, 200), new CCPoint(0, 170)
                };
            CCDrawingPrimitives.DrawSolidPoly(filledVertices, 5, new CCColor4B(128, 128, 255, 255));

            // closed purble poly
            CCPoint[] vertices2 = {new CCPoint(30, 130), new CCPoint(30, 230), new CCPoint(50, 200)};
            CCDrawingPrimitives.DrawPoly(vertices2, 3, true, new CCColor4B(255, 0, 255, 255));

            // draw quad bezier path
            CCDrawingPrimitives.DrawQuadBezier(new CCPoint(0, s.Height),
                                               new CCPoint(s.Width / 2, s.Height / 2),
                                               new CCPoint(s.Width, s.Height),
                                               50,
                                               new CCColor4B(255, 0, 255, 255));

            // draw cubic bezier path
            CCDrawingPrimitives.DrawCubicBezier(new CCPoint(s.Width / 2, s.Height / 2),
                                                new CCPoint(s.Width / 2 + 30, s.Height / 2 + 50),
                                                new CCPoint(s.Width / 2 + 60, s.Height / 2 - 50),
                                                new CCPoint(s.Width, s.Height / 2), 100,
                                                new CCColor4B(255, 0, 255, 255));

            //draw a solid polygon
            CCPoint[] vertices3 =
                {
                    new CCPoint(60, 160), new CCPoint(70, 190), new CCPoint(100, 190),
                    new CCPoint(90, 160)
                };
            CCDrawingPrimitives.DrawSolidPoly(vertices3, 4, new CCColor4B(255, 255, 0, 255));

            CCDrawingPrimitives.End();

        }
    }
    #endregion

    #region Draw Node
    public class DrawNodeTest : BaseDrawNodeTest
    {
        public override bool Init()
        {
            base.Init();

            CCSize s = CCDirector.SharedDirector.WinSize;

            CCDrawNode draw = new CCDrawNode();
            AddChild(draw, 10);

            // Draw 10 circles
            for (int i = 0; i < 10; i++)
            {
                draw.DrawDot(new CCPoint(s.Width / 2, s.Height / 2), 10 * (10 - i),
                             new CCColor4F(CCRandom.Float_0_1(), CCRandom.Float_0_1(), CCRandom.Float_0_1(), 1));
            }

            // Draw polygons
            CCPoint[] points = new CCPoint[]
                {
                    new CCPoint(s.Height / 4, 0),
                    new CCPoint(s.Width, s.Height / 5),
                    new CCPoint(s.Width / 3 * 2, s.Height)
                };
            draw.DrawPolygon(points, points.Length, new CCColor4F(1, 0, 0, 0.5f), 4, new CCColor4F(0, 0, 1, 1));

            // star poly (triggers buggs)
            {
                const float o = 80;
                const float w = 20;
                const float h = 50;
                CCPoint[] star = new CCPoint[]
                    {
                        new CCPoint(o + w, o - h), new CCPoint(o + w * 2, o), // lower spike
                        new CCPoint(o + w * 2 + h, o + w), new CCPoint(o + w * 2, o + w * 2), // right spike
                        //				{o +w, o+w*2+h}, {o,o+w*2},					// top spike
                        //				{o -h, o+w}, {o,o},							// left spike
                    };

                draw.DrawPolygon(star, star.Length, new CCColor4F(1, 0, 0, 0.5f), 1, new CCColor4F(0, 0, 1, 1));
            }

            // star poly (doesn't trigger bug... order is important un tesselation is supported.
            {
                const float o = 180;
                const float w = 20;
                const float h = 50;
                var star = new CCPoint[]
                    {
                        new CCPoint(o, o), new CCPoint(o + w, o - h), new CCPoint(o + w * 2, o), // lower spike
                        new CCPoint(o + w * 2 + h, o + w), new CCPoint(o + w * 2, o + w * 2), // right spike
                        new CCPoint(o + w, o + w * 2 + h), new CCPoint(o, o + w * 2), // top spike
                        new CCPoint(o - h, o + w), // left spike
                    };

                draw.DrawPolygon(star, star.Length, new CCColor4F(1, 0, 0, 0.5f), 1, new CCColor4F(0, 0, 1, 1));
            }


            // Draw segment
            draw.DrawSegment(new CCPoint(20, s.Height), new CCPoint(20, s.Height / 2), 10, new CCColor4F(0, 1, 0, 1));

            draw.DrawSegment(new CCPoint(10, s.Height / 2), new CCPoint(s.Width / 2, s.Height / 2), 40,
                             new CCColor4F(1, 0, 1, 0.5f));

            return true;
        }
    }
    #endregion

#region RoundRectPrimitive

    public class RoundRectPrimitive : CCNodeRGBA
    {
        // This comes from https://cocos2dxna.codeplex.com/discussions/568573#post1309173
        // This class does not work with the transform.

        private const float kappa = 0.552228474f;
        protected int _radius;
        private int _cornerSegments;
        private CCPoint[] _vertices;
        private CCPoint[] _polyVertices;

        public RoundRectPrimitive(CCSize size, int radius)
        {
            _radius = radius;
            _cornerSegments = 4;
            ContentSize = size;
        }
        public RoundRectPrimitive(CCSize size, int radius, int cornerSegments)
        {
            _radius = radius;
            _cornerSegments = cornerSegments;
            ContentSize = size;
        }

        public override CCSize ContentSize
        {
            set
            {
                base.ContentSize = value;
                InitVertices();
            }
        }
        private void AppendCubicBezier(int startPoint, CCPoint[] vertices, CCPoint origin, CCPoint control1, CCPoint control2, CCPoint destination, int segments)
        {
            float t = 0;
            for (int i = 0; i < segments; i++)
            {
                float x = (float)Math.Pow(1 - t, 3) * origin.X + 3.0f * (float)Math.Pow(1 - t, 2) * t * control1.X + 3.0f * (1 - t) * t * t * control2.X + t * t * t * destination.X;
                float y = (float)Math.Pow(1 - t, 3) * origin.Y + 3.0f * (float)Math.Pow(1 - t, 2) * t * control1.Y + 3.0f * (1 - t) * t * t * control2.Y + t * t * t * destination.Y;
                vertices[startPoint + i] = new CCPoint(x * Scale, y * Scale);
                t += 1.0f / segments;
            }
        }
        public void InitVertices()
        {
            // Creates the vertices all relative to 0,0
            _vertices = new CCPoint[16];

            // Bottom left
            _vertices[0] = new CCPoint(1, 1 + _radius);
            _vertices[1] = new CCPoint(1, 1 + _radius * (1 - kappa));
            _vertices[2] = new CCPoint(1 + _radius * (1 - kappa), 1);
            _vertices[3] = new CCPoint(1 + _radius, 1);

            // Bottom right
            _vertices[4] = new CCPoint(ContentSize.Width - _radius - 1, 1);
            _vertices[5] = new CCPoint(ContentSize.Width - _radius * (1 - kappa) - 1, 1);
            _vertices[6] = new CCPoint(ContentSize.Width - 1, 1 + _radius * (1 - kappa));
            _vertices[7] = new CCPoint(ContentSize.Width - 1, 1 + _radius);

            // Top right
            _vertices[8] = new CCPoint(ContentSize.Width - 1, 1 + ContentSize.Height - _radius);
            _vertices[9] = new CCPoint(ContentSize.Width - 1, 1 + ContentSize.Height - _radius * (1 - kappa));
            _vertices[10] = new CCPoint(ContentSize.Width - _radius * (1 - kappa) - 1, ContentSize.Height - 1);
            _vertices[11] = new CCPoint(ContentSize.Width - _radius - 1, ContentSize.Height - 1);

            // Top left
            _vertices[12] = new CCPoint(1 + _radius, ContentSize.Height - 1);
            _vertices[13] = new CCPoint(1 + _radius * (1 - kappa), ContentSize.Height - 1);
            _vertices[14] = new CCPoint(1, ContentSize.Height - _radius * (1 - kappa) - 1);
            _vertices[15] = new CCPoint(1, ContentSize.Height - _radius - 1);

            _polyVertices = new CCPoint[4 * _cornerSegments + 1];
            AppendCubicBezier(0 * _cornerSegments, _polyVertices, _vertices[0], _vertices[1], _vertices[2], _vertices[3], _cornerSegments);
            AppendCubicBezier(1 * _cornerSegments, _polyVertices, _vertices[4], _vertices[5], _vertices[6], _vertices[7], _cornerSegments);
            AppendCubicBezier(2 * _cornerSegments, _polyVertices, _vertices[8], _vertices[9], _vertices[10], _vertices[11], _cornerSegments);
            AppendCubicBezier(3 * _cornerSegments, _polyVertices, _vertices[12], _vertices[13], _vertices[14], _vertices[15], _cornerSegments);
            _polyVertices[4 * _cornerSegments] = _vertices[0];
        }
        public override void Visit()
        {
            base.Visit();

            float dx = Position.X - AnchorPoint.X * ContentSize.Width;
            float dy = Position.Y - AnchorPoint.Y * ContentSize.Height;
            CCPoint offset = new CCPoint(dx, dy);
            CCPoint[] pv = new CCPoint[_polyVertices.Length];
            for (int i = 0; i < _polyVertices.Length; i++)
            {
                pv[i] = _polyVertices[i] + offset;
            }
            CCDrawingPrimitives.Begin();
            CCDrawingPrimitives.DrawSolidPoly(pv, 4 * _cornerSegments + 1, Color);
            CCDrawingPrimitives.DrawCubicBezier(_vertices[0] + offset, _vertices[1] + offset, _vertices[2] + offset, _vertices[3] + offset, _cornerSegments, Color);
            CCDrawingPrimitives.DrawCubicBezier(_vertices[4] + offset, _vertices[5] + offset, _vertices[6] + offset, _vertices[7] + offset, _cornerSegments, Color);
            CCDrawingPrimitives.DrawCubicBezier(_vertices[8] + offset, _vertices[9] + offset, _vertices[10] + offset, _vertices[11] + offset, _cornerSegments, Color);
            CCDrawingPrimitives.DrawCubicBezier(_vertices[12] + offset, _vertices[13] + offset, _vertices[14] + offset, _vertices[15] + offset, _cornerSegments, Color);

            CCDrawingPrimitives.End();

            CCDrawingPrimitives.Begin();
            CCDrawingPrimitives.DrawLine(_vertices[3] + offset, _vertices[4] + offset, Color);
            CCDrawingPrimitives.DrawLine(_vertices[7] + offset, _vertices[8] + offset, Color);
            CCDrawingPrimitives.DrawLine(_vertices[11] + offset, _vertices[12] + offset, Color);
            CCDrawingPrimitives.DrawLine(_vertices[15] + offset, _vertices[0] + offset, Color);

            CCDrawingPrimitives.End();
        }
    }

    public class DrawPrimitivesRoundRectTest : BaseDrawNodeTest
    {
        private const int kTagRoundRect = 5606;

        public DrawPrimitivesRoundRectTest()
        {
            TouchMode = CCTouchMode.OneByOne;
            TouchEnabled = true;
            RoundRectPrimitive r = new RoundRectPrimitive(new CCSize(180f, 180f), 5);
            r.Color = CCColor3B.Yellow;
            r.Position = CCDirector.SharedDirector.WinSize.Center;
            r.AnchorPoint = CCPoint.AnchorMiddle;
            AddChild(r, 0, kTagRoundRect);
        }
        public override string subtitle()
        {
            return "Rounded Rectangle Test - Drag To Resize";
        }
        public void updateSize(CCPoint touchLocation)
        {
            CCNode l = GetChildByTag(kTagRoundRect);
            CCPoint s = l.Position;
            CCSize newSize = new CCSize(Math.Abs(touchLocation.X - s.X) * 2, Math.Abs(touchLocation.Y - s.Y) * 2);
            l.ContentSize = newSize;
        }

        public override bool TouchBegan(CCTouch touche)
        {
            updateSize(touche.Location);
            return true;
        }

        public override void TouchMoved(CCTouch touche)
        {
            updateSize(touche.Location);
        }

        public override void TouchEnded(CCTouch touche)
        {
            updateSize(touche.Location);
        }
    }
#endregion

    #region RoundRectPrimitive

    public class RoundRectSprite : CCSprite
    {
        // This comes from https://cocos2dxna.codeplex.com/discussions/568573#post1309173
        // This class does not work with the transform.

        private RoundRectPrimitive _Rect;
        private CCTexture2D _Texture;

        public RoundRectSprite(CCSize size, int radius)
        {
            _Rect = new RoundRectPrimitive(size, radius);
        }
        public RoundRectSprite(CCSize size, int radius, int cornerSegments)
        {
            _Rect = new RoundRectPrimitive(size, radius, cornerSegments);
        }

        private CCRenderTexture _Render;

        public override CCSize ContentSize
        {
            set
            {
                base.ContentSize = value;
                _Texture = null;
                if (_Rect != null)
                {
                    _Rect.ContentSize = value;
                }
                int iw = (int)value.Width;
                int ih = (int)value.Height;
                if (iw > 0f && ih > 0f)
                {
                    _Rect.RemoveFromParent();
                    _Render = new CCRenderTexture((int)value.Width, (int)value.Height, SurfaceFormat.Color, DepthFormat.None, RenderTargetUsage.PreserveContents);
                    _Render.AddChild(_Rect, 0);
                }
            }
        }

        public override CCPoint Position
        {
            set
            {
                base.Position = value;
                _Texture = null;
            }
        }
        public override void Visit()
        {
            if((_Texture == null || _Texture.IsDisposed || _Texture.XNATexture == null) && _Rect != null && _Render != null) {
                CCSize s = ContentSize;
                if (s.Width >= 1f && s.Height >= 1f)
                {
                    _Render.Clear(0, 0, 0, 255);
                    _Render.Begin();
                    _Rect.Visit();
                    _Render.End();
                    _Texture = _Render.Sprite.Texture;
                    InitWithTexture(_Texture);
                }
            }
            base.Visit();
        }
    }

    public class DrawPrimitivesRoundRectSpriteTest : BaseDrawNodeTest
    {
        private const int kTagRoundRect = 5606;

        public DrawPrimitivesRoundRectSpriteTest()
        {
            TouchMode = CCTouchMode.OneByOne;
            TouchEnabled = true;
            RoundRectSprite r = new RoundRectSprite(new CCSize(180f, 180f), 5);
            r.Color = CCColor3B.Yellow;
            r.Position = CCDirector.SharedDirector.WinSize.Center;
            r.AnchorPoint = CCPoint.AnchorMiddle;
            AddChild(r, 0, kTagRoundRect);
        }
        public override string subtitle()
        {
            return "Rounded Rect Sprite Test - Drag To Resize";
        }
        public void updateSize(CCPoint touchLocation)
        {
            CCNode l = GetChildByTag(kTagRoundRect);
            CCPoint s = l.Position;
            CCSize newSize = new CCSize(Math.Abs(touchLocation.X - s.X) * 2, Math.Abs(touchLocation.Y - s.Y) * 2);
            newSize = newSize.Clamp(CCDirector.SharedDirector.WinSize);
            l.ContentSize = newSize;
        }

        public override bool TouchBegan(CCTouch touche)
        {
            updateSize(touche.Location);
            return true;
        }

        public override void TouchMoved(CCTouch touche)
        {
            updateSize(touche.Location);
        }

        public override void TouchEnded(CCTouch touche)
        {
            updateSize(touche.Location);
        }
    }
    #endregion
}
