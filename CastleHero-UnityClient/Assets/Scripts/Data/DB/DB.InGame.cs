using System;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("stage")]
    public class StageDB : DB<StageEntity> { }
    
    [DB("wave")]
    public class WaveDB : DB<WaveEntity>
    {
        public WaveEntity[] Map(int groupId) => Array.FindAll(entities, (x) => x.groupId == groupId);
    }

    [DB("dungeon")]
    public class DungeonDB : DB<DungeonEntity> { }
    
    [DB("dungeon_reward")]
    public class DungeonRewardDB : DB<DungeonRewardEntity> { }

    [DB("elements")]

    public class ElementDB : DB<ElementEntity> { }
}