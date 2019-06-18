using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.IO;

namespace Cocos2D
{
    public class CCSpriteFrameCache
    {
        public static CCSpriteFrameCache pSharedSpriteFrameCache = null;
        protected Dictionary<string, CCSpriteFrame> m_pSpriteFrames;
        protected Dictionary<string, string> m_pSpriteFramesAliases;
        private bool _AllowFrameOverwrite = false;

        public bool Init()
        {
            m_pSpriteFrames = new Dictionary<string, CCSpriteFrame>();
            m_pSpriteFramesAliases = new Dictionary<string, string>();
            return true;
        }

        /// <summary>
        /// When false, an exception is thrown if an animation frame is overwritten.
        /// </summary>
        public bool AllowFrameOverrite
        {
            get
            {
                return (_AllowFrameOverwrite);
            }
            set
            {
                _AllowFrameOverwrite = value;
            }
        }

        public void AddSpriteFramesWithDictionary(PlistDictionary pobDictionary, CCTexture2D pobTexture)
        {
            /*
            Supported Zwoptex Formats:

            ZWTCoordinatesFormatOptionXMLLegacy = 0, // Flash Version
            ZWTCoordinatesFormatOptionXML1_0 = 1, // Desktop Version 0.0 - 0.4b
            ZWTCoordinatesFormatOptionXML1_1 = 2, // Desktop Version 1.0.0 - 1.0.1
            ZWTCoordinatesFormatOptionXML1_2 = 3, // Desktop Version 1.0.2+
            */

            PlistDictionary metadataDict = null;
            if (pobDictionary.ContainsKey("metadata"))
            {
                metadataDict = pobDictionary["metadata"].AsDictionary;
            }

            PlistDictionary framesDict = null;
            if (pobDictionary.ContainsKey("frames"))
            {
                framesDict = pobDictionary["frames"].AsDictionary;
            }

            int format = 0;

            // get the format
            if (metadataDict != null)
            {
                format = metadataDict["format"].AsInt;
            }

            // check the format
            if (format < 0 || format > 3)
            {
                throw (new NotSupportedException("PList format " + format + " is not supported."));
            }

            foreach (var pair in framesDict)
            {
                PlistDictionary frameDict = pair.Value.AsDictionary;
                CCSpriteFrame spriteFrame = null;

                if (format == 0)
                {
                    float x=0f, y=0f, w=0f, h=0f;
                    x = frameDict["x"].AsFloat;
                    y = frameDict["y"].AsFloat;
                    w = frameDict["width"].AsFloat;
                    h = frameDict["height"].AsFloat;
                    float ox = 0f, oy = 0f;
                    ox = frameDict["offsetX"].AsFloat;
                    oy = frameDict["offsetY"].AsFloat;
                    int ow = 0, oh = 0;
                    ow = frameDict["originalWidth"].AsInt;
                    oh = frameDict["originalHeight"].AsInt;
                    // check ow/oh
                    if (ow == 0 || oh == 0)
                    {
                        CCLog.Log(
                            "cocos2d: WARNING: originalWidth/Height not found on the CCSpriteFrame. AnchorPoint won't work as expected. Regenerate the .plist or check the 'format' metatag");
                    }
                    // abs ow/oh
                    ow = Math.Abs(ow);
                    oh = Math.Abs(oh);
                    // create frame
                    spriteFrame = new CCSpriteFrame(pobTexture,
                                                new CCRect(x, y, w, h),
                                                false,
                                                new CCPoint(ox, oy),
                                                new CCSize(ow, oh)
                        );
                }
                else if (format == 1 || format == 2)
                {
					CCRect frame = CCRect.Parse(frameDict["frame"].AsString);
                    bool rotated = false;

                    // rotation
                    if (format == 2)
                    {
                        if (frameDict.ContainsKey("rotated"))
                        {
                            rotated = frameDict["rotated"].AsBool;
                        }
                    }

                    CCPoint offset = CCPoint.Parse(frameDict["offset"].AsString);
					CCSize sourceSize = CCSize.Parse (frameDict["sourceSize"].AsString);

                    // create frame
                    spriteFrame = new CCSpriteFrame(pobTexture,
                                                frame,
                                                rotated,
                                                offset,
                                                sourceSize
                        );
                }
                else if (format == 3)
                {
                    // get values
                    CCSize spriteSize = CCSize.Parse (frameDict["spriteSize"].AsString);
                    CCPoint spriteOffset = CCPoint.Parse(frameDict["spriteOffset"].AsString);
					CCSize spriteSourceSize = CCSize.Parse (frameDict["spriteSourceSize"].AsString);
                    CCRect textureRect = CCRect.Parse(frameDict["textureRect"].AsString);
                    bool textureRotated = false;
                    if (frameDict.ContainsKey("textureRotated"))
                    {
                        textureRotated = frameDict["textureRotated"].AsBool;
                    }

                    // get aliases
                    PlistArray aliases = frameDict["aliases"].AsArray;
                    string frameKey = pair.Key;

                    foreach (PlistObjectBase item2 in aliases)
                    {
                        string oneAlias = item2.AsString;
                        if (m_pSpriteFramesAliases.Keys.Contains(oneAlias))
                        {
                            if (m_pSpriteFramesAliases[oneAlias] != null)
                            {
                                CCLog.Log("cocos2d: WARNING: an alias with name {0} already exists", oneAlias);
                            }
                        }
                        if (!m_pSpriteFramesAliases.Keys.Contains(oneAlias))
                        {
                            m_pSpriteFramesAliases.Add(oneAlias, frameKey);
                        }
                    }

                    // create frame
                    spriteFrame = new CCSpriteFrame(pobTexture,
                                                new CCRect(textureRect.Origin.X, textureRect.Origin.Y, spriteSize.Width, spriteSize.Height),
                                                textureRotated,
                                                spriteOffset,
                                                spriteSourceSize);
                }

                // add sprite frame
                string key = pair.Key;
                if (!_AllowFrameOverwrite && m_pSpriteFrames.ContainsKey(key))
                {
                    CCLog.Log("Frame named " + key + " already exists in the animation cache. Not overwriting existing record.");
                }
                else if (_AllowFrameOverwrite || !m_pSpriteFrames.ContainsKey(key))
                {
                    m_pSpriteFrames[key] = spriteFrame;
                }
            }
        }

        public void AddSpriteFramesWithFile(string pszPlist)
        {
            PlistDocument document = CCContentManager.SharedContentManager.Load<PlistDocument>(pszPlist);

            PlistDictionary dict = document.Root.AsDictionary;
            string texturePath = "";
            PlistDictionary metadataDict = dict.ContainsKey("metadata") ? dict["metadata"].AsDictionary : null;

            if (metadataDict != null)
            {
                // try to read  texture file name from meta data
                if (metadataDict.ContainsKey("textureFileName"))
                {
                    texturePath = metadataDict["textureFileName"].AsString;
                }
            }

            if (!string.IsNullOrEmpty(texturePath))
            {
                // build texture path relative to plist file
                texturePath = CCFileUtils.FullPathFromRelativeFile(texturePath, pszPlist);
            }
            else
            {
                // build texture path by replacing file extension
                texturePath = pszPlist;

                // remove .xxx
                texturePath = CCFileUtils.RemoveExtension(texturePath);

                // append .png
                texturePath = texturePath + ".png";

                CCLog.Log("cocos2d: CCSpriteFrameCache: Trying to use file {0} as texture", texturePath);
            }

            CCTexture2D pTexture = CCTextureCache.SharedTextureCache.AddImage(texturePath);

            if (pTexture != null)
            {
                AddSpriteFramesWithDictionary(dict, pTexture);
            }
            else
            {
                CCLog.Log("cocos2d: CCSpriteFrameCache: Couldn't load texture");
            }
        }

        public void AddSpriteFramesWithFile(string plist, string textureFileName)
        {
            Debug.Assert(textureFileName != null);
            
            CCTexture2D texture = CCTextureCache.SharedTextureCache.AddImage(textureFileName);

            if (texture != null)
            {
                AddSpriteFramesWithFile(plist, texture);
            }
            else
            {
                CCLog.Log("cocos2d: CCSpriteFrameCache: couldn't load texture file. File not found {0}", textureFileName);
            }
        }

        public void AddSpriteFramesWithFile(Stream plist, CCTexture2D pobTexture)
        {
            PlistDocument document = new PlistDocument();
            try
            {
                document.LoadFromXmlFile(plist);
            }
            catch (Exception)
            {
                throw (new Microsoft.Xna.Framework.Content.ContentLoadException("Failed to load the particle definition file from stream"));
            }

            PlistDictionary dict = document.Root.AsDictionary;

            AddSpriteFramesWithDictionary(dict, pobTexture);
        }

        public void AddSpriteFramesWithFile(string pszPlist, CCTexture2D pobTexture)
        {
            PlistDocument document = CCContentManager.SharedContentManager.Load<PlistDocument>(pszPlist);

            PlistDictionary dict = document.Root.AsDictionary;

            AddSpriteFramesWithDictionary(dict, pobTexture);
        }

        public void AddSpriteFrame(CCSpriteFrame pobFrame, string pszFrameName)
        {
            if (!_AllowFrameOverwrite && m_pSpriteFrames.ContainsKey(pszFrameName))
            {
                throw (new ArgumentException("The frame named " + pszFrameName + " already exists."));
            }
            m_pSpriteFrames[pszFrameName] = pobFrame;
        }

        public void RemoveSpriteFrames()
        {
            m_pSpriteFrames.Clear();
            m_pSpriteFramesAliases.Clear();
        }

        public void RemoveUnusedSpriteFrames()
        {
            if (m_pSpriteFrames.Count > 0)
            {
                var tmp = new Dictionary<string, WeakReference>();

                foreach (var pair in m_pSpriteFrames)
                {
                    tmp.Add(pair.Key, new WeakReference(pair.Value));
                }

                m_pSpriteFrames.Clear();

                GC.Collect();

                foreach (var pair in tmp)
                {
                    if (pair.Value.IsAlive)
                    {
                        m_pSpriteFrames.Add(pair.Key, (CCSpriteFrame) pair.Value.Target);
                    }
                }
            }
        }

        public void RemoveSpriteFrameByName(string pszName)
        {
            // explicit nil handling
            if (string.IsNullOrEmpty(pszName))
            {
                return;
            }

            // Is this an alias ?
            string key = m_pSpriteFramesAliases[pszName];

            if (!string.IsNullOrEmpty(key))
            {
                m_pSpriteFrames.Remove(key);
                m_pSpriteFramesAliases.Remove(key);
            }
            else
            {
                m_pSpriteFrames.Remove(pszName);
            }
        }

        public void RemoveSpriteFramesFromFile(string plist)
        {
            PlistDocument document = CCContentManager.SharedContentManager.Load<PlistDocument>(plist);

            PlistDictionary dict = document.Root.AsDictionary;

            RemoveSpriteFramesFromDictionary(dict);
        }

        public void RemoveSpriteFramesFromDictionary(PlistDictionary dictionary)
        {
            PlistDictionary framesDict = dictionary["frames"].AsDictionary;
            var keysToRemove = new List<string>();

            foreach (var pair in framesDict)
            {
                if (m_pSpriteFrames.ContainsKey(pair.Key))
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (string key in keysToRemove)
            {
                m_pSpriteFrames.Remove(key);
            }
        }

        public void RemoveSpriteFramesFromTexture(CCTexture2D texture)
        {
            var keysToRemove = new List<string>();

            foreach (string key in m_pSpriteFrames.Keys)
            {
                CCSpriteFrame frame = m_pSpriteFrames[key];
                if (frame != null && (frame.Texture.Name == texture.Name))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (string key in keysToRemove)
            {
                m_pSpriteFrames.Remove(key);
            }
        }

        /// <summary>
        /// Get the sprite frame for the given frame name, or as an alias for the sprite frame name.
        /// </summary>
        /// <param name="pszName"></param>
        /// <returns></returns>
        public CCSpriteFrame SpriteFrameByName(string pszName)
        {
            CCSpriteFrame frame;

            if (!m_pSpriteFrames.TryGetValue(pszName, out frame))
            {
                // try alias dictionary
                string key;
                if (m_pSpriteFramesAliases.TryGetValue(pszName, out key))
                {
                    if (!m_pSpriteFrames.TryGetValue(key, out frame))
                    {
                        CCLog.Log("cocos2d: CCSpriteFrameCahce: Frame '{0}' not found", pszName);
                    }
                }
            }

            if (frame != null)
            {
                CCLog.Log("cocos2d: {0} frame {1}", pszName, frame.Rect.ToString());
            }
            return frame;
        }

        public static CCSpriteFrameCache SharedSpriteFrameCache
        {
            get
            {
                if (pSharedSpriteFrameCache == null)
                {
                    pSharedSpriteFrameCache = new CCSpriteFrameCache();
                    pSharedSpriteFrameCache.Init();
                }

                return pSharedSpriteFrameCache;
            }
        }

        public static void PurgeSharedSpriteFrameCache()
        {
            pSharedSpriteFrameCache = null;
        }
    }
}