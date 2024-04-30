using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public class DefaultUnitFactory : IUnitFactory
    {
        private readonly PoolContainer _pools;
        
        public DefaultUnitFactory(PoolContainer pools)
        {
            _pools = pools;
        }
        
        public async UniTask<T> Create<T>(UnitEntity entity, Vector2 position) where T : UnitBehaviour
        {
            string prefab = entity.prefab;
            var pool = _pools.Get(prefab);
            var unit = await pool.Get(position) as T;
            if (unit == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{entity.prefab}] 리소스가 존재하지 않습니다.");
#endif
                return null;
            }
            
            unit.Container = _pools;
            unit.position = position;
            unit.Init(entity);
            return unit;
        }
    }
}