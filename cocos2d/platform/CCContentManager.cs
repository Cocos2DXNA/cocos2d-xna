﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Cocos2D
{
    public class CCContentManager : ContentManager
    {
        public static CCContentManager SharedContentManager;

        internal static void Initialize(IServiceProvider serviceProvider, string rootDirectory)
        {
            SharedContentManager = new CCContentManager(serviceProvider, rootDirectory);
        }

        private Dictionary<string, object> _loadedAssets = new Dictionary<string, object>();
        private Dictionary<string, WeakReference> _loadedWeakAssets = new Dictionary<string, WeakReference>();
        
        private Dictionary<string, string> _assetLookupDict = new Dictionary<string, string>();
        private List<string> _searchPaths = new List<string>();
        private List<string> _searchResolutionsOrder = new List<string>(); 

        private Dictionary<string, string> _assetPathCache = new Dictionary<string, string>();

        public CCContentManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public CCContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
        }

        public T TryLoad<T>(string assetName)
        {
            try
            {
                return Load<T>(assetName);
            }
            catch (Exception)
            {
                _assetPathCache[assetName] = null;
                
                return default(T);
            }
        }

        public override T Load<T>(string assetName)
        {
            return Load<T>(assetName, false);
        }

        public T Load<T>(string assetName, bool weakReference)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }

            // Check for a previously loaded asset first
            object asset;
            if (_loadedAssets.TryGetValue(assetName, out asset))
            {
                if (asset is T)
                {
                    return (T)asset;
                }
            }

            WeakReference reference;
            if (_loadedWeakAssets.TryGetValue(assetName, out reference))
            {
                if (reference.IsAlive)
                {
                    if (reference.Target is T)
                    {
                        return (T) reference.Target;
                    }
                }
                else
                {
                    _loadedWeakAssets.Remove(assetName);
                }
            }

            CheckDefaultPath(_searchPaths);
            CheckDefaultPath(_searchResolutionsOrder);

            foreach (var searchPath in _searchPaths)
            {
                foreach (string resolutionOrder  in _searchResolutionsOrder)
                {
                    var path = Path.Combine(Path.Combine(searchPath, resolutionOrder), assetName);

                    try
                    {
                        return InternalLoad<T>(assetName, path, weakReference);
                    }
                    catch (ContentLoadException)
                    {
                        // try other path
                    }
                }
            }

            throw new ContentLoadException("Failed to load the asset file from " + assetName);
        }

        private T InternalLoad<T>(string assetName, string path, bool weakReference)
        {
            T result = default(T);

            try
            {
                //Special types
                if (typeof(T) == typeof(string))
                {
                    //TODO: No need CCContent, use TileContainer
                    var content = ReadAsset<CCContent>(path, null);
                    result = (T)(object)content.Content;
                }
                else
                {
                    // Load the asset.
                    result = ReadAsset<T>(path, null);
                }
            }
            catch (ContentLoadException)
            {

                if (typeof(T) == typeof(PlistDocument))
                {
                    //TODO: No need CCContent, use TileContainer
                    var content = ReadAsset<CCContent>(path, null);
                    result = (T) (object) new PlistDocument(content.Content);
                }

                throw new ContentLoadException("Failed to load the asset file from " + assetName);
            }

            if (weakReference)
            {
                _loadedWeakAssets[assetName] = new WeakReference(result);
            }
            else
            {
                _loadedAssets[assetName] = result;
            }

            return result;
        }

        public virtual void LoadFilenameLookupDictionaryFromFile(string filename)
        {
            var document = Load<PlistDocument>(filename, true);
            
            SetFilenameLookupDictionary(document.Root as PlistDictionary);
        }

        public virtual void SetFilenameLookupDictionary(PlistDictionary filenameLookupDict)
        {
            //TODO: Load lookup names from PlistDictionary
        }

        public List<string> SearchResolutionsOrder
        {
            get { return _searchResolutionsOrder; }
        }

        public List<string> SearchPaths
        {
            get { return _searchPaths; }
        }

        private void CheckDefaultPath(List<string> paths)
        {
            for (int i = paths.Count - 1; i >= 0; i--)
            {
                if (paths[i] == "")
                {
                    return;
                }
            }

            paths.Add("");
        }
    }
}
