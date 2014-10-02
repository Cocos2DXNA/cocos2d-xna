using System;

namespace Cocos2D
{
    public class CCMenuItemFont : CCMenuItemLabel
    {
        protected string m_strFontName;
        protected uint m_uFontSize;

        public CCMenuItemFont (string value) : this(value, null)
        { }
        
		public CCMenuItemFont (string value, Action<CCMenuItem> selector)
        {
            InitWithString(value, selector);
        }
        
        /// <summary>
        /// Sets the font size for all items.
        /// </summary>
        public static uint FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        /// <summary>
        /// Sets the font name for all items.
        /// </summary>
        public static string FontName
        {
            get { return _fontName; }
            set { _fontName = value; }
        }

        /// <summary>
        /// Sets the font size for this menu item
        /// </summary>
        public uint ItemFontSize
        {
            set
            {
                m_uFontSize = value;
                RecreateLabel();
            }
            get { return m_uFontSize; }
        }

        /// <summary>
        /// Sets the name of the font for this item.
        /// </summary>
        public string ItemFontName
        {
            set
            {
                m_strFontName = value;
                RecreateLabel();
            }
            get { return m_strFontName; }
        }

        public override CCColor3B Color
        {
            set
            {
                base.Color = value;
                RecreateLabel();
            }
        }

        [Obsolete("Use ItemFontSize")]
        public uint FontSizeObj
        {
            set
            {
                m_uFontSize = value;
                RecreateLabel();
            }
            get { return m_uFontSize; }
        }

        [Obsolete("Use ItemFontName")]
        public string FontNameObj
        {
            set
            {
                m_strFontName = value;
                RecreateLabel();
            }
            get { return m_strFontName; }
        }

		protected virtual bool InitWithString(string value, Action<CCMenuItem> selector)
        {
            //CCAssert( value != NULL && strlen(value) != 0, "Value length must be greater than 0");

            m_strFontName = _fontName;
            m_uFontSize = _fontSize;

            CCLabelTTF label = new CCLabelTTF(value, m_strFontName, m_uFontSize);
            base.InitWithLabel(label, selector);
            return true;
        }

        protected virtual void RecreateLabel()
        {
            CCLabelTTF label = new CCLabelTTF((m_pLabel as ICCLabelProtocol).Text, m_strFontName, m_uFontSize);
            label.Color = Color;
            Label = label;
        }
    }
}