using System;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Unit.Skill.Components;
using RGLabs.Unit.Skill.Components.Factory;

namespace RGLabs.Unit.Skill
{
    public class SkillBuilder
    {
        private readonly TargetingFactory _targetingFactory = new();
        private readonly BoundFactory _boundFactory = new();
        private readonly CycleFactory _cycleFactory = new();
        private readonly RunnerFactory _runnerFactory = new();

        private ISkill _skill;
        
        public SkillBuilder StartBuild(UnitBehaviour owner, int id, int lv)
        {
            var db = Context.currentBehaviour.db.skills;
            int skillId = (id * 10) + lv;
            if (!db.TryFind(skillId, out var entity))
                throw new Exception($"Skill DB not contains {skillId}.");

            var type = Type.GetType($"RGLabs.Unit.Skill.Skill{id}");
            if (type == null)
                throw new Exception($"Skill{id} class not found.");

            _skill = (ISkill)Activator.CreateInstance(type);
            _skill.Owner = owner;
            _skill.Data = entity;
            return this;
        }

        public SkillBuilder SetTargeting(Targeting option, int maxTarget)
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            _skill.Targeting = _targetingFactory.GetTargeting(option, _skill.Owner, maxTarget);
            return this;
        }

        public SkillBuilder SetCircleBound(Targeting targeting, int maxTarget)
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            return SetBound(IDetection.Option.Circle, targeting, maxTarget, _skill.Data.range);
        }
        
        public SkillBuilder SetArcBound(Targeting targeting, int maxTarget)
        {
            if (_skill == null)
                throw new Exception($"skill is null");
            
            return SetBound(IDetection.Option.Arc, targeting, maxTarget, _skill.Data.range);
        }

        public SkillBuilder SetBoxBound(Targeting targeting, int maxTarget, float x, float angle)
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            var bound = _boundFactory.GetBound(IDetection.Option.Box, targeting, _skill.Owner, maxTarget, x, _skill.Data.range);
            bound.Finder.detection.Angle = angle;
            return this;
        }

        private SkillBuilder SetBound(IDetection.Option shape, Targeting targeting, int maxTarget,
            float x, float y = -1)
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            _skill.Bound = _boundFactory.GetBound(shape, targeting, _skill.Owner, maxTarget, x, y);
            return this;
        }

        public SkillBuilder SetCycle(ICycle.Option option)
        {
            if(_skill == null)
                throw new Exception($"skill is null");

            _skill.Cycle = _cycleFactory.GetCycle(option, _skill.Owner, _skill.Data);
            return this;
        }

        public SkillBuilder SetRunner(IRunner.Option option)
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            _skill.Runner = _runnerFactory.GetRunner(option, _skill.Owner);
            return this;
        }

        public ISkill BuildActiveSkill()
        {
            SetCycle(ICycle.Option.CoolTime);
            SetRunner(IRunner.Option.Animation);

            return Build();
        }
        
        public ISkill Build()
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            if (_skill.Cycle == null || _skill.Runner == null)
                throw new Exception($"skill has no cycle or runner");

            _skill.Init();
            return _skill;
        }
    }
}