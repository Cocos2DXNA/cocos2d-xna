using System;
using System.Collections.Generic;

namespace Cocos2D
{
    public class CCMenuItemToggle : CCMenuItem
    {
        public List<CCMenuItem> m_pSubItems;
        private int m_uSelectedIndex=-1;

        public CCMenuItemToggle()
        {
            InitWithTarget(null);
        }

        public CCMenuItemToggle(Action<CCMenuItem> selector, params CCMenuItem[] items)
        {
            InitWithTarget(selector, items);
        }

        public int SelectedIndex
        {
            get { return m_uSelectedIndex; }
            set
            {
                if (value != m_uSelectedIndex && m_pSubItems.Count > 0)
                {
                    m_uSelectedIndex = value;
                    var currentItem = (CCMenuItem) GetChildByTag(kCurrentItem);
                    if (currentItem != null)
                    {
                        currentItem.Visible = false;
                        currentItem.Tag = CCNode.kCCNodeTagInvalid;
                    }
                    CCMenuItem item = m_pSubItems[m_uSelectedIndex];
                    item.Visible = true;
                    item.Tag = kCurrentItem;
                }
            }
        }

        public List<CCMenuItem> SubItems
        {
            get { return m_pSubItems; }
            set { m_pSubItems = value; }
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                foreach (CCMenuItem item in m_pSubItems)
                {
                    item.Enabled = value;
                }
            }
        }

        public bool InitWithTarget(Action<CCMenuItem> selector, CCMenuItem[] items)
        {
            base.InitWithTarget(selector);
            CascadeColorEnabled = true;
            CascadeOpacityEnabled = true;
            m_pSubItems = new List<CCMenuItem>();
            float w = float.MinValue;
            float h = float.MinValue;
            foreach (CCMenuItem item in items)
            {
                m_pSubItems.Add(item);
                AddChild(item, 0);
                item.Visible = false;
                if (w < item.ContentSize.Width)
                {
                    w = item.ContentSize.Width;
                }
                if (h < item.ContentSize.Height)
                {
                    h = item.ContentSize.Height;
                }
                item.AnchorPoint = CCPoint.AnchorMiddle;
            }
            ContentSize = new CCSize(w, h);
            foreach (CCMenuItem item in items)
            {
                item.Position = ContentSize.Center;
            }
            m_uSelectedIndex = int.MaxValue;
            SelectedIndex = 0;
            return true;
        }

        public static CCMenuItemToggle Create(CCMenuItem item)
        {
            var pRet = new CCMenuItemToggle();
            pRet.InitWithItem(item);
            return pRet;
        }

        public bool InitWithItem(CCMenuItem item)
        {
            base.InitWithTarget(null);
            m_pSubItems = new List<CCMenuItem>();
            m_pSubItems.Add(item);
            m_uSelectedIndex = int.MaxValue;
            ContentSize = item.ContentSize;
            AddChild(item, 0);
            item.Visible = true;
            SelectedIndex = 0;

            CascadeColorEnabled = true;
            CascadeOpacityEnabled = true;

            return true;
        }

        public void AddSubItem(CCMenuItem item)
        {
            m_pSubItems.Add(item);
        }

        public CCMenuItem SelectedItem
        {
            get { return m_pSubItems[m_uSelectedIndex]; }
        }

        public override void Activate()
        {
            // update index
            if (m_bIsEnabled)
            {
                int newIndex = (m_uSelectedIndex + 1) % m_pSubItems.Count;
                SelectedIndex = newIndex;
            }
            base.Activate();
        }

        /// <summary>
        /// Set this to true if you want to zoom-in/out on the button image like the CCMenuItemLabel works.
        /// </summary>
        public bool ZoomBehaviorOnTouch { get; set; }
        private float m_fOriginalScale = 0f;

        public override void Selected()
        {
            base.Selected();
            m_pSubItems[m_uSelectedIndex].Selected();
            if (ZoomBehaviorOnTouch)
            {
                CCAction action = m_pSubItems[m_uSelectedIndex].GetActionByTag(unchecked((int)kZoomActionTag));
                if (action != null)
                {
                    StopAction(action);
                }
                else
                {
                    m_fOriginalScale = Scale;
                }

                CCAction zoomAction = new CCScaleTo(0.1f, m_fOriginalScale * 1.2f);
                zoomAction.Tag = unchecked((int)kZoomActionTag);
                m_pSubItems[m_uSelectedIndex].RunAction(zoomAction);
            }
        }

        public override void Unselected()
        {
            base.Unselected();
            m_pSubItems[m_uSelectedIndex].Unselected();
            if (ZoomBehaviorOnTouch)
            {
                m_pSubItems[m_uSelectedIndex].StopActionByTag(unchecked((int)kZoomActionTag));
                CCAction zoomAction = new CCScaleTo(0.1f, m_fOriginalScale);
                zoomAction.Tag = unchecked((int)kZoomActionTag);
                m_pSubItems[m_uSelectedIndex].RunAction(zoomAction);
            }
        }
    }
}