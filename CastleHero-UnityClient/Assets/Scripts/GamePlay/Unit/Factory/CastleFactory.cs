using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Factory;
using CastleHero.Network.Shared;
using CastleHero.GamePlay.Unit.Behaviours;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Factory
{
    public class CastleFactory : ICastleFactory
    {
        public const string DEFAULT_CASTLE_PREFAB = "Castle_01/Castle_01.prefab";

        private readonly PoolContainer _container;
        private readonly IDBProvider _db;

        public CastleFactory(PoolContainer container, IDBProvider db)
        {
            _container = container;
            _db = db;
        }

        public async UniTask<IUnitBehaviour> Create(int id, int lv, int grade, Vector2 position)
        {
            var info = new UnitInfo
            {
                lv = lv,
                rate = grade,
                id = id
            };

            return await Create(info, position);
        }

        public UniTask<IUnitBehaviour> Create(UnitInfo info, Vector2 position) =>
            Create(DEFAULT_CASTLE_PREFAB, info, position);

        public async UniTask<IUnitBehaviour> Create(string prefab, UnitInfo info, Vector2 position)
        {
            if (!_db.Castles.TryFind(info.lv, out var entity))
                return null;

            var unitEntity = entity.ToUnitEntity();

            var unit = await CreateInternal(prefab, position);
            unit.Init(info, unitEntity, default);
            unit.Position = position;

            return unit;
        }

        public async UniTask<IUnitBehaviour> CreateBarricade(UnitInfo info, Vector2 position)
        {
            return await Create(info, position);
        }

        private UniTask<UnitBehaviour> CreateInternal(string prefab, Vector2 position)
        {
            if (!_container.TryGet<UnitBehaviour>(prefab, out var unit, position))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{prefab}] 풀에 프리팹이 등록되지 않았습니다. Preloader 에서 누락.");
#endif
                return UniTask.FromResult<UnitBehaviour>(null);
            }

            return UniTask.FromResult(unit);
        }
    }
}
