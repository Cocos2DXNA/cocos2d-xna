using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace Cocos2D
{
    public class CCSpriteSheet
    {
        private readonly Dictionary<string, CCSpriteFrame> _spriteFrames = new Dictionary<string, CCSpriteFrame>();
        private readonly Dictionary<string, string> _spriteFramesAliases = new Dictionary<string, string>();

		private PlistType plistType = PlistType.Cocos2D;

		// We need to read the sprite sheet textures relative to the plist file path.
		// When we have the sprite sheet split between multiple image files 
		// to be loaded this allows us to load those files relative to the plist.  Right now
		// only used for PlistType SpriteKit right now but can be used for other types as well
		// in the future
		private string plistFilePath;

		private enum PlistType
		{
			Cocos2D,
			SpriteKit
		}

        #region Constructors

        public CCSpriteSheet(Dictionary<string, CCSpriteFrame> frames)
        {
            if (frames != null)
            {
                _spriteFrames = new Dictionary<string, CCSpriteFrame>(frames);
                AutoCreateAliasList();
            }
        }

        public CCSpriteSheet(string fileName)
        {
            InitWithFile(fileName);
        }

        public CCSpriteSheet(string fileName, string textureFileName)
        {
            InitWithFile(fileName, textureFileName);
        }

        public CCSpriteSheet(string fileName, CCTexture2D texture)
        {
            InitWithFile(fileName, texture);
        }

        public CCSpriteSheet(Stream stream, CCTexture2D texture)
        {
            InitWithStream(stream, texture);
        }

        public CCSpriteSheet(Stream stream, string texture)
        {
            InitWithStream(stream, texture);
        }

        public CCSpriteSheet(PlistDictionary dictionary, CCTexture2D texture)
        {
            InitWithDictionary(dictionary, texture);
        }
        #endregion

        private void AutoCreateAliasList()
        {
            foreach (string key in _spriteFrames.Keys)
            {
                int idx = key.LastIndexOf('.');
                if (idx > -1)
                {
                    string alias = key.Substring(0, idx);
                    _spriteFramesAliases[alias] = key;
                    CCLog.Log("Created alias for frame {0} as {1}", key, alias);
                }
            }
        }

        private PlistType GetPlistType(PlistDictionary dict)
		{
			var isSpriteKit = dict.ContainsKey ("format") ? dict ["format"].AsString == "APPL" : false;

			return isSpriteKit ? PlistType.SpriteKit : PlistType.Cocos2D;

		}

        [Obsolete("Use the ctor. These methods will become private soon.")]
        public void InitWithFile(string fileName)
        {
            PlistDocument document = CCContentManager.SharedContentManager.Load<PlistDocument>(fileName);

            var dict = document.Root.AsDictionary;
            var texturePath = string.Empty;
			plistFilePath = string.Empty;

			plistType = GetPlistType (dict);

			if (plistType == PlistType.SpriteKit) 
			{
				var images = dict.ContainsKey ("images") ? dict ["images"].AsArray : null;

				var imageDict = images [0].AsDictionary;

				if (imageDict != null) {
					// try to read  texture file name from meta data
					if (imageDict.ContainsKey ("path")) {
						texturePath = imageDict ["path"].AsString;
					}
				}
			} 
			else 
			{
				var metadataDict = dict.ContainsKey ("metadata") ? dict ["metadata"].AsDictionary : null;

				if (metadataDict != null) {
					// try to read  texture file name from meta data
					if (metadataDict.ContainsKey ("textureFileName")) {
						texturePath = metadataDict ["textureFileName"].AsString;
					}
				}
			}

            if (!string.IsNullOrEmpty(texturePath))
            {
                // build texture path relative to plist file
                texturePath = CCFileUtils.FullPathFromRelativeFile(texturePath, fileName);
            }
            else
            {
                // build texture path by replacing file extension
                texturePath = fileName;

                // remove .xxx
                texturePath = CCFileUtils.RemoveExtension(texturePath);

                CCLog.Log("cocos2d: CCSpriteFrameCache: Trying to use file {0} as texture", texturePath);
            }

			plistFilePath = Path.GetDirectoryName (texturePath);

            CCTexture2D pTexture = CCTextureCache.SharedTextureCache.AddImage(texturePath);

            if (pTexture != null)
            {
                InitWithDictionary(dict, pTexture);
            }
            else
            {
                CCLog.Log("CCSpriteSheet: Couldn't load texture");
            }
        }

        [Obsolete("Use the ctor. These methods will become private soon.")]
        public void InitWithFile(string fileName, string textureFileName)
        {
            Debug.Assert(textureFileName != null);
            
            CCTexture2D texture = CCTextureCache.SharedTextureCache.AddImage(textureFileName);

            if (texture != null)
            {
                InitWithFile(fileName, texture);
            }
            else
            {
                CCLog.Log("CCSpriteSheet: couldn't load texture file. File not found {0}", textureFileName);
            }
        }

        [Obsolete("Use the ctor. These methods will become private soon.")]
        public void InitWithFile(string fileName, CCTexture2D texture)
        {
            PlistDocument document = CCContentManager.SharedContentManager.Load<PlistDocument>(fileName);

            PlistDictionary dict = document.Root.AsDictionary;

            InitWithDictionary(dict, texture);
        }

        [Obsolete("Use the ctor. These methods will become private soon.")]
        public void InitWithStream(Stream stream, string textureFileName)
        {
            CCTexture2D texture = CCTextureCache.SharedTextureCache.AddImage(textureFileName);

            if (texture != null)
            {
                InitWithStream(stream, texture);
            }
            else
            {
                CCLog.Log("CCSpriteSheet: couldn't load texture file. File not found {0}", textureFileName);
            }
        }

        [Obsolete("Use the ctor. These methods will become private soon.")]
        public void InitWithStream(Stream stream, CCTexture2D texture)
        {
            var document = new PlistDocument();
            try
            {
                document.LoadFromXmlFile(stream);
                plistType = GetPlistType(document.Root.AsDictionary);
            }
            catch (Exception)
            {
                throw (new Microsoft.Xna.Framework.Content.ContentLoadException("Failed to load the sprite sheet definition file from stream"));
            }

            PlistDictionary dict = document.Root.AsDictionary;

            InitWithDictionary(dict, texture);
        }

        [Obsolete("Use the ctor. These methods will become private soon.")]
        public void InitWithDictionary(PlistDictionary dict, CCTexture2D texture)
        {
            _spriteFrames.Clear();
            _spriteFramesAliases.Clear();

			if (plistType == PlistType.SpriteKit)
				LoadAppleDictionary (dict, texture);
			else
				LoadCocos2DDictionary(dict, texture);
        }

        #region Loaders

        private void LoadAppleDictionary(PlistDictionary dict, CCTexture2D texture)
		{

			var version = dict.ContainsKey ("version") ? dict ["version"].AsInt : 0; 

			if (version != 1)
				throw (new NotSupportedException("Binary PList version " + version + " is not supported."));


			var images = dict.ContainsKey ("images") ? dict ["images"].AsArray : null;

			foreach (var imageEntry in images) 
			{
				// we only support one image for now
				var imageDict = imageEntry.AsDictionary;

				var path = imageDict ["path"].AsString;

				path = Path.Combine(plistFilePath, CCFileUtils.RemoveExtension(path));

				if (!CCTextureCache.SharedTextureCache.Contains (path))
					texture = CCTextureCache.SharedTextureCache.AddImage (path);
				else
					texture = CCTextureCache.SharedTextureCache[path];



				// size not used right now
				//var size = CCSize.Parse(imageDict ["size"].AsString);

				var subImages = imageDict ["subimages"].AsArray;

				foreach (var subImage in subImages) {
					CCSpriteFrame spriteFrame = null;

					var subImageDict = subImage.AsDictionary;
					var name = subImageDict ["name"].AsString;
					var alias = subImageDict ["alias"].AsString;
					var isFullyOpaque = true;

					if (subImageDict.ContainsKey ("isFullyOpaque"))
						isFullyOpaque = subImageDict ["isFullyOpaque"].AsBool;

					var textureRect = CCRect.Parse (subImageDict ["textureRect"].AsString);
					var spriteOffset = CCPoint.Parse (subImageDict ["spriteOffset"].AsString);

					// We are going to override the sprite offset for now to be 0,0
					// It seems the offset is calculated off of the original size but if 
					// we pass this offset it throws our center position calculations off.
					spriteOffset = CCPoint.Zero;

					var textureRotated = false;
					if (subImageDict.ContainsKey ("textureRotated")) {
						textureRotated = subImageDict ["textureRotated"].AsBool;
					}
					var spriteSourceSize = CCSize.Parse (subImageDict ["spriteSourceSize"].AsString);
					var frameRect = textureRect;
					if (textureRotated)
						frameRect = new CCRect (textureRect.Origin.X, textureRect.Origin.Y, textureRect.Size.Height, textureRect.Size.Width);

#if DEBUG
					CCLog.Log ("texture {0} rect {1} rotated {2} offset {3}, sourcesize {4}", name, textureRect, textureRotated, spriteOffset, spriteSourceSize);
#endif

					// create frame
					spriteFrame = new CCSpriteFrame (
						texture,
						frameRect,
						textureRotated,
						spriteOffset,
						spriteSourceSize
					);


					_spriteFrames [name] = spriteFrame;
				}
			}
            AutoCreateAliasList();
		}

		private void LoadCocos2DDictionary(PlistDictionary dict, CCTexture2D texture)
		{
			
			PlistDictionary metadataDict = null;

			if (dict.ContainsKey("metadata"))
			{
				metadataDict = dict["metadata"].AsDictionary;
			}

			PlistDictionary framesDict = null;
			if (dict.ContainsKey("frames"))
			{
				framesDict = dict["frames"].AsDictionary;
			}

			// get the format
			int format = 0;
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
					float x = 0f, y = 0f, w = 0f, h = 0f;
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
					spriteFrame = new CCSpriteFrame(
						texture,
						new CCRect(x, y, w, h),
						false,
						new CCPoint(ox, oy),
						new CCSize(ow, oh)
						);
				}
				else if (format == 1 || format == 2)
				{
					var frame = CCRect.Parse(frameDict["frame"].AsString);
					bool rotated = false;

					// rotation
					if (format == 2)
					{
						if (frameDict.ContainsKey("rotated"))
						{
							rotated = frameDict["rotated"].AsBool;
						}
					}

					var offset = CCPoint.Parse(frameDict["offset"].AsString);
					var sourceSize = CCSize.Parse(frameDict["sourceSize"].AsString);

					// create frame
					spriteFrame = new CCSpriteFrame(texture, frame, rotated, offset, sourceSize);
				}
				else if (format == 3)
				{
					var spriteSize = CCSize.Parse(frameDict["spriteSize"].AsString);
					var spriteOffset = CCPoint.Parse(frameDict["spriteOffset"].AsString);
					var spriteSourceSize = CCSize.Parse(frameDict["spriteSourceSize"].AsString);
					var textureRect = CCRect.Parse(frameDict["textureRect"].AsString);

					bool textureRotated = false;

					if (frameDict.ContainsKey("textureRotated"))
					{
						textureRotated = frameDict["textureRotated"].AsBool;
					}

					// get aliases
					var aliases = frameDict["aliases"].AsArray;

					for (int i = 0; i < aliases.Count; i++ )
					{
						string oneAlias = aliases[i].AsString;

						if (_spriteFramesAliases.ContainsKey(oneAlias))
						{
							if (_spriteFramesAliases[oneAlias] != null)
							{
								CCLog.Log("cocos2d: WARNING: an alias with name {0} already exists", oneAlias);
							}
						}

						if (!_spriteFramesAliases.ContainsKey(oneAlias))
						{
							_spriteFramesAliases.Add(oneAlias, pair.Key);
						}
					}

					// create frame
					spriteFrame = new CCSpriteFrame(
						texture,
						new CCRect(textureRect.Origin.X, textureRect.Origin.Y, spriteSize.Width, spriteSize.Height),
						textureRotated,
						spriteOffset,
						spriteSourceSize
						);
				}

				_spriteFrames[pair.Key] = spriteFrame;
			}
            AutoCreateAliasList();
		}

        #endregion

        #region Frame Access Methods

        public List<CCSpriteFrame> Frames 
		{
			get 
			{
				return _spriteFrames.Values.ToList ();
			}

		}

		public CCSpriteFrame this [string name]
		{
			get 
			{
				return SpriteFrameByName (name);
			}
		}


        public CCSpriteFrame SpriteFrameByName(string name)
        {
            CCSpriteFrame frame;

            if (!_spriteFrames.TryGetValue(name, out frame))
            {
                string key;
                
                if (_spriteFramesAliases.TryGetValue(name, out key))
                {
                    if (!_spriteFrames.TryGetValue(key, out frame))
                    {
                        CCLog.Log("cocos2d: CCSpriteFrameCache: Frame '{0}' not found", key);
                    }
                }
            }

            if (frame != null)
            {
                CCLog.Log("cocos2d: {0} frame {1}", name, frame.Rect.ToString());
            }
            else
            {
                CCLog.Log("cocos2d: CCSpriteFrameCache: Frame '{0}' not found", name);
            }
            
            return frame;
        }

        #endregion
    }
}