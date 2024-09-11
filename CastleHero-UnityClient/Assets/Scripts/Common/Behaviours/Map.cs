using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Repositories;
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
        private CancellationTokenSource _ctSource;

        private void Awake()
        {
            Context.OnLoadCompleteQueue.Enqueue(Init);

            this.SubscribeMessage<StartGame>(_ => _subscription?.Dispose());
        }

        private void Init()
        {
            _subscription = Storage.userRepository.entrance
                .ThrottleFrame(1)
                .Subscribe(OnEntranceChanged)
                .AddTo(this);
        }

        private async void OnEntranceChanged(GameEntrance entrance)
        {
            if (!Storage.db.TryLoadGameEntity(entrance.type, entrance.id, out var entity))
                return;

            string mapName = entity.Map;
            if (mapName == _currentMapName)
                return;

            _ctSource?.Cancel();
            _ctSource = new();

            var legacy = _map;
            if (legacy != null)
            {
                _map = null;
                Addressables.ReleaseInstance(legacy);
            }
            
            var task = await Addressables.InstantiateAsync(mapName)
                .WithCancellation(_ctSource.Token)
                .SuppressCancellationThrow();

            if (task.IsCanceled)
            {
                // Addressables.InstantiateAsync 함수를 캔슬 했으나 맵이 남아있는 경우가 있음.
                var notCleared = GameObject.FindGameObjectsWithTag("Map");
                foreach (var obj in notCleared)
                {
                    if(_map != obj)
                        Addressables.Release(obj);
                }
                
                return;
            }

            _map = task.Result;
            _map.transform.SetParent(transform);
            _map.transform.localScale = Vector3.one;
            _currentMapName = mapName;
        }
    }
}