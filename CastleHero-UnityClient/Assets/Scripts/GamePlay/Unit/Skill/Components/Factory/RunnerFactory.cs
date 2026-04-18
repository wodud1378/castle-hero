using System;
using CastleHero.GamePlay.Unit.Behaviours;

namespace CastleHero.GamePlay.Unit.Skill.Components.Factory
{
    public class RunnerFactory
    {
        public IRunner GetRunner(IRunner.Option option, UnitActor owner)
        {
            switch (option)
            {
                case IRunner.Option.Animation:
                    return new AnimationRunner(owner);
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }
    }
}