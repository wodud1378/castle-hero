using Cysharp.Threading.Tasks;
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
           var repo = Context.currentBehaviour.userRepo;
           repo.stage
               .Subscribe(OnStageChanged)
               .AddTo(this);
        }

        private async void OnStageChanged(int stage)
        {
            var db = Context.currentBehaviour.db.stages;
            if (!db.TryFind(stage, out var entity))
                return;

            string mapName = entity.map;
            if (mapName == _currentMapName)
                return;

            var legacy = _map;
            var handle = Addressables.InstantiateAsync(mapName);
            _map = await handle.ToUniTask();
            
            Addressables.ReleaseInstance(legacy);
        }
    }
}