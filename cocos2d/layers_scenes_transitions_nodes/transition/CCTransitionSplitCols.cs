/****************************************************************************
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2008-2010 Ricardo Quesada
Copyright (c) 2011 Zynga Inc.
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

namespace cocos2d
{
    /// <summary>
    /// @brief CCTransitionSplitCols:
    /// The odd columns goes upwards while the even columns goes downwards.
    /// </summary>
    public class CCTransitionSplitCols : CCTransitionScene, ICCTransitionEaseScene
    {
        #region ICCTransitionEaseScene Members

        public virtual CCFiniteTimeAction EaseAction(CCActionInterval action)
        {
            return CCEaseInOut.Create(action, 3.0f);
        }

        #endregion

        public virtual CCActionInterval Action()
        {
            return CCSplitCols.Create(3, m_fDuration / 2.0f);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_pInScene.Visible = false;

            CCActionInterval split = Action();
            CCActionInterval seq = CCSequence.FromActions
                (
                    split,
                    CCCallFunc.Create((HideOutShowIn)),
                    split.Reverse()
                );

            RunAction(CCSequence.FromActions(
                EaseAction(seq),
                CCCallFunc.Create(Finish),
                CCStopGrid.Create()));
        }

        public new static CCTransitionSplitCols Create(float t, CCScene scene)
        {
            var pScene = new CCTransitionSplitCols();
            if (pScene.InitWithDuration(t, scene))
            {
                return pScene;
            }

            return null;
        }
    }
}