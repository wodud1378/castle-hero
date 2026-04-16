using System.Collections.Generic;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;

namespace CastleHero.GamePlay.Unit.Skill.Components
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

        public bool HasTargets() => _finder.Update(_owner.Position);
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