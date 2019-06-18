using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Cocos2D
{
#if !NETFX_CORE
	public class CCSizeConverter : TypeConverter
	{
        public CCSizeConverter() { }

		// Overrides the CanConvertFrom method of TypeConverter.
		// The ITypeDescriptorContext interface provides the context for the
		// conversion. Typically, this interface is used at design time to 
		// provide information about the design-time container.
		public override bool CanConvertFrom(ITypeDescriptorContext context, 
		                                    Type sourceType) {
			
			if (sourceType == typeof(string)) {
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}
		// Overrides the ConvertFrom method of TypeConverter.
		public override object ConvertFrom(ITypeDescriptorContext context, 
		                                   CultureInfo culture, object value) {
			if (value is string) {
				return CCSizeFromString(value as String);
			}
			return base.ConvertFrom(context, culture, value);
		}
		
		// Overrides the ConvertTo method of TypeConverter.
		public override object ConvertTo(ITypeDescriptorContext context, 
		                                 CultureInfo culture, object value, Type destinationType) {  
			if (destinationType == typeof(string)) {
				return "{" + ((CCSize)value).Width + "," + ((CCSize)value).Height + "}";
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
		
		public static CCSize CCSizeFromString(string pszContent)
		{
			CCSize ret = new CCSize();
			
			do
			{
				List<string> strs = new List<string>();
				if (!CCUtils.SplitWithForm(pszContent, strs)) break;
				
				float width = CCUtils.CCParseFloat(strs[0]);
				float height = CCUtils.CCParseFloat(strs[1]);
				
				ret = new CCSize(width, height);
			} while (false);
			
			return ret;
		}
		
	}
#else
    public class CCSizeConverter
    {
        public static CCSize CCSizeFromString(string pszContent)
        {
            CCSize ret = new CCSize();

            do
            {
                List<string> strs = new List<string>();
                if (!CCUtils.SplitWithForm(pszContent, strs)) break;

                float width = CCUtils.CCParseFloat(strs[0]);
                float height = CCUtils.CCParseFloat(strs[1]);

                ret = new CCSize(width, height);
            } while (false);

            return ret;
        }

    }
#endif
    }

