using System;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Skill.Components;

namespace RGLabs.Unit.Skill
{
    public class SkillBuilder
    {
        public readonly UnitBehaviour owner;
        
        private readonly ISkill _skill;

        public SkillBuilder(UnitBehaviour owner, int id, int lv)
        {
            this.owner = owner;
            
            var db = Context.currentBehaviour.db.skills;
            int skillId = (id * 10) + lv;
            if (!db.TryFind(skillId, out var entity))
                return;
            
            var type = Type.GetType($"RGLabs.Unit.Skill.Skill{id}");
            if (type == null)
                return;
            
            _skill = (ISkill)Activator.CreateInstance(type);
            _skill.Owner = owner;
            _skill.Data = entity;
        }
        
        public SkillBuilder SetTargeting(ITargeting targeting)
        {
            if (_skill != null)
                _skill.Targeting = targeting;

            return this;
        }
        
        public SkillBuilder SetBound(IBound bound)
        {
            if (_skill != null)
                _skill.Bound = bound;

            return this;
        }

        public ISkill Build()
        {
            if (_skill == null)
                return null;
            
            _skill.Init();
            return _skill;
        }
    }
}