using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Common.ResourceManagement;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.UnitFactory
{
    public class DefaultUnitFactory : IUnitFactory
    {
        private readonly Dictionary<string, AddressablePool<GameUnit>> _pools = new();

        private readonly AssetBundleResource _resource;

        public async UniTask<T> Create<T>(UnitEntity entity, Vector2 position) where T : GameUnit
        {
            string prefab = entity.prefab;
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new AddressablePool<GameUnit>(prefab);
                _pools[prefab] = pool;
            }

            var unit = await pool.Get() as T;
            if (unit == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{entity.prefab}] 리소스가 존재하지 않습니다.");
#endif
                return null;
            }

            unit.Position = position;
            unit.Init(entity);
            return unit;
        }
    }
}