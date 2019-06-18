
using System;

namespace Cocos2D
{
    /// <summary>
    /// @brief CCMenuItem base class
    /// Subclass CCMenuItem (or any subclass) to create your custom CCMenuItem objects.
    /// </summary>
    public class CCMenuItem : CCNodeRGBA
    {
        public const int kCurrentItem = 32767;
        public const uint kZoomActionTag = 0xc0c05002;

        public event Action<CCMenuItem> OnActivated;
        public event Action<CCMenuItem> OnSelected;
        public event Action<CCMenuItem> OnUnselected;

        protected static uint _fontSize = 32;
        protected static string _fontName = "arial";
        protected static bool _fontNameRelease = false;

        protected bool m_bIsEnabled;
        protected bool m_bIsSelected;

        protected string m_functionName;
        protected Action<CCMenuItem> m_pfnSelector;

        public CCMenuItem()
        {
            m_bIsSelected = false;
            m_bIsEnabled = false;
            m_pfnSelector = null;
        }

        /// <summary>
        /// Creates a CCMenuItem with a target/selector
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public CCMenuItem(Action<CCMenuItem> selector)
        {
            InitWithTarget(selector);
        }

        public virtual bool Enabled
        {
            get { return m_bIsEnabled; }
            set { m_bIsEnabled = value; }
        }

        public virtual bool IsSelected
        {
            get { return m_bIsSelected; }
        }

        /// <summary>
        /// Initializes a CCMenuItem with a target/selector
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public bool InitWithTarget(Action<CCMenuItem> selector)
        {
            AnchorPoint = new CCPoint(0.5f, 0.5f);
            m_pfnSelector = selector;
            m_bIsEnabled = true;
            m_bIsSelected = false;
            return true;
        }

        /// <summary>
        /// Returns the outside box
        /// </summary>
        /// <returns></returns>
        public CCRect Rectangle
        {
			get {
				return new CCRect (m_obPosition.X - m_obContentSize.Width * m_obAnchorPoint.X,
				                          m_obPosition.Y - m_obContentSize.Height * m_obAnchorPoint.Y,
				                          m_obContentSize.Width,
				                          m_obContentSize.Height);
			}
        }

        /// <summary>
        /// Activate the item
        /// </summary>
        public virtual void Activate()
        {
            if (m_bIsEnabled)
            {
                if (m_pfnSelector != null)
                {
                    m_pfnSelector(this);
                }
                if(OnActivated!=null)
                OnActivated(this);

                //if (m_functionName.size() && CCScriptEngineManager.sharedScriptEngineManager().getScriptEngine())
                //{
                //CCScriptEngineManager.sharedScriptEngineManager().getScriptEngine().executeCallFuncN(m_functionName.c_str(), this);
                //}
            }
        }

        /// <summary>
        /// The item was selected (not activated), similar to "mouse-over"
        /// </summary>
        public virtual void Selected()
        {
            m_bIsSelected = true;
            if(OnSelected != null)
            OnSelected(this);
        }

        /// <summary>
        /// The item was unselected
        /// </summary>
        public virtual void Unselected()
        {
            m_bIsSelected = false;
            if(OnUnselected!=null)
            OnUnselected(this);
        }

        /// <summary>
        /// Register a script function, the function is called in activete
        /// If pszFunctionName is NULL, then unregister it.
        /// </summary>
        /// <param name="pszFunctionName"></param>
        public virtual void RegisterScriptHandler(string pszFunctionName)
        {
            throw new NotImplementedException("CCMenuItem.RegisterScriptHandler is not supported in this version of Cocos2d-XNA");
        }

        /// <summary>
        /// set the target/selector of the menu item. You should use the events associated with the menu
        /// item lifecycle.
        /// </summary>
        /// <param name="selector"></param>
        public virtual void SetTarget(Action<CCMenuItem> selector)
        {
            m_pfnSelector = selector;
        }
    }
}