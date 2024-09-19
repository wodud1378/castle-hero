using System;
using System.Collections.Generic;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace RGLabs.Common.ResourceManagement
{
    public static class AtlasCache
    {
        public enum AtlasNames
        {
            // Constants.
            Common,
            Portrait,
            Global,
            
            // Main.
            Lobby,
            Shop,
            
            // Items.
            Box,
            Parts,
            Soul,
            Use,
            Equipment,
            
            // Banners.
            DungeonBanner,
        }

        private static readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> Cache = new();

        public static void Load(params AtlasNames[] names)
        {
            foreach (var name in names)
            {
                var tag = $"Atlas/{name.ToString()}.spriteatlas";
                if (Cache.ContainsKey(tag))
                    continue;
                
                var handle = Addressables.LoadAssetAsync<SpriteAtlas>(tag);
                handle.WaitForCompletion();

                Cache[tag] = handle;
            }
        }

        public static void Release(params AtlasNames[] names)
        {
            foreach (var name in names)
            {
                var tag = $"Atlas/{name.ToString()}.spriteatlas";
                if (!Cache.TryGetValue(tag, out var handle))
                    continue;
                
                handle.Release();
                Cache.Remove(tag);
            }
        }

        public static void Clear()
        {
            using var itr = Cache.GetEnumerator();
            while (itr.MoveNext())
            {
                itr.Current.Value.Release();
            }
            
            Cache.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void RegisterUnityCallback()
        {
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }
        
        private static void OnAtlasRequested(string tag, Action<SpriteAtlas> callback)
        {
            Debug.Log($"{tag} atlas requested");
            
            if (!Cache.TryGetValue(tag, out var handle))
            {
                handle = Addressables.LoadAssetAsync<SpriteAtlas>($"Atlas/{tag}.spriteatlas");
                handle.WaitForCompletion();
                Cache[tag] = handle;
            }
            
            callback.Invoke(handle.Result);
        }
    }
}