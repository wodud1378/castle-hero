using System;
using System.Collections.Generic;
using CastleHero.Common.Pattern;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Factory;

using CastleHero.Data.Repositories;
namespace CastleHero.GamePlay.InGame
{
    public class StagePreloadPlanner
    {
        private readonly IDBProvider _db;
        private readonly IUserRepository _userRepo;

        public StagePreloadPlanner(IDBProvider db, IUserRepository userRepo)
        {
            _db = db;
            _userRepo = userRepo;
        }

        public IEnumerable<PreloadEntry> Plan(int stageId)
        {
            var entries = new Dictionary<string, int>();

            if (!_db.Stages.TryFind(stageId, out var stageEntity))
                return Array.Empty<PreloadEntry>();

            CollectWaveUnits(stageEntity.WaveId, entries);
            CollectCastle(entries);
            CollectAllyCharacters(entries);

            return ToEntries(entries);
        }

        private void CollectWaveUnits(int waveGroupId, Dictionary<string, int> entries)
        {
            var waves = _db.Waves.Map(waveGroupId);
            if (waves == null || waves.Length == 0)
                return;

            // Accumulate total spawn count per unit ID across all waves in the group.
            var unitTotals = new Dictionary<int, int>();
            foreach (var wave in waves)
            {
                if (wave.ids == null)
                    continue;

                for (int i = 0; i < wave.ids.Length; i++)
                {
                    int unitId = wave.ids[i];
                    int count = wave.counts != null && i < wave.counts.Length ? wave.counts[i] : 1;

                    if (!unitTotals.ContainsKey(unitId))
                        unitTotals[unitId] = 0;
                    unitTotals[unitId] += count;
                }
            }

            foreach (var pair in unitTotals)
            {
                int unitId = pair.Key;
                int count = pair.Value;

                if (!_db.Units.TryFind(unitId, out var unitEntity))
                    continue;

                AddEntry(entries, unitEntity.prefab, count);
                AddEntry(entries, unitEntity.projectile, count);

                CollectSkillEffects(unitEntity.skill, count, entries);
            }
        }

        private void CollectSkillEffects(int skillId, int count, Dictionary<string, int> entries)
        {
            if (skillId <= 0)
                return;

            if (!_db.Skills.TryFind(skillId, out var skillEntity))
                return;

            if (skillEntity.effects == null)
                return;

            foreach (var effect in skillEntity.effects)
            {
                AddEntry(entries, effect, count);
            }
        }

        private void CollectCastle(Dictionary<string, int> entries)
        {
            AddEntry(entries, UnitFactory.DEFAULT_CASTLE_PREFAB, 1);

            int castleLv = _userRepo?.GameRecord?.CastleLv?.Value ?? 1;
            if (!_db.Castles.TryFind(castleLv, out var castleEntity))
                return;

            if (castleEntity.skillEffects != null)
            {
                foreach (var effect in castleEntity.skillEffects)
                {
                    AddEntry(entries, effect, 1);
                }
            }

            if (castleEntity.unitEffects != null)
            {
                foreach (var effect in castleEntity.unitEffects)
                {
                    AddEntry(entries, effect, 1);
                }
            }
        }

        private void CollectAllyCharacters(Dictionary<string, int> entries)
        {
            var userRepo = _userRepo;
            if (userRepo == null)
                return;

            var unitsInField = userRepo.UnitsInField();
            foreach (var unitInfo in unitsInField)
            {
                if (!_db.Units.TryFind(unitInfo.id, out var unitEntity))
                    continue;

                AddEntry(entries, unitEntity.prefab, 1);
                AddEntry(entries, unitEntity.projectile, 1);

                CollectSkillEffects(unitEntity.skill, 1, entries);
            }
        }

        private static void AddEntry(Dictionary<string, int> entries, string path, int count)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!entries.ContainsKey(path))
                entries[path] = 0;
            entries[path] += count;
        }

        private static IEnumerable<PreloadEntry> ToEntries(Dictionary<string, int> entries)
        {
            var result = new List<PreloadEntry>(entries.Count);
            foreach (var pair in entries)
            {
                result.Add(new PreloadEntry(pair.Key, pair.Value));
            }
            return result;
        }
    }
}
