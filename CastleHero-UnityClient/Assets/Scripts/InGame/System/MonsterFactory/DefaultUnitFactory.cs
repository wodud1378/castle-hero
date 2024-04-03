using System;
using System.Collections.Generic;
using RGLabs.Common.Pattern;
using RGLabs.Common.ResourceManagement;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System.Spawn;
using UnityEngine.AddressableAssets;

namespace RGLabs.InGame.System.MonsterFactory
{
    public class DefaultUnitFactory : IUnitFactory
    {
        // TODO: DI?
        private AssetBundleResource _resource = new();

        private Dictionary<string, ObjectPool<GameUnit>> _pools = new();

        public DefaultUnitFactory()
        {
            // TODO : 다운로드 구문 추후에 게임 시작으로 이동.
            Addressables.InitializeAsync().Completed += (h) =>
            {
                Addressables.DownloadDependenciesAsync("Monster_001");
            };
        }
        
        public void PushCreationRequest<T>(UnitEntity entity, Action<T> onCreated) where T : GameUnit
        {
            _resource.Instantiate(entity.prefab, onCreated);
        }
    }
}