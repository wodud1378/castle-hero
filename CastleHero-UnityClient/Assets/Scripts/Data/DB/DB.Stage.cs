using System;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Stage_Table")]
    public class StageDB : DB<StageEntity> { }
    
    [DB("Mob_Wave_Table")]
    public class WaveDB : DB<WaveEntity>
    {
        public WaveEntity[] Map(int groupId) => Array.FindAll(entities, (x) => x.groupId == groupId);
    }
}