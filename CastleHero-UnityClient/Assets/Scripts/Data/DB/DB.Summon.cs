using System;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("summon")]
    public class SummonDB : DB<SummonEntity> { }

    [DB("group")]
    public class SummonGroupDB : DB<SummonGroupEntity>
    {
        public SummonGroupEntity[] Map(int groupId) => Array.FindAll(entities, (x) => x.groupId == groupId);
    }
}