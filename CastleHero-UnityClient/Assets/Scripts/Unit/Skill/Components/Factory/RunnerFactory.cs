using System;
using RGLabs.Unit.Behaviours;

namespace RGLabs.Unit.Skill.Components.Factory
{
    public class RunnerFactory
    {
        public IRunner GetRunner(IRunner.Option option, UnitBehaviour owner)
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