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

using System;

namespace cocos2d
{
    /// <summary>
    /// @brief CCFadeOutTRTiles action
    /// Fades out the tiles in a Top-Right direction
    /// </summary>
    public class CCFadeOutTRTiles : CCTiledGrid3DAction
    {
        public virtual float TestFunc(CCGridSize pos, float time)
        {
            var n = new CCPoint((m_sGridSize.X * time), (m_sGridSize.Y * time));
            if ((n.X + n.Y) == 0.0f)
            {
                return 1.0f;
            }

            return (float) Math.Pow((pos.X + pos.Y) / (n.X + n.Y), 6);
        }

        public void TurnOnTile(CCGridSize pos)
        {
            CCQuad3 orig = OriginalTile(pos);
            SetTile(pos, ref orig);
        }

        public void TurnOffTile(CCGridSize pos)
        {
            var coords = new CCQuad3();
            //memset(&coords, 0, sizeof(ccQuad3));
            SetTile(pos, ref coords);
        }

        public virtual void TransformTile(CCGridSize pos, float distance)
        {
            CCQuad3 coords = OriginalTile(pos);
            CCPoint step = m_pTarget.Grid.Step;

            coords.BottomLeft.X += (step.X / 2) * (1.0f - distance);
            coords.BottomLeft.Y += (step.Y / 2) * (1.0f - distance);

            coords.BottomRight.X -= (step.X / 2) * (1.0f - distance);
            coords.BottomRight.Y += (step.Y / 2) * (1.0f - distance);

            coords.TopLeft.X += (step.X / 2) * (1.0f - distance);
            coords.TopLeft.Y -= (step.Y / 2) * (1.0f - distance);

            coords.TopRight.X -= (step.X / 2) * (1.0f - distance);
            coords.TopRight.Y -= (step.Y / 2) * (1.0f - distance);

            SetTile(pos, ref coords);
        }

        public override void Update(float time)
        {
            int i, j;

            for (i = 0; i < m_sGridSize.X; ++i)
            {
                for (j = 0; j < m_sGridSize.Y; ++j)
                {
                    float distance = TestFunc(new CCGridSize(i, j), time);
                    if (distance == 0)
                    {
                        TurnOffTile(new CCGridSize(i, j));
                    }
                    else if (distance < 1)
                    {
                        TransformTile(new CCGridSize(i, j), distance);
                    }
                    else
                    {
                        TurnOnTile(new CCGridSize(i, j));
                    }
                }
            }
        }

        /// <summary>
        /// creates the action with the grid size and the duration
        /// </summary>
        public new static CCFadeOutTRTiles Create(CCGridSize gridSize, float time)
        {
            var pAction = new CCFadeOutTRTiles();
            pAction.InitWithSize(gridSize, time);
            return pAction;
        }
    }
}