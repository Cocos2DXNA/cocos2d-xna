using System.Collections.Generic;
using System.Linq;

namespace Cocos2D
{
    public class CCTouchDelegate : ICCTouchDelegate
    {
        protected Dictionary<int, string> m_pEventTypeFuncMap;

        public virtual int TouchPriority
        {
            get
            {
                return (0);
            }
        }
        public virtual bool VisibleForTouches
        {
            get
            {
                return (true);
            }
            set
            {
                // do nothing
            }
        }
        /// <summary>
        /// functions for script call back
        /// </summary>
        public void RegisterScriptTouchHandler(int eventType, string pszScriptFunctionName)
        {
            if (m_pEventTypeFuncMap == null)
            {
                m_pEventTypeFuncMap = new Dictionary<int, string>();
            }

            (m_pEventTypeFuncMap)[eventType] = pszScriptFunctionName;
        }

        public bool DoesScriptHandlerExist(int eventType)
        {
            if (m_pEventTypeFuncMap != null)
            {
#if NETFX_CORE
                return this.m_pEventTypeFuncMap != null && this.m_pEventTypeFuncMap[eventType].Length != 0;
#else
                return (m_pEventTypeFuncMap)[eventType].Count() != 0;
#endif
            }

            return false;
        }

        public void ExcuteScriptTouchHandler(int eventType, CCTouch pTouch)
        {
            if (m_pEventTypeFuncMap != null && CCScriptEngineManager.SharedScriptEngineManager.ScriptEngine != null)
            {
                CCScriptEngineManager.SharedScriptEngineManager.ScriptEngine.ExecuteTouchEvent((m_pEventTypeFuncMap)[eventType],
                                                                                                 pTouch);
            }
        }

        public void ExcuteScriptTouchesHandler(int eventType, List<CCTouch> pTouches)
        {
            if (m_pEventTypeFuncMap != null && CCScriptEngineManager.SharedScriptEngineManager.ScriptEngine != null)
            {
                CCScriptEngineManager.SharedScriptEngineManager.ScriptEngine.ExecuteTouchesEvent((m_pEventTypeFuncMap)[eventType],
                                                                                                   pTouches);
            }
        }
    }
}