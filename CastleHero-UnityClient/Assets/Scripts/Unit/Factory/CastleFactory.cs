using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Pattern;
using RGLabs.Data;
using RGLabs.Data.DB;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Network.Shared;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public class CastleFactory : IUnitFactory
    {
        public const string DEFAULT_CASTLE_PREFAB = "Castle_01/Castle_01.prefab";
        
        private readonly CastleDB _db = Storage.db.castles;
        private readonly PoolContainer _container = Context.poolContainer;

        public async UniTask<UnitBehaviour> Create(int id, int lv, int grade, Vector2 position)
        {
            var info = new UnitInfo
            {
                lv = lv,
                rate = grade,
                id = id
            };

            return await Create(info, position);
        }

        public UniTask<UnitBehaviour> Create(UnitInfo info, Vector2 position) => Create(DEFAULT_CASTLE_PREFAB, info, position);
        
        public async UniTask<UnitBehaviour> Create(string prefab, UnitInfo info, Vector2 position)
        {
            if (!_db.TryFind(info.lv, out var entity))
                return null;

            var unitEntity = entity.ToUnitEntity();
            
            var unit = await CreateInternal(prefab, position);
            unit.Init(info, unitEntity, default);
            unit.position = position;

            return unit;
        }
        
        private async UniTask<UnitBehaviour> CreateInternal(string prefab, Vector2 position)
        {
            var unit = await _container.GetItem<UnitBehaviour>(prefab, position);
            if (unit == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{DEFAULT_CASTLE_PREFAB}] 리소스가 존재하지 않습니다.");
#endif
                return null;
            }

            return unit;
        }
    }
}