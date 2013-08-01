/*
 * CCControlSwitch.h
 *
 * Copyright 2012 Yannick Loriot. All rights reserved.
 * http://yannickloriot.com
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */


using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Cocos2D
{
    public delegate void CCSwitchValueChangedDelegate(object sender, bool bState);

    /** @class CCControlSwitch Switch control for Cocos2D. */

    public class CCControlSwitch : CCControl
    {
        /** Initializes a switch with a mask sprite, on/off sprites for on/off states and a thumb sprite. */

        protected bool _moved;
        /** A Boolean value that determines the off/on state of the switch. */
        protected bool _on;
        protected float _initialTouchXPosition;
        protected CCControlSwitchSprite _switchSprite;

        public event CCSwitchValueChangedDelegate OnValueChanged;

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                _enabled = value;
                if (_switchSprite != null)
                {
                    _switchSprite.Opacity = (byte) (value ? 255 : 128);
                }
            }
        }

        protected virtual bool InitWithMaskSprite(CCSprite maskSprite, CCSprite onSprite, CCSprite offSprite, CCSprite thumbSprite)
        {
            return InitWithMaskSprite(maskSprite, onSprite, offSprite, thumbSprite, null, null);
        }

        /** Creates a switch with a mask sprite, on/off sprites for on/off states and a thumb sprite. */

        public CCControlSwitch(CCSprite maskSprite, CCSprite onSprite, CCSprite offSprite, CCSprite thumbSprite)
        {
            InitWithMaskSprite(maskSprite, onSprite, offSprite, thumbSprite, null, null);
        }

        /** Initializes a switch with a mask sprite, on/off sprites for on/off states, a thumb sprite and an on/off labels. */

        protected virtual bool InitWithMaskSprite(CCSprite maskSprite, CCSprite onSprite, CCSprite offSprite, CCSprite thumbSprite, CCLabelTTF onLabel,
                                       CCLabelTTF offLabel)
        {
            if (base.Init())
            {
                Debug.Assert(maskSprite != null, "Mask must not be nil.");
                Debug.Assert(onSprite != null, "onSprite must not be nil.");
                Debug.Assert(offSprite != null, "offSprite must not be nil.");
                Debug.Assert(thumbSprite != null, "thumbSprite must not be nil.");

                TouchEnabled = true;
                _on = true;

                _switchSprite = new CCControlSwitchSprite();
                _switchSprite.InitWithMaskSprite(maskSprite, onSprite, offSprite, thumbSprite,
                                                   onLabel, offLabel);
                _switchSprite.Position = new CCPoint(_switchSprite.ContentSize.Width / 2, _switchSprite.ContentSize.Height / 2);
                AddChild(_switchSprite);

                IgnoreAnchorPointForPosition = false;
                AnchorPoint = new CCPoint(0.5f, 0.5f);
                ContentSize = _switchSprite.ContentSize;
                return true;
            }
            return false;
        }

        /** Creates a switch with a mask sprite, on/off sprites for on/off states, a thumb sprite and an on/off labels. */

        public CCControlSwitch(CCSprite maskSprite, CCSprite onSprite, CCSprite offSprite, CCSprite thumbSprite, CCLabelTTF onLabel,
                                             CCLabelTTF offLabel)
        {
            InitWithMaskSprite(maskSprite, onSprite, offSprite, thumbSprite, onLabel, offLabel);
        }

        /**
		 * Set the state of the switch to On or Off, optionally animating the transition.
		 *
		 * @param isOn YES if the switch should be turned to the On position; NO if it 
		 * should be turned to the Off position. If the switch is already in the 
		 * designated position, nothing happens.
		 * @param animated YES to animate the "flipping" of the switch; otherwise NO.
		 */

        public void SetOn(bool isOn)
        {
            SetOn(isOn, false);
        }

        public void SetOn(bool isOn, bool animated)
        {
            bool bNotify = false;
            if (_on != isOn)
            {
                _on = isOn;
                bNotify = true;
            }

            _switchSprite.RunAction(
                new CCActionTween (
                    0.2f,
                    "sliderXPosition",
                    _switchSprite.SliderXPosition,
                    (_on) ? _switchSprite.OnPosition : _switchSprite.OffPosition
                    )
                );

            if (bNotify)
            {
                SendActionsForControlEvents(CCControlEvent.ValueChanged);
                if (OnValueChanged != null)
                {
                    OnValueChanged(this, _on);
                }
            }
        }

        public bool IsOn()
        {
            return _on;
        }

        public bool HasMoved()
        {
            return _moved;
        }


        public CCPoint LocationFromTouch(CCTouch touch)
        {
            CCPoint touchLocation = touch.Location; // Get the touch position
            touchLocation = ConvertToNodeSpace(touchLocation); // Convert to the node space of this class

            return touchLocation;
        }

        //events
        public override bool TouchBegan(CCTouch pTouch)
        {
            if (!IsTouchInside(pTouch) || !Enabled)
            {
                return false;
            }

            _moved = false;

            CCPoint location = LocationFromTouch(pTouch);

            _initialTouchXPosition = location.X - _switchSprite.SliderXPosition;

            _switchSprite.ThumbSprite.Color = new CCColor3B(166, 166, 166);
            _switchSprite.NeedsLayout();

            return true;
        }

        public override void TouchMoved(CCTouch pTouch)
        {
            CCPoint location = LocationFromTouch(pTouch);
            location = new CCPoint(location.X - _initialTouchXPosition, 0);

            _moved = true;

            _switchSprite.SliderXPosition = location.X;
        }

        public override void TouchEnded(CCTouch pTouch)
        {
            CCPoint location = LocationFromTouch(pTouch);

            _switchSprite.ThumbSprite.Color = new CCColor3B(255, 255, 255);

            if (HasMoved())
            {
                SetOn(!(location.X < _switchSprite.ContentSize.Width / 2), true);
            }
            else
            {
                SetOn(!_on, true);
            }
        }

        public override void TouchCancelled(CCTouch pTouch)
        {
            CCPoint location = LocationFromTouch(pTouch);

            _switchSprite.ThumbSprite.Color = new CCColor3B(255, 255, 255);

            if (HasMoved())
            {
                SetOn(!(location.X < _switchSprite.ContentSize.Width / 2), true);
            }
            else
            {
                SetOn(!_on, true);
            }
        }

        /** Sprite which represents the view. */
    }

    public class CCControlSwitchSprite : CCSprite, ICCActionTweenDelegate
    {
        private CCSprite _thumbSprite;
        private float _offPosition;
        private float _onPosition;
        private float _sliderXPosition;
        private CCSprite _maskSprite;
        private CCTexture2D _maskTexture;
        private CCLabelTTF _offLabel;
        private CCSprite _offSprite;
        private CCLabelTTF _onLabel;
        private CCSprite _onSprite;

        public CCControlSwitchSprite()
        {
            _sliderXPosition = 0.0f;
            _onPosition = 0.0f;
            _offPosition = 0.0f;
            _maskTexture = null;
            TextureLocation = 0;
            MaskLocation = 0;
            _onSprite = null;
            _offSprite = null;
            _thumbSprite = null;
            _onLabel = null;
            _offLabel = null;
        }

        public float OnPosition
        {
            get { return _onPosition; }
            set { _onPosition = value; }
        }

        public float OffPosition
        {
            get { return _offPosition; }
            set { _offPosition = value; }
        }

        public CCTexture2D MaskTexture
        {
            get { return _maskTexture; }
            set { _maskTexture = value; }
        }

        public uint TextureLocation { get; set; }

        public uint MaskLocation { get; set; }

        public CCSprite OnSprite
        {
            get { return _onSprite; }
            set { _onSprite = value; }
        }

        public CCSprite OffSprite
        {
            get { return _offSprite; }
            set { _offSprite = value; }
        }

        public CCSprite ThumbSprite
        {
            get { return _thumbSprite; }
            set { _thumbSprite = value; }
        }


        public CCLabelTTF OnLabel
        {
            get { return _onLabel; }
            set { _onLabel = value; }
        }

        public CCLabelTTF OffLabel
        {
            get { return _offLabel; }
            set { _offLabel = value; }
        }

        public float SliderXPosition
        {
            get { return _sliderXPosition; }
            set
            {
                if (value <= _offPosition)
                {
                    // Off
                    value = _offPosition;
                }
                else if (value >= _onPosition)
                {
                    // On
                    value = _onPosition;
                }

                _sliderXPosition = value;

                NeedsLayout();
            }
        }

        public float OnSideWidth
        {
            get { return _onSprite.ContentSize.Width; }
        }

        public float OffSideWidth
        {
            get { return _offSprite.ContentSize.Height; }
        }

        #region CCActionTweenDelegate Members

        public virtual void UpdateTweenAction(float value, string key)
        {
            //CCLog.Log("key = {0}, value = {1}", key, value);
            SliderXPosition = value;
        }

        #endregion

        public bool InitWithMaskSprite(CCSprite maskSprite, CCSprite onSprite, CCSprite offSprite,
                                       CCSprite thumbSprite, CCLabelTTF onLabel, CCLabelTTF offLabel)
        {
            if (base.InitWithTexture(maskSprite.Texture))
            {
                // Sets the default values
                _onPosition = 0;
                _offPosition = -onSprite.ContentSize.Width + thumbSprite.ContentSize.Width / 2;
                _sliderXPosition = _onPosition;

                OnSprite = onSprite;
                OffSprite = offSprite;
                ThumbSprite = thumbSprite;
                OnLabel = onLabel;
                OffLabel = offLabel;

                AddChild(_thumbSprite);

                // Set up the mask with the Mask shader
                MaskTexture = maskSprite.Texture;

                /*
				CCGLProgram* pProgram = new CCGLProgram();
				pProgram->initWithVertexShaderByteArray(ccPositionTextureColor_vert, ccExSwitchMask_frag);
				setShaderProgram(pProgram);
				pProgram->release();

				CHECK_GL_ERROR_DEBUG();

				getShaderProgram()->addAttribute(kCCAttributeNamePosition, kCCVertexAttrib_Position);
				getShaderProgram()->addAttribute(kCCAttributeNameColor, kCCVertexAttrib_Color);
				getShaderProgram()->addAttribute(kCCAttributeNameTexCoord, kCCVertexAttrib_TexCoords);
				CHECK_GL_ERROR_DEBUG();

				getShaderProgram()->link();
				CHECK_GL_ERROR_DEBUG();

				getShaderProgram()->updateUniforms();
				CHECK_GL_ERROR_DEBUG();                

				m_uTextureLocation    = glGetUniformLocation( getShaderProgram()->getProgram(), "u_texture");
				m_uMaskLocation       = glGetUniformLocation( getShaderProgram()->getProgram(), "u_mask");
				CHECK_GL_ERROR_DEBUG();
				*/
                ContentSize = _maskTexture.ContentSize;

                NeedsLayout();
                return true;
            }
            return false;
        }

        public override void Draw()
        {
            CCDrawManager.BlendFunc(CCBlendFunc.AlphaBlend);
            CCDrawManager.BindTexture(Texture);
            CCDrawManager.DrawQuad(ref m_sQuad);

            //    /*
            //    CC_NODE_DRAW_SETUP();

            //    ccGLEnableVertexAttribs(kCCVertexAttribFlag_PosColorTex);
            //    ccGLBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            //    getShaderProgram()->setUniformForModelViewProjectionMatrix();

            //    glActiveTexture(GL_TEXTURE0);
            //    glBindTexture( GL_TEXTURE_2D, getTexture()->getName());
            //    glUniform1i(m_uTextureLocation, 0);

            //    glActiveTexture(GL_TEXTURE1);
            //    glBindTexture( GL_TEXTURE_2D, m_pMaskTexture->getName() );
            //    glUniform1i(m_uMaskLocation, 1);

            //#define kQuadSize sizeof(m_sQuad.bl)
            //    long offset = (long)&m_sQuad;

            //    // vertex
            //    int diff = offsetof( ccV3F_C4B_T2F, vertices);
            //    glVertexAttribPointer(kCCVertexAttrib_Position, 3, GL_FLOAT, GL_FALSE, kQuadSize, (void*) (offset + diff));

            //    // texCoods
            //    diff = offsetof( ccV3F_C4B_T2F, texCoords);
            //    glVertexAttribPointer(kCCVertexAttrib_TexCoords, 2, GL_FLOAT, GL_FALSE, kQuadSize, (void*)(offset + diff));

            //    // color
            //    diff = offsetof( ccV3F_C4B_T2F, colors);
            //    glVertexAttribPointer(kCCVertexAttrib_Color, 4, GL_UNSIGNED_BYTE, GL_TRUE, kQuadSize, (void*)(offset + diff));

            //    glDrawArrays(GL_TRIANGLE_STRIP, 0, 4);    
            //    glActiveTexture(GL_TEXTURE0);
            //    */
        }


        public void NeedsLayout()
        {
            _onSprite.Position = new CCPoint(_onSprite.ContentSize.Width / 2 + _sliderXPosition,
                                               _onSprite.ContentSize.Height / 2);
            _offSprite.Position = new CCPoint(_onSprite.ContentSize.Width + _offSprite.ContentSize.Width / 2 + _sliderXPosition,
                                                _offSprite.ContentSize.Height / 2);
            _thumbSprite.Position = new CCPoint(_onSprite.ContentSize.Width + _sliderXPosition,
                                                 _maskTexture.ContentSize.Height / 2);

            if (_onLabel != null)
            {
                _onLabel.Position = new CCPoint(_onSprite.Position.X - _thumbSprite.ContentSize.Width / 6,
                                                  _onSprite.ContentSize.Height / 2);
            }
            if (_offLabel != null)
            {
                _offLabel.Position = new CCPoint(_offSprite.Position.X + _thumbSprite.ContentSize.Width / 6,
                                                   _offSprite.ContentSize.Height / 2);
            }

            CCRenderTexture rt = new CCRenderTexture((int) _maskTexture.ContentSize.Width, (int) _maskTexture.ContentSize.Height,
                                                        SurfaceFormat.Color, DepthFormat.None, RenderTargetUsage.DiscardContents);

            rt.BeginWithClear(0, 0, 0, 0);

            _onSprite.Visit();
            _offSprite.Visit();

            if (_onLabel != null)
            {
                _onLabel.Visit();
            }
            if (_offLabel != null)
            {
                _offLabel.Visit();
            }

            if (_maskSprite == null)
            {
                _maskSprite = new CCSprite(_maskTexture);
                _maskSprite.AnchorPoint = new CCPoint(0, 0);
                _maskSprite.BlendFunc = new CCBlendFunc(CCOGLES.GL_ZERO, CCOGLES.GL_SRC_ALPHA);
            }
            else
            {
                _maskSprite.Texture = _maskTexture;
            }

            _maskSprite.Visit();

            rt.End();

            Texture = rt.Sprite.Texture;
            //IsFlipY = true;
        }
    }
}