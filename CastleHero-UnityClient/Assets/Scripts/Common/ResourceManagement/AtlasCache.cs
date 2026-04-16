using System;
using System.Collections.Generic;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace CastleHero.Common.ResourceManagement
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