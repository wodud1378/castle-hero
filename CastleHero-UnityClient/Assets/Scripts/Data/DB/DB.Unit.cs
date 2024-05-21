using System.Collections.Generic;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Castle_Table")]
    public class CastleDB : DB<CastleEntity> { }
    
    [DB("Character_Table")]
    public class UnitDB : DB<UnitEntity>
    {
        public readonly Dictionary<int, float> sizeCache = new();

        public void CacheUnitSizes()
        {
            foreach (var entity in entities)
            {
                sizeCache.TryAdd(entity.Id, entity.size);
            }
        }
    }
    
    [DB("Character_Upgrade_Table")]
    public class UnitLevelDB : DB<UnitLevelEntity> { }
    
    [DB("Skill_Table")]
    public class SkillDB : DB<SkillEntity> { }
}