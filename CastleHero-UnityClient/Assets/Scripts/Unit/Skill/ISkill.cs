using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Skill.Components;
using UniRx;

namespace RGLabs.Unit.Skill
{
    public enum SkillState
    {
        Wait,
        Ready,
        Running,
    }
    
    public interface ISkill
    {
        public UnitBehaviour Owner { get; set; }
        public SkillEntity Data { get; set; }
        public IBound Bound { get; set; }
        public ITargeting Targeting { get; set; }
        
        public ReactiveProperty<SkillState> State { get; }
        
        public void Run();

        public void Init();
    }
}