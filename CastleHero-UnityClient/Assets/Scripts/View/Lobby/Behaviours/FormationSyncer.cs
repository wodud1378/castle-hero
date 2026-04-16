using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Factory;
using CastleHero.Network.Shared;
using UniRx;

namespace CastleHero.View.Lobby.Behaviours
{
    /// <summary>
    /// 유저 리포지토리의 변경 스트림을 구독하여 FormationComposer 를 동기화한다.
    /// Composer 가 가진 상태에 대한 외부 반영만 담당하고, 스스로는 상태를 소유하지 않는다.
    /// </summary>
    public sealed class FormationSyncer : IDisposable
    {
        private readonly FormationComposer _composer;
        private readonly IUserRepository _userRepo;
        private readonly IDBProvider _db;
        private readonly CompositeDisposable _disposables = new();

        public FormationSyncer(FormationComposer composer, IUserRepository userRepo, IDBProvider db)
        {
            _composer = composer;
            _userRepo = userRepo;
            _db = db;
        }

        public void Start()
        {
            _userRepo.GameRecord.CastleLv
                .Subscribe(OnCastleLevelChanged)
                .AddTo(_disposables);

            _userRepo.Characters
                .WhenUpdate(UpdateUnits)
                .AddTo(_disposables);

            _userRepo.Inventory
                .WhenUpdate(UpdateUnitsWhereHasEquipments)
                .AddTo(_disposables);

            _userRepo.Entrance
                .Subscribe(ReloadCastle)
                .AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();

        private void UpdateUnits(ReactiveCollection<UnitInfo> units)
        {
            foreach (var unit in units)
            {
                var inField = _composer.Draft.Characters.FirstOrDefault(x => x.Id == unit.id) as UnitBehaviour;
                if (inField != null &&
                    _db.Units.TryFind(unit.id, out var entity) &&
                    _db.Balances.TryFind(unit.id, out var balance))
                {
                    inField.Core.Update(unit, entity, balance);
                }
            }
        }

        private void UpdateUnitsWhereHasEquipments(ReactiveCollection<IItem> items)
        {
            var ids = items.OfType<EquipItem>()
                .Select(x => x.character)
                .Where(x => x != 0)
                .Distinct();

            foreach (var id in ids)
            {
                var unit = _userRepo.Characters.Units.FirstOrDefault(x => x.id == id);
                var inField = _composer.Draft.Characters.FirstOrDefault(x => x.Id == id) as UnitBehaviour;

                if (unit != null &&
                    inField != null &&
                    _db.Units.TryFind(unit.id, out var entity) &&
                    _db.Balances.TryFind(unit.id, out var balance))
                {
                    inField.Core.Update(unit, entity, balance);
                }
            }
        }

        private void ReloadCastle(GameEntrance entrance)
        {
            string prefab;
            if (entrance.type == GameType.Dungeon)
            {
                prefab = _db.Dungeons.TryFind(entrance.id, out var dungeonEntity)
                    ? dungeonEntity.castlePrefab
                    : string.Empty;
            }
            else
            {
                prefab = CastleFactory.DEFAULT_CASTLE_PREFAB;
            }

            _composer.LoadCastle(prefab).Forget();
        }

        private void OnCastleLevelChanged(int lv)
        {
            if (_db.Castles.TryFind(lv, out var entity))
            {
                _composer.ApplyCastleCapacity(entity.maxCharacter, entity.barricadeCount);
                _composer.ReapplyCastle(lv, entity);
            }
        }
    }
}
