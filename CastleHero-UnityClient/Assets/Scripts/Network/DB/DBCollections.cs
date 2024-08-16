using System;
using RGLabs.Data.DB;
using RGLabs.Data.Model;

namespace RGLabs.Network.DB
{
    public class DBCollections
    {
        public StageDB stages;
        public WaveDB waves;
        public DungeonDB dungeons;
        public DungeonRewardDB dungeonRewards;
        public CastleDB castles;
        public UnitDB units;
        public UnitLevelDB levels;
        public UnitRateDB rates;
        public UnitBalanceDB balances;
        public SkillDB skills;
        public SummonDB summons;
        public SummonGroupDB summonGroups;
        public ItemDB items;
        public ShopDB shop;
        public ShopItemGroupDB shopGroup;

        public bool TryLoadGameEntity(GameType type, int id, out IGameEntity entity)
        {
            entity = default;
            switch (type)
            {
                case GameType.Stage:
                    if (!stages.TryFind(id, out var s))
                        return false;

                    entity = s;
                    return true;
                case GameType.Dungeon:
                    if (!dungeons.TryFind(id, out var d))
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
                    if (!stages.TryFindIndex(id, out int sIndex))
                        return false;

                    if (!stages.TryIndexOf(sIndex + 1, out var s))
                        return false;

                    entity = s;
                    return true;
                case GameType.Dungeon:
                    if (!dungeons.TryFindIndex(id, out int dIndex))
                        return false;

                    if (!dungeons.TryIndexOf(dIndex + 1, out var d))
                        return false;

                    entity = d;
                    return true;
                default:
                    return false;
            }
        }
    }
}