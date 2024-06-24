using System;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Summon_Table")]
    public class SummonDB : DB<SummonEntity> { }

    [DB("Summon_Grp_Table")]
    public class SummonGroupDB : DB<SummonGroupEntity>
    {
        public SummonGroupEntity[] Map(int groupId) => Array.FindAll(entities, (x) => x.groupId == groupId);
    }
}