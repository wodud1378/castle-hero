using System.Collections.Generic;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;

namespace RGLabs.Unit.Skill.Components
{
    public interface ITargeting
    {
        public List<UnitBehaviour> Targets { get; }

        public bool HasTargets();
    }

    public class FindTargets : ITargeting
    {
        private readonly UnitBehaviour _owner;
        private readonly Finder _finder;

        public List<UnitBehaviour> Targets => _finder.Found;

        public FindTargets(UnitBehaviour owner, Finder finder)
        {
            _owner = owner;
            _finder = finder;
        }

        public bool HasTargets() => _finder.Update(_owner.position);
    }

    public class SelfTarget : ITargeting
    {
        public List<UnitBehaviour> Targets { get; } = new();

        public SelfTarget(UnitBehaviour owner)
        {
            Targets.Add(owner);
        }

        public bool HasTargets() => true;
    }
}