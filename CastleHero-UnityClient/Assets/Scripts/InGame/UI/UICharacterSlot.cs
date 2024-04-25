using System;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Utility;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UICharacterSlot : MonoBehaviour, IDisposable
    {
        [SerializeField] private Image _icon;
        
        public UnitEntity Entity { get; private set; }

        private AsyncOperationHandle<Sprite> _resourceHandle;
        
        public async UniTask Init(UnitEntity entity)
        {
            Entity = entity;
            _resourceHandle = await _icon.LoadImage(entity.icon);
        }
        
        public void Dispose() => _resourceHandle.Release();
    }
}