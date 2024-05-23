using System;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;

namespace RGLabs.Unit.Skill.Components.Factory
{
    public class CycleFactory
    {
        public ICycle GetCycle(ICycle.Option option, UnitBehaviour owner, SkillEntity data)
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