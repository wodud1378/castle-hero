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
    public class UnitBalanceDB : DB<UnitBalanceEntity>
    {
        protected override void Convert(object from, ref UnitBalanceEntity to)
        {
            base.Convert(from, ref to);

            var options = new List<int[]>();
            if(to.rate1Options != null) options.Add(to.rate1Options);
            if(to.rate2Options != null) options.Add(to.rate2Options);
            if(to.rate3Options != null) options.Add(to.rate3Options);
            if(to.rate4Options != null) options.Add(to.rate4Options);
            if(to.rate5Options != null) options.Add(to.rate5Options);

            var values = new List<float[]>();
            if(to.rate1OptionValues != null) values.Add(to.rate1OptionValues);
            if(to.rate2OptionValues != null) values.Add(to.rate2OptionValues);
            if(to.rate3OptionValues != null) values.Add(to.rate3OptionValues);
            if(to.rate4OptionValues != null) values.Add(to.rate4OptionValues);
            if(to.rate5OptionValues != null) values.Add(to.rate5OptionValues);
            
            to.rateOptions = options.ToArray();
            to.rateValues = values.ToArray();
        }
    }
    
    [DB("Rate_Table")]
    public class UnitRateDB : DB<UnitRateEntity> { }
    
    [DB("Level_Table")]
    public class UnitLevelDB : DB<UnitLevelEntity> { }

    
    [DB("Skill_Table")]
    public class SkillDB : DB<SkillEntity> { }
}