using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public class UnitFactory : IUnitFactory
    {
        private readonly PoolContainer _pools;

        public UnitFactory(PoolContainer pools)
        {
            _pools = pools;
        }
        
        public async UniTask<UnitBehaviour> Create(UnitEntity entity, Vector2 position)
        {
            string prefab = entity.prefab;
            var pool = _pools.Get(prefab);
            var unit = await pool.Get(position) as UnitBehaviour;
            if (unit == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{entity.prefab}] 리소스가 존재하지 않습니다.");
#endif
                return null;
            }

            unit.Container = _pools;
            unit.Init(entity);
            unit.position = position;
            return unit;
        }
    }
}