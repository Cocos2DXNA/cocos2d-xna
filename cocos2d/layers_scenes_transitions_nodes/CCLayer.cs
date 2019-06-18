/****************************************************************************
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2008-2010 Ricardo Quesada
Copyright (c) 2011      Zynga Inc.
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
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cocos2D
{
    public class CCLayer : CCNode, ICCAccelerometerDelegate
    {
        private bool m_bIsAccelerometerEnabled;

        private CCRenderTexture m_pRenderTexture;
        private bool m_bRestoreScissor;
        private CCRect m_tSaveScissorRect;
        private bool m_bNoDrawChildren;

        /// <summary>
        /// Set to true if the child drawing should be isolated in their own render target
        /// </summary>
        protected CCClipMode m_childClippingMode = CCClipMode.None;

        public CCLayer(CCClipMode clipMode)
        {
            m_childClippingMode = clipMode;
            AnchorPoint = new CCPoint(0.5f, 0.5f);
            m_bIgnoreAnchorPointForPosition = true;
            CCDirector director = CCDirector.SharedDirector;
            if (director != null)
            {
                ContentSize = director.WinSize;
            }
            Init();
        }

        /// <summary>
        /// Default layer constructor that does not use clipping.
        /// </summary>
        public CCLayer() : this(CCClipMode.None)
        {
        }


        public CCClipMode ChildClippingMode
        {
            get { return m_childClippingMode; }
            set
            {
                if (m_childClippingMode != value)
                {
                    m_childClippingMode = value;
                    InitClipping();
                }
            }
        }

        public override CCSize ContentSize
        {
            get { return base.ContentSize; }
            set
            {
                base.ContentSize = value;
                InitClipping();
            }
        }

        private bool m_bDidInit = false;

        public override void Visit()
        {
            // quick return if not visible
            if (!Visible)
            {
                return;
            }
            if (m_childClippingMode == CCClipMode.None)
            {
                base.Visit();
                return;
            }

            UpdateGraphIndex();
            CCDrawManager.PushMatrix();

            if (m_pGrid != null && m_pGrid.Active)
            {
                m_pGrid.BeforeDraw();
                TransformAncestors();
            }

            Transform();

            BeforeDraw();

            if (!m_bNoDrawChildren && m_pChildren != null)
            {
                SortAllChildren();

                CCNode[] arrayData = m_pChildren.Elements;
                int count = m_pChildren.count;
                int i = 0;

                // draw children zOrder < 0
                for (; i < count; i++)
                {
                    CCNode child = arrayData[i];
                    if (child.m_nZOrder < 0)
                    {
                        child.Visit();
                    }
                    else
                    {
                        break;
                    }
                }

                // this draw
                Draw();

                // draw children zOrder >= 0
                for (; i < count; i++)
                {
                    arrayData[i].Visit();
                }
            }
            else
            {
                Draw();
            }

            AfterDraw();

            if (m_pGrid != null && m_pGrid.Active)
            {
                m_pGrid.AfterDraw(this);
            }

            CCDrawManager.PopMatrix();
        }

        private void InitClipping()
        {
            if (m_childClippingMode == CCClipMode.BoundsWithRenderTarget)
            {
                if (m_pRenderTexture == null || m_pRenderTexture.ContentSize.Width < ContentSize.Width || m_pRenderTexture.ContentSize.Height < ContentSize.Height)
                {
                    m_pRenderTexture = new CCRenderTexture((int)ContentSize.Width, (int)ContentSize.Height);
                    m_pRenderTexture.Sprite.AnchorPoint = new CCPoint(0, 0);
                }
                m_pRenderTexture.Sprite.TextureRect = new CCRect(0, 0, ContentSize.Width, ContentSize.Height);
            }
            else
            {
                m_pRenderTexture = null;
            }
        }

        private void BeforeDraw()
        {
            m_bNoDrawChildren = false;

            if (m_childClippingMode == CCClipMode.Bounds)
            {
                // We always clip to the bounding box
                var rect = new CCRect(0, 0, m_obContentSize.Width, m_obContentSize.Height);
                var bounds = CCAffineTransform.Transform(rect, NodeToWorldTransform());

                var winSize = CCDirector.SharedDirector.WinSize;

                CCRect prevScissorRect;
                if (CCDrawManager.ScissorRectEnabled)
                {
                    prevScissorRect = CCDrawManager.ScissorRect;
                }
                else
                {
                    prevScissorRect = new CCRect(0, 0, winSize.Width, winSize.Height);
                }

                if (!bounds.IntersectsRect(prevScissorRect))
                {
                    m_bNoDrawChildren = true;
                    return;
                }

                float minX = Math.Max(bounds.MinX, prevScissorRect.MinX);
                float minY = Math.Max(bounds.MinY, prevScissorRect.MinY);
                float maxX = Math.Min(bounds.MaxX, prevScissorRect.MaxX);
                float maxY = Math.Min(bounds.MaxY, prevScissorRect.MaxY);
              
                if (CCDrawManager.ScissorRectEnabled)
                {
                    m_bRestoreScissor = true;
                }
                else
                {
                    CCDrawManager.ScissorRectEnabled = true;
                }

                m_tSaveScissorRect = prevScissorRect;

                CCDrawManager.SetScissorInPoints(minX, minY, maxX - minX, maxY - minY);
            }
            else if (m_childClippingMode == CCClipMode.BoundsWithRenderTarget)
            {
                m_tSaveScissorRect = CCDrawManager.ScissorRect;
                m_bRestoreScissor = CCDrawManager.ScissorRectEnabled;

                CCDrawManager.ScissorRectEnabled = false;

                CCDrawManager.PushMatrix();
                CCDrawManager.SetIdentityMatrix();

                m_pRenderTexture.BeginWithClear(0, 0, 0, 0);
            }
        }

        /**
     * retract what's done in beforeDraw so that there's no side effect to
     * other nodes.
     */
        private void AfterDraw()
        {
            if (m_childClippingMode != CCClipMode.None)
            {
                if (m_childClippingMode == CCClipMode.BoundsWithRenderTarget)
                {
                    m_pRenderTexture.End();

                    CCDrawManager.PopMatrix();
                }

                if (m_bRestoreScissor)
                {
                    CCDrawManager.SetScissorInPoints(
                        m_tSaveScissorRect.Origin.X, m_tSaveScissorRect.Origin.Y,
                        m_tSaveScissorRect.Size.Width, m_tSaveScissorRect.Size.Height);

                    CCDrawManager.ScissorRectEnabled = true;

                    m_bRestoreScissor = false;
                }
                else
                {
                    CCDrawManager.ScissorRectEnabled = false;
                }

                if (m_childClippingMode == CCClipMode.BoundsWithRenderTarget)
                {
                    m_pRenderTexture.Sprite.Visit();
                }
            }
        }

        public override bool Init()
        {
            if (m_bDidInit)
            {
                return (true);
            }

            TouchMode = CCTouchMode.AllAtOnce;

            bool bRet = false;
            CCDirector director = CCDirector.SharedDirector;
            if (director != null)
            {
                //                ContentSize = director.WinSize;
                m_bIsAccelerometerEnabled = false;
                bRet = true;
                m_bDidInit = true;
            }
            return bRet;
        }

        public override void OnEnter()
        {
            if(!m_bDidInit) {
                Init();
            }

            // then iterate over all the children
            base.OnEnter();

            CCDirector director = CCDirector.SharedDirector;
            CCApplication application = CCApplication.SharedApplication;

            // add this layer to concern the Accelerometer Sensor
            if (m_bIsAccelerometerEnabled)
            {
#if !PSM &&!NETFX_CORE
                director.Accelerometer.SetDelegate(this);
#endif
			}
        }

        public override void OnExit()
        {

            // remove this layer from the delegates who concern Accelerometer Sensor
            if (m_bIsAccelerometerEnabled)
            {
                //CCDirector director = CCDirector.SharedDirector;
                //director.Accelerometer.setDelegate(null);
            }

            base.OnExit();
        }

        public override void OnEnterTransitionDidFinish()
        {
            //if (m_bIsAccelerometerEnabled)
            //{
            //    CCDirector.SharedDirector.Accelerometer.SetDelegate(this);
            //}

            base.OnEnterTransitionDidFinish();
        }

        public bool AccelerometerEnabled
        {
            get { 
#if !PSM
				return m_bIsAccelerometerEnabled; 
#else
				return(false);
#endif
			}
            set {
#if !PSM &&!NETFX_CORE
                if (value != m_bIsAccelerometerEnabled)
                {
                    m_bIsAccelerometerEnabled = value;

                    if (m_bRunning)
                    {
                        CCDirector pDirector = CCDirector.SharedDirector;
                        pDirector.Accelerometer.SetDelegate(value ? this : null);
                    }
                }
#else
                m_bIsAccelerometerEnabled = false;
#endif
			}
        }

        public virtual void DidAccelerate(CCAcceleration pAccelerationValue)
        {
        }
    }
}