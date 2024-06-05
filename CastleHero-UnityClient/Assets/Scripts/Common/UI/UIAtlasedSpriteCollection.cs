using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UIAtlasedSpriteCollection : MonoBehaviour, IDisposable
    {
        public UIAtlasedSprite[] sprites;

        public async UniTask LoadAll()
        {
            var tasks = new List<UniTask>();
            foreach (var sprite in sprites)
            {
                tasks.Add(sprite.Load());
            }

            await UniTask.WhenAll(tasks);
        }
        
        public void Dispose()
        {
        }
    }
}