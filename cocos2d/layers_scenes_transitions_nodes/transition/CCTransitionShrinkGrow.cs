/****************************************************************************
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2008-2010 Ricardo Quesada
Copyright (c) 2011 Zynga Inc.

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

namespace cocos2d
{
    public class CCTransitionShrinkGrow : CCTransitionScene, ICCTransitionEaseScene
    {

        public CCTransitionShrinkGrow (float t, CCScene scene) : base (t, scene)
        { }

        #region ICCTransitionEaseScene Members

        public CCFiniteTimeAction EaseAction(CCActionInterval action)
        {
            return new CCEaseOut(action, 2.0f);
        }

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();

            m_pInScene.Scale = 0.001f;
            m_pOutScene.Scale = (1.0f);

            m_pInScene.AnchorPoint = new CCPoint(2 / 3.0f, 0.5f);
            m_pOutScene.AnchorPoint = new CCPoint(1 / 3.0f, 0.5f);

            CCActionInterval scaleOut = new CCScaleTo(m_fDuration, 0.01f);
            CCActionInterval scaleIn = new CCScaleTo(m_fDuration, 1.0f);

            m_pInScene.RunAction(EaseAction(scaleIn));
            m_pOutScene.RunAction
                (
                    CCSequence.FromActions
                        (
                            EaseAction(scaleOut),
                            new CCCallFunc((Finish))
                        )
                );
        }

    }
}