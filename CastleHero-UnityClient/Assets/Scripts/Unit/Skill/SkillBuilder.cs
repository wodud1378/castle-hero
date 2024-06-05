using System;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Unit.Skill.Components;
using RGLabs.Unit.Skill.Components.Factory;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class SkillBuilder
    {
        private readonly TargetingFactory _targetingFactory = new();
        private readonly BoundFactory _boundFactory = new();
        private readonly CycleFactory _cycleFactory = new();
        private readonly RunnerFactory _runnerFactory = new();

        private ISkill _skill;
        
        public bool StartBuild(UnitBehaviour owner, int id, int lv)
        {
            var db = Storage.db.skills;
            int skillId = (id * 10) + lv;
            if (!db.TryFind(skillId, out var entity))
            {
                Debug.LogWarning($"Skill DB not contains {skillId}.");
                return false;
            }

            var type = Type.GetType($"RGLabs.Unit.Skill.Skill{id}");
            if (type == null)
            {
                Debug.LogWarning($"Skill{id} class not found.");
                return false;
            }

            _skill = (ISkill)Activator.CreateInstance(type);
            _skill.Owner = owner;
            _skill.Data = entity;
            return true;
        }

        public SkillBuilder SetTargeting(Targeting option, int maxTarget)
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            _skill.Targeting = _targetingFactory.GetTargeting(option, _skill.Owner, maxTarget);
            return this;
        }

        public SkillBuilder SetBound(IDetection.Option shape, Targeting targeting, int maxTarget)
        {
            if (_skill == null)
                throw new Exception($"skill is null");

            _skill.Bound = _boundFactory.GetBound(shape, targeting, _skill.Owner, maxTarget, _skill.Data.x, _skill.Data.y);
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