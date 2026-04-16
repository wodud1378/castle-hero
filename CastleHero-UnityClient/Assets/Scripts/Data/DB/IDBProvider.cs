using CastleHero.Data.DB;
using CastleHero.Data.Model;

namespace CastleHero.Data.DB
{
    public interface IDBProvider
    {
        StageDB Stages { get; }
        WaveDB Waves { get; }
        DungeonDB Dungeons { get; }
        DungeonRewardDB DungeonRewards { get; }
        ElementDB Elements { get; }
        CastleDB Castles { get; }
        UnitDB Units { get; }
        UnitLevelDB Levels { get; }
        UnitRateDB Rates { get; }
        UnitBalanceDB Balances { get; }
        SkillDB Skills { get; }
        SummonDB Summons { get; }
        SummonGroupDB SummonGroups { get; }
        ItemDB Items { get; }
        EquipItemStatDB EquipmentStats { get; }
        ShopDB Shop { get; }
        ShopItemGroupDB ShopGroup { get; }

        bool TryLoadGameEntity(GameType type, int id, out IGameEntity entity);
        bool TryLoadNextGameEntity(GameType type, int id, out IGameEntity entity);
    }
}
