using Cysharp.Threading.Tasks;
using RGLabs.Data;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Common.Behaviours
{
    public class Map : MonoBehaviour
    {
        private string _currentMapName;
        private GameObject _map;

        private void Awake()
        {
            Context.OnLoadCompleteQueue.Enqueue(Init);
        }

        private void Init()
        {
           Storage.userRepository.stage
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