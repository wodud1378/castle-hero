using System;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Behaviours;

namespace CastleHero.GamePlay.Unit.Skill.Components.Factory
{
    public class CycleFactory
    {
        public ICycle GetCycle(ICycle.Option option, UnitActor owner, SkillEntity data)
        {
            switch (option)
            {
                case ICycle.Option.CoolTime:
                    return new CoolTime(owner, data.coolTime);
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }
    }
}