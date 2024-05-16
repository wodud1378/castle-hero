using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Data.DB;
using RGLabs.Data.User;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public class UnitFactory : IUnitFactory
    {
        private readonly PoolContainer _pools;
        private readonly UnitDB _db;

        public UnitFactory(PoolContainer pools, UnitDB db)
        {
            _pools = pools;
            _db = db;
        }
        
        public async UniTask<UnitBehaviour> Create(int id, int lv, int grade, Vector2 position)
        {
            var info = new UnitInfo
            {
                lv = lv,
                grade = grade,
                id = id
            };

            return await Create(info, position);
        }

        public async UniTask<UnitBehaviour> Create(UnitInfo info, Vector2 position)
        {
            if (!_db.TryFind(info.id, out var entity))
                return null;

            var unit = await CreateInternal(entity.prefab, position);
            if (unit == null)
                return null;

            unit.Init(info, entity);
            unit.position = position;
            return unit;
        }

        private async UniTask<UnitBehaviour> CreateInternal(string prefab, Vector2 position)
        {
            var pool = _pools.Get(prefab);
            var unit = await pool.Get(position) as UnitBehaviour;
            if (unit == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{prefab}] 리소스가 존재하지 않습니다.");
#endif
                return null;
            }

            unit.Container = _pools;
            unit.Pool = pool;
            unit.factory = this;
            return unit;
        }
    }
}