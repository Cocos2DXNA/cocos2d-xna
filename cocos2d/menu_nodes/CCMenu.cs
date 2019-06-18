using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cocos2D
{
    public enum CCMenuState
    {
        Waiting,
        TrackingTouch,
        Focused
    };

    /// <summary>
    /// A CCMenu
    /// Features and Limitation:
    ///  You can add MenuItem objects in runtime using addChild:
    ///  But the only accecpted children are MenuItem objects
    /// </summary>
    public class CCMenu : CCLayerRGBA
    {
        public const float kDefaultPadding = 5;
        public const int kCCMenuHandlerPriority = -128;
        public const int kMaxGraphPriority = 9;

        protected bool m_bEnabled;
        protected CCMenuState m_eState;
        protected CCMenuItem m_pSelectedItem;

        private LinkedList<CCMenuItem> _Items = new LinkedList<CCMenuItem>();

        /// <summary>
        /// Default ctor that sets the content size of the menu to match the window size.
        /// </summary>
        private CCMenu() 
        {
            Init();
//            ContentSize = CCDirector.SharedDirector.WinSize;
        }
        public CCMenu(params CCMenuItem[] items)
        {
            InitWithItems(items);
        }

        public override bool HasFocus
        {
            set
            {
                base.HasFocus = value;
                // Set the first menu item to have the focus
                if (FocusedItem == null)
                {
                    _Items.First.Value.HasFocus = true;
                }
            }
        }

        /// <summary>
        /// Handles the button press event to track which focused menu item will get the activation
        /// </summary>
        /// <param name="backButton"></param>
        /// <param name="startButton"></param>
        /// <param name="systemButton"></param>
        /// <param name="aButton"></param>
        /// <param name="bButton"></param>
        /// <param name="xButton"></param>
        /// <param name="yButton"></param>
        /// <param name="leftShoulder"></param>
        /// <param name="rightShoulder"></param>
        /// <param name="player"></param>
        protected override void OnGamePadButtonUpdate(CCGamePadButtonStatus backButton, CCGamePadButtonStatus startButton, CCGamePadButtonStatus systemButton, CCGamePadButtonStatus aButton, CCGamePadButtonStatus bButton, CCGamePadButtonStatus xButton, CCGamePadButtonStatus yButton, CCGamePadButtonStatus leftShoulder, CCGamePadButtonStatus rightShoulder, Microsoft.Xna.Framework.PlayerIndex player)
        {
            base.OnGamePadButtonUpdate(backButton, startButton, systemButton, aButton, bButton, xButton, yButton, leftShoulder, rightShoulder, player);
            if (!HasFocus)
            {
                return;
            }
            if (backButton == CCGamePadButtonStatus.Pressed || aButton == CCGamePadButtonStatus.Pressed || bButton == CCGamePadButtonStatus.Pressed ||
                xButton == CCGamePadButtonStatus.Pressed || yButton == CCGamePadButtonStatus.Pressed || leftShoulder == CCGamePadButtonStatus.Pressed ||
                rightShoulder == CCGamePadButtonStatus.Pressed)
            {
                CCMenuItem item = FocusedItem;
                item.Selected();
                m_pSelectedItem = item;
                m_eState = CCMenuState.TrackingTouch;
            }
            else if (backButton == CCGamePadButtonStatus.Released || aButton == CCGamePadButtonStatus.Released || bButton == CCGamePadButtonStatus.Released ||
                xButton == CCGamePadButtonStatus.Released || yButton == CCGamePadButtonStatus.Released || leftShoulder == CCGamePadButtonStatus.Released ||
                rightShoulder == CCGamePadButtonStatus.Released)
            {
                if (m_eState == CCMenuState.TrackingTouch)
                {
                    // Now we are selecting the menu item
                    CCMenuItem item = FocusedItem;
                    if (item != null && m_pSelectedItem == item)
                    {
                        // Activate this item
                        item.Unselected();
                        item.Activate();
                        m_eState = CCMenuState.Waiting;
                        m_pSelectedItem = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the menu item with the focus. Note that this only has a value if the GamePad or Keyboard is enabled. Touch
        /// devices do not have a "focus" concept.
        /// </summary>
        public CCMenuItem FocusedItem
        {
            get
            {
                // Find the item with the focus
                foreach(CCMenuItem item in _Items) 
                {
                    if (item.HasFocus)
                    {
                        return (item);
                    }
                }
                return (null);
            }
        }

        public bool Enabled
        {
            get { return m_bEnabled; }
            set { m_bEnabled = value; }
        }

        public override bool Init()
        {
            return InitWithArray(null);
        }

        public bool InitWithItems(params CCMenuItem[] items)
        {
            return InitWithArray(items);
        }

        /// <summary>
        /// The position of the menu is set to the center of the main screen
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private bool InitWithArray(params CCMenuItem[] items)
        {
            if (_Items.Count > 0)
            {
                List<CCMenuItem> copy = new List<CCMenuItem>(_Items);
                foreach (CCMenuItem i in copy)
                {
                    RemoveChild(i, false);
                }
            }
            if (base.Init())
            {
                if (CCConfiguration.SharedConfiguration.UseGraphPriority)
                {
                    TouchPriority = kMaxGraphPriority;
                }
                else
                {
                    TouchPriority = kCCMenuHandlerPriority;
                }
                TouchMode = CCTouchMode.OneByOne;
                TouchEnabled = true;

                m_bEnabled = true;
                // menu in the center of the screen
                CCSize s = CCDirector.SharedDirector.WinSize;

                IgnoreAnchorPointForPosition = true;
                AnchorPoint = new CCPoint(0.5f, 0.5f);
                ContentSize = s;

                Position = (new CCPoint(s.Width / 2, s.Height / 2));

                if (items != null)
                {
                    int z = 0;
                    foreach (CCMenuItem item in items)
                    {
                        AddChild(item, z);
                        z++;
                    }
                }

                //    [self alignItemsVertically];
                m_pSelectedItem = null;
                m_eState = CCMenuState.Waiting;

                // enable cascade color and opacity on menus
                CascadeColorEnabled = true;
                CascadeOpacityEnabled = true;

                return true;
            }
            return false;
        }

        /*
        * override add:
        */

        public override void AddChild(CCNode child, int zOrder, int tag)
        {
            Debug.Assert(child is CCMenuItem, "Menu only supports MenuItem objects as children");
            base.AddChild(child, zOrder, tag);
            if (_Items.Count == 0)
            {
                _Items.AddFirst(child as CCMenuItem);
            }
            else
            {
                _Items.AddLast(child as CCMenuItem);
            }
        }

        public override void RemoveChild(CCNode child, bool cleanup)
        {
            Debug.Assert(child is CCMenuItem, "Menu only supports MenuItem objects as children");

            if (m_pSelectedItem == child)
            {
                m_pSelectedItem = null;
            }

            base.RemoveChild(child, cleanup);

            if (_Items.Contains(child as CCMenuItem))
            {
                _Items.Remove(child as CCMenuItem);
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            
            foreach (CCMenuItem item in _Items)
            {
                CCFocusManager.Instance.Add(item);
            }
        }

        public override void OnExit()
        {
            if (m_eState == CCMenuState.TrackingTouch)
            {
                if (m_pSelectedItem != null)
                {
                    m_pSelectedItem.Unselected();
                    m_pSelectedItem = null;
                }
                m_eState = CCMenuState.Waiting;
            }
            
            foreach (CCMenuItem item in _Items)
            {
                CCFocusManager.Instance.Remove(item);
            }
            
            base.OnExit();
        }

        #region Menu - Events

        public void SetHandlerPriority(int newPriority)
        {
            CCTouchDispatcher pDispatcher = CCDirector.SharedDirector.TouchDispatcher;
            pDispatcher.SetPriority(newPriority, this);
        }

        public override bool TouchBegan(CCTouch touch)
        {
            if (m_eState != CCMenuState.Waiting || !m_bVisible || !m_bEnabled)
            {
                return false;
            }

            for (CCNode c = m_pParent; c != null; c = c.Parent)
            {
                if (c.Visible == false)
                {
                    return false;
                }
            }

            m_pSelectedItem = ItemForTouch(touch);
            if (m_pSelectedItem != null)
            {
                m_eState = CCMenuState.TrackingTouch;
                m_pSelectedItem.Selected();
                return true;
            }
            return false;
        }

        public override void TouchEnded(CCTouch touch)
        {
            Debug.Assert(m_eState == CCMenuState.TrackingTouch, "[Menu TouchEnded] -- invalid state");
            if (m_pSelectedItem != null)
            {
                m_pSelectedItem.Unselected();
                m_pSelectedItem.Activate();
            }
            m_eState = CCMenuState.Waiting;
        }

        public override void TouchCancelled(CCTouch touch)
        {
            Debug.Assert(m_eState == CCMenuState.TrackingTouch, "[Menu ccTouchCancelled] -- invalid state");
            if (m_pSelectedItem != null)
            {
                m_pSelectedItem.Unselected();
            }
            m_eState = CCMenuState.Waiting;
        }

        public override void TouchMoved(CCTouch touch)
        {
            Debug.Assert(m_eState == CCMenuState.TrackingTouch, "[Menu TouchMoved] -- invalid state");
            CCMenuItem currentItem = ItemForTouch(touch);
            if (currentItem != m_pSelectedItem)
            {
                if (m_pSelectedItem != null)
                {
                    m_pSelectedItem.Unselected();
                }

                m_pSelectedItem = currentItem;

                if (m_pSelectedItem != null)
                {
                    m_pSelectedItem.Selected();
                }
            }
        }

        #endregion

        #region Menu - Alignment

        public void AlignItemsVertically()
        {
            AlignItemsVerticallyWithPadding(kDefaultPadding);
        }

        public void AlignItemsVerticallyWithPadding(float padding)
        {
            float width = 0f;
            float height = 0f; 

            if (m_pChildren != null && m_pChildren.count > 0)
            {
                height = -padding;
                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren[i];
                    if (!pChild.VisibleInParent)
                    {
                        continue;
                    }
                    height += pChild.ContentSizeInPixels.Height  + padding;
                    width = Math.Max(width, pChild.ContentSizeInPixels.Width );
                }

                float y = height / 2.0f;

                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren[i];
                    if (!pChild.VisibleInParent)
                    {
                        continue;
                    }
                    pChild.Position = new CCPoint(0, y - pChild.ContentSizeInPixels.Height  / 2.0f);
                    y -= pChild.ContentSizeInPixels.Height  + padding;
                    width = Math.Max(width, pChild.ContentSizeInPixels.Width );
                }
            }
            ContentSize = new CCSize(width, height);
        }

        public void AlignItemsHorizontally()
        {
            AlignItemsHorizontallyWithPadding(kDefaultPadding);
        }

        public void AlignItemsHorizontallyWithPadding(float padding)
        {
            float height = 0f;
            float width = 0f;
            if (m_pChildren != null && m_pChildren.count > 0)
            {
                width = -padding;
                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren[i];
                    if (pChild.VisibleInParent)
                    {
                        width += pChild.ContentSizeInPixels.Width + padding;
                        height = Math.Max(height, pChild.ContentSizeInPixels.Height);
                    }
                }

                float x = -width / 2.0f;

                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren[i];
                    if (pChild.VisibleInParent)
                    {
                        pChild.Position = new CCPoint(x + pChild.ContentSizeInPixels.Width  / 2.0f, 0);
                        x += pChild.ContentSizeInPixels.Width  + padding;
                        height = Math.Max(height, pChild.ContentSizeInPixels.Height * pChild.ScaleY);
                    }
                }
            }
            ContentSize = new CCSize(width, height);
        }

        public void AlignItemsInColumns(params int[] columns)
        {
            int[] rows = columns;

            int height = -5;
            int row = 0;
            int rowHeight = 0;
            int columnsOccupied = 0;
            int rowColumns;

            if (m_pChildren != null && m_pChildren.count > 0)
            {
                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren.Elements[i];
                    if (!pChild.VisibleInParent)
                    {
                        continue;
                    }
                    Debug.Assert(row < rows.Length, "");

                    rowColumns = rows[row];
                    // can not have zero columns on a row
                    Debug.Assert(rowColumns > 0, "");

                    float tmp = pChild.ContentSizeInPixels.Height;
                    rowHeight = (int) ((rowHeight >= tmp || float.IsNaN(tmp)) ? rowHeight : tmp);

                    ++columnsOccupied;
                    if (columnsOccupied >= rowColumns)
                    {
                            height += rowHeight + (int)kDefaultPadding;

                        columnsOccupied = 0;
                        rowHeight = 0;
                        ++row;
                    }
                }
            }

            // check if too many rows/columns for available menu items
            Debug.Assert(columnsOccupied == 0, "");

            CCSize winSize = ContentSize; // CCDirector.SharedDirector.WinSize;

            row = 0;
            rowHeight = 0;
            rowColumns = 0;
            float w = 0.0f;
            float x = 0.0f;
            float y = (height / 2f);

            if (m_pChildren != null && m_pChildren.count > 0)
            {
                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren.Elements[i];
                    if (!pChild.VisibleInParent)
                    {
                        continue;
                    }
                    if (rowColumns == 0)
                    {
                        rowColumns = rows[row];
                            if (rowColumns == 0)
                            {
                                throw (new ArgumentException("Can not have a zero column size for a row."));
                            }
                            w = (winSize.Width - 2 * kDefaultPadding) / rowColumns; // 1 + rowColumns
                            x = w/2f; // center of column
                    }

                        float tmp = pChild.ContentSizeInPixels.Height*pChild.ScaleY;
                    rowHeight = (int) ((rowHeight >= tmp || float.IsNaN(tmp)) ? rowHeight : tmp);

                        pChild.Position = new CCPoint(kDefaultPadding + x - (winSize.Width - 2*kDefaultPadding) / 2,
                                               y - pChild.ContentSizeInPixels.Height*pChild.ScaleY / 2);

                    x += w;
                    ++columnsOccupied;

                    if (columnsOccupied >= rowColumns)
                    {
                        y -= rowHeight + 5;

                        columnsOccupied = 0;
                        rowColumns = 0;
                        rowHeight = 0;
                        ++row;
                    }
                }
            }
        }


        public void AlignItemsInRows(params int[] rows)
        {
            int[] columns = rows;

            List<int> columnWidths = new List<int>();

            List<int> columnHeights = new List<int>();


            int width = -10;
            int columnHeight = -5;
            int column = 0;
            int columnWidth = 0;
            int rowsOccupied = 0;
            int columnRows;

            if (m_pChildren != null && m_pChildren.count > 0)
            {
                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren.Elements[i];
                    if (!pChild.VisibleInParent)
                    {
                        continue;
                    }

                    // check if too many menu items for the amount of rows/columns
                    Debug.Assert(column < columns.Length, "");

                    columnRows = columns[column];
                    // can't have zero rows on a column
                    Debug.Assert(columnRows > 0, "");

                    // columnWidth = fmaxf(columnWidth, [item contentSize].width);
                    float tmp = pChild.ContentSizeInPixels.Width ;
                    columnWidth = (int)((columnWidth >= tmp || float.IsNaN(tmp)) ? columnWidth : tmp);


                    columnHeight += (int)(pChild.ContentSizeInPixels.Height  + 5);
                    ++rowsOccupied;

                    if (rowsOccupied >= columnRows)
                    {
                        columnWidths.Add(columnWidth);
                        columnHeights.Add(columnHeight);
                        width += columnWidth + 10;

                        rowsOccupied = 0;
                        columnWidth = 0;
                        columnHeight = -5;
                        ++column;
                    }
                }
            }

            // check if too many rows/columns for available menu items.
            Debug.Assert(rowsOccupied == 0, "");

            CCSize winSize = ContentSize; // CCDirector.SharedDirector.WinSize;

            column = 0;
            columnWidth = 0;
            columnRows = 0;
            float x = (-width / 2f);
            float y = 0.0f;

            if (m_pChildren != null && m_pChildren.count > 0)
            {
                for (int i = 0, count = m_pChildren.count; i < count; i++)
                {
                    CCNode pChild = m_pChildren.Elements[i];
                    if (!pChild.VisibleInParent)
                    {
                        continue;
                    }

                    if (columnRows == 0)
                    {
                        columnRows = columns[column];
                        y = columnHeights[column];
                    }

                    // columnWidth = fmaxf(columnWidth, [item contentSize].width);
                    float tmp = pChild.ContentSizeInPixels.Width ;
                    columnWidth = (int)((columnWidth >= tmp || float.IsNaN(tmp)) ? columnWidth : tmp);

                    pChild.Position = new CCPoint(x + columnWidths[column] / 2,
                                                  y - winSize.Height / 2);
                    y -= pChild.ContentSizeInPixels.Height  + 10;
                    ++rowsOccupied;

                    if (rowsOccupied >= columnRows)
                    {
                        x += columnWidth + 5;
                        rowsOccupied = 0;
                        columnRows = 0;
                        columnWidth = 0;
                        ++column;
                    }
                }
            }
        }

        #endregion

        protected virtual CCMenuItem ItemForTouch(CCTouch touch)
        {
            CCPoint touchLocation = touch.Location;

            if (m_pChildren != null && m_pChildren.count > 0)
            {
                for (int i = m_pChildren.count-1; i >= 0; i--)
                {
                    var pChild = m_pChildren.Elements[i] as CCMenuItem;
                    if (pChild != null && pChild.VisibleInParent && pChild.Enabled)
                    {
                        CCPoint local = pChild.ConvertToNodeSpace(touchLocation);
                        CCRect r = pChild.Rectangle;
                        r.Origin = CCPoint.Zero;

                        if (r.ContainsPoint(local))
                        {
                            return pChild;
                        }
                    }
                }
            }

            return null;
        }
    }
}