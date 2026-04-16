using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Factory;
using CastleHero.Data.Repositories;
using CastleHero.Network.Shared;
using CastleHero.GamePlay.Unit.Behaviours;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Factory
{
    public class UnitFactory : IUnitFactory
    {
        private readonly PoolContainer _container;
        private readonly IDBProvider _db;
        private readonly IUserRepository _userRepo;

        public UnitFactory(PoolContainer container, IDBProvider db, IUserRepository userRepo)
        {
            _container = container;
            _db = db;
            _userRepo = userRepo;
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

        public async UniTask<IUnitBehaviour> CreateBarricade(UnitInfo info, Vector2 position)
        {
            int lv = _userRepo.GameRecord.CastleLv.Value;
            if (!_db.Castles.TryFind(lv, out var castleEntity))
                return null;

            if (!_db.Units.TryFind(info.id, out var unitEntity))
                return null;

            unitEntity.hp = castleEntity.barricadeHp;

            var unit = await CreateInternal(unitEntity.prefab, position);
            if (unit == null)
                return null;

            unit.Init(info, unitEntity, default);
            unit.Position = position;
            return unit;
        }

        public async UniTask<IUnitBehaviour> Create(UnitInfo info, Vector2 position)
        {
            if (!_db.Units.TryFind(info.id, out var unitEntity))
                return null;

            var unit = await CreateInternal(unitEntity.prefab, position);
            if (unit == null)
                return null;

            if (!_db.Balances.TryFind(info.id, out var balanceEntity))
                balanceEntity = default;

            unit.Init(info, unitEntity, balanceEntity);
            unit.Position = position;
            return unit;
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
