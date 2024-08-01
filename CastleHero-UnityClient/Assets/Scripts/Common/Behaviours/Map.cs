using System;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Lobby.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Common.Behaviours
{
    public class Map : MonoBehaviour
    {
        private string _currentMapName;
        private GameObject _map;
        private IDisposable _subscription;

        private void Awake()
        {
            Context.OnLoadCompleteQueue.Enqueue(Init);
            
            this.SubscribeMessage<StartGame>(_=> _subscription?.Dispose());
        }

        private void Init()
        {
           _subscription = Storage.userRepository.focusedStage
               .ThrottleFrame(1)
               .Subscribe(OnStageChanged)
               .AddTo(this);
        }

        private async void OnStageChanged(int stage)
        {
            var db = Storage.db.stages;
            if (!db.TryFind(stage, out var entity))
                return;

            string mapName = entity.map;
            if (mapName == _currentMapName)
                return;

            var legacy = _map;
            var handle = Addressables.InstantiateAsync(mapName);
            _map = await handle.ToUniTask();
            _map.transform.SetParent(transform);
            _map.transform.localScale = Vector3.one;
            _currentMapName = mapName;
  
            if(legacy != null)
                Addressables.ReleaseInstance(legacy);
        }
    }
}