using System;
using System.Collections.Generic;
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
    public class UnitFactory
    {
        public const string DEFAULT_CASTLE_PREFAB = "Castle_01/Castle_01.prefab";

        private readonly PoolContainer _pool;
        private readonly IDBProvider _db;
        private readonly IUserRepository _userRepo;
        private readonly HashSet<string> _managedPaths = new();

        public UnitFactory(PoolContainer pool, IDBProvider db, IUserRepository userRepo)
        {
            _pool = pool;
            _db = db;
            _userRepo = userRepo;
        }

        public IUnitActor Create(IUnitCreationData data)
        {
            return data switch
            {
                CastleCreationData castle => CreateCastle(castle),
                BarricadeCreationData barricade => CreateBarricade(barricade),
                UnitCreationData unit => CreateUnit(unit),
                _ => throw new ArgumentException($"Unknown creation data type: {data.GetType().Name}")
            };
        }

        public void PreloadResources(IEnumerable<PreloadEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (_managedPaths.Contains(entry.Path))
                    continue;

                _pool.LoadAndRegister(entry.Path, entry.Count);
                _managedPaths.Add(entry.Path);
            }
        }

        public void ReleaseResources()
        {
            foreach (var path in _managedPaths)
                _pool.Remove(path);

            _managedPaths.Clear();
        }

        public void CleanResources()
        {
            ReleaseResources();
        }

        private IUnitActor CreateUnit(UnitCreationData data)
        {
            var info = data.Info;
            if (!_db.Units.TryFind(info.id, out var unitEntity))
                return null;

            if (!_db.Balances.TryFind(info.id, out var balanceEntity))
                balanceEntity = default;

            var unit = GetFromPool(unitEntity.prefab, data.Position);
            if (unit == null)
                return null;

            unit.Init(info, unitEntity, balanceEntity);
            unit.Position = data.Position;
            return unit;
        }

        private IUnitActor CreateCastle(CastleCreationData data)
        {
            var info = data.Info;
            var prefab = string.IsNullOrEmpty(data.Prefab) ? DEFAULT_CASTLE_PREFAB : data.Prefab;

            if (!_db.Castles.TryFind(info.lv, out var entity))
                return null;

            var unitEntity = entity.ToUnitEntity();

            var unit = GetFromPool(prefab, data.Position);
            if (unit == null)
                return null;

            unit.Init(info, unitEntity, default);
            unit.Position = data.Position;
            return unit;
        }

        private IUnitActor CreateBarricade(BarricadeCreationData data)
        {
            var info = data.Info;
            int lv = _userRepo.GameRecord.CastleLv.Value;

            if (!_db.Castles.TryFind(lv, out var castleEntity))
                return null;

            if (!_db.Units.TryFind(info.id, out var unitEntity))
                return null;

            unitEntity.hp = castleEntity.barricadeHp;

            var unit = GetFromPool(unitEntity.prefab, data.Position);
            if (unit == null)
                return null;

            unit.Init(info, unitEntity, default);
            unit.Position = data.Position;
            return unit;
        }

        private UnitActor GetFromPool(string prefab, Vector2 position)
        {
            if (_pool.TryGet<UnitActor>(prefab, out var unit, position))
                return unit;

            if (_pool.LoadAndRegister(prefab))
            {
                _managedPaths.Add(prefab);
                return _pool.TryGet<UnitActor>(prefab, out unit, position) ? unit : null;
            }

            return null;
        }
    }
}
