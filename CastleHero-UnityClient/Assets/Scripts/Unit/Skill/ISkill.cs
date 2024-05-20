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
        public void Run();
        
        public ReactiveProperty<SkillState> State { get; }
    }
}