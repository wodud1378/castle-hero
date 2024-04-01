using System;
using RGLabs.Common.ResourceManagement;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System.Spawn;
using UnityEngine.AddressableAssets;

namespace RGLabs.InGame.System.MonsterFactory
{
    public class DefaultMonsterFactory : IMonsterFactory
    {
        // TODO: DI?
        private AssetBundleResource _resource = new();

        public DefaultMonsterFactory()
        {
            // TODO : 다운로드 구문 추후에 게임 시작으로 이동.
            Addressables.InitializeAsync().Completed += (h) =>
            {
                Addressables.DownloadDependenciesAsync("Monster_001");
            };
        }
        
        public void PushCreationRequest(MonsterEntity entity, Action<GameUnit> onCreated)
        {
            _resource.Instantiate(entity.prefab, onCreated);
        }
    }
}