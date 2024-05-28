using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Data.DB;
using RGLabs.Data.Model;
using RGLabs.Data.User;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public class CastleFactory : IUnitFactory
    {
        private const string CastlePrefab = "Castle_01/Castle_01.prefab";
        
        private readonly CastleDB _db;
        private readonly PoolContainer _container;

        public CastleFactory(PoolContainer container, CastleDB db)
        {
            _container = container;
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
            if (!_db.TryFind(info.lv, out var entity))
                return null;

            var unitEntity = new UnitEntity
            {
                Id = 1,
                hp = entity.hp,
                atkLayer = 1,
                defLayer = 1,
            };
            
            var unit = await CreateInternal(position);
            unit.Init(info, unitEntity, default);
            unit.position = position;

            return unit;
        }
        
        private async UniTask<UnitBehaviour> CreateInternal(Vector2 position)
        {
            var unit = await _container.GetItem<UnitBehaviour>(CastlePrefab, position);
            if (unit == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{CastlePrefab}] 리소스가 존재하지 않습니다.");
#endif
                return null;
            }

            return unit;
        }
    }
}