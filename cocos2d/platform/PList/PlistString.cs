//
// Taken from PodSleuth (http://git.gnome.org/cgit/podsleuth)
//  
// Author:
//       Aaron Bockover <abockover@novell.com>
// 
// Copyright (c) 2007-2009 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace Cocos2D.PropertyList
{
	public class PlistString : PlistObject<string>
	{
		public PlistString(string value) : base(value)
		{
		}
		
		public override void Write (System.Xml.XmlWriter writer)
		{
			writer.WriteElementString ("string", Value);
		}

	    public override byte[] AsBinary
	    {
	        get { return System.Text.Encoding.UTF8.GetBytes(Value); }
	    }

	    public override int AsInt
	    {
	        get
	        {
                int result = 0;
                int.TryParse(Value, out result);
                return result;
            }
	    }

	    public override float AsFloat
	    {
	        get
	        {
	            float result = 0;
	            float.TryParse(Value, out result);
	            return result;
	        }
	    }

	    public override string AsString
	    {
	        get { return Value; }
	    }

	    public override DateTime AsDate
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public override bool AsBool
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public override PlistArray AsArray
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public override PlistDictionary AsDictionary
	    {
	        get { throw new NotImplementedException(); }
	    }
	}
}
