using System.Collections.Generic;
using RGLabs.Common.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace RGLabs.Editor
{
    public static class AtlasHelper
    {
        [MenuItem("GameObject/RGLabs/Attach Atlas Reference")]
        public static void AttachAtlasReference()
        {
            var selection = Selection.gameObjects?[0];
            if (selection == null)
                return;

            var atlasedSprites = new List<UIAtlasedSprite>();
            var all = GetAssets<SpriteAtlas>("t: SpriteAtlas");
            var images = selection.GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                var sprite = image.sprite;
                if (sprite == null)
                    continue;

                var found = all.Find((x) => x.GetSprite(sprite.name));
                if (found == null)
                    continue;
                
                if(!image.TryGetComponent(out UIAtlasedSprite atlasedSprite)) 
                    atlasedSprite = image.gameObject.AddComponent<UIAtlasedSprite>();
                
                var reference = new AssetReferenceAtlasedSprite(GetGuid(found));
                reference.SetEditorSubObject(sprite);
                
                atlasedSprite.image = image;
                atlasedSprite.reference = reference;
                
                atlasedSprites.Add(atlasedSprite);
                
                EditorUtility.SetDirty(image);
            }
            
            if (!selection.TryGetComponent(out UIAtlasedSpriteCollection atlasRef))
                atlasRef = selection.AddComponent<UIAtlasedSpriteCollection>();

            atlasRef.sprites = atlasedSprites.ToArray();
            
            EditorUtility.SetDirty(selection);
        }        
        private static string GetGuid(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            
            return AssetDatabase.AssetPathToGUID(path);
        }

        private static List<T> GetAssets<T>(string filter, string[] folders = null) where T : Object
        {
            var list = new List<T>();
            var guids = AssetDatabase.FindAssets(filter, folders);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset == null)
                    continue;

                list.Add(asset);
            }

            return list.Count == 0 ? null : list;
        }
    }
}