using System;
using CastleHero.Data.DB;
using CastleHero.Data.Model;

namespace CastleHero.Network.DB
{
    public class DBCollections : IDBProvider
    {
        public StageDB Stages { get; set; }
        public WaveDB Waves { get; set; }
        public DungeonDB Dungeons { get; set; }
        public DungeonRewardDB DungeonRewards { get; set; }
        public ElementDB Elements { get; set; }
        public CastleDB Castles { get; set; }
        public UnitDB Units { get; set; }
        public UnitLevelDB Levels { get; set; }
        public UnitRateDB Rates { get; set; }
        public UnitBalanceDB Balances { get; set; }
        public SkillDB Skills { get; set; }
        public SummonDB Summons { get; set; }
        public SummonGroupDB SummonGroups { get; set; }
        public ItemDB Items { get; set; }
        public EquipItemStatDB EquipmentStats { get; set; }
        public ShopDB Shop { get; set; }
        public ShopItemGroupDB ShopGroup { get; set; }

        public bool TryLoadGameEntity(GameType type, int id, out IGameEntity entity)
        {
            entity = default;
            switch (type)
            {
                case GameType.Stage:
                    if (!Stages.TryFind(id, out var s))
                        return false;

                    entity = s;
                    return true;
                case GameType.Dungeon:
                    if (!Dungeons.TryFind(id, out var d))
                        return false;

                    entity = d;
                    return true;
                default:
                    return false;
            }
        }

        public bool TryLoadNextGameEntity(GameType type, int id, out IGameEntity entity)
        {
            entity = default;

            switch (type)
            {
                case GameType.Stage:
                    if (!Stages.TryFindIndex(id, out int sIndex))
                        return false;

                    if (!Stages.TryIndexOf(sIndex + 1, out var s))
                        return false;

                    entity = s;
                    return true;
                case GameType.Dungeon:
                    if (!Dungeons.TryFindIndex(id, out int dIndex))
                        return false;

                    if (!Dungeons.TryIndexOf(dIndex + 1, out var d))
                        return false;

                    entity = d;
                    return true;
                default:
                    return false;
            }
        }
    }
}