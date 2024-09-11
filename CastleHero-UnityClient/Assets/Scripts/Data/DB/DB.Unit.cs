using System.Collections.Generic;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("castle")]
    public class CastleDB : DB<CastleEntity>
    {
        public int MaxLv => this[^1].Id;
    }
    
    [DB("unit")]
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

    [DB("balance")]
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

    [DB("rate")]
    public class UnitRateDB : DB<UnitRateEntity>
    {
        public int MaxRate => this[^1].Id + 1;
    }

    [DB("level")]
    public class UnitLevelDB : DB<UnitLevelEntity>
    {
        public int MaxLv => this[^1].Id + 1;
    }

    
    [DB("skill")]
    public class SkillDB : DB<SkillEntity> { }
}