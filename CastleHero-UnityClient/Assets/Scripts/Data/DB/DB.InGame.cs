using System;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Stage_Table", "stage")]
    public class StageDB : DB<StageEntity> { }
    
    [DB("Mob_Wave_Table", "wave")]
    public class WaveDB : DB<WaveEntity>
    {
        public WaveEntity[] Map(int groupId) => Array.FindAll(entities, (x) => x.groupId == groupId);
    }

    [DB("Dg_Table", "dungeon")]
    public class DungeonDB : DB<DungeonEntity>
    {
    }
    
    
    [DB("Dg_Rwd_Item_Grp_Table", "dungeon_reward")]
    public class DungeonRewardDB : DB<DungeonRewardEntity>
    {
    }
}