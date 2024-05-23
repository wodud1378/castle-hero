using System;
using RGLabs.Unit.Behaviours;

namespace RGLabs.Unit.Skill.Components
{
    public interface IRunner
    {
        public enum Option
        {
            Animation
        }
        
        public event Action OnExecuteEvent;
        public event Action OnReleaseEvent;
        
        public bool IsRunning { get; }
 
        public void Run();
    }

    public class AnimationRunner : IRunner
    {
        public event Action OnExecuteEvent;
        public event Action OnReleaseEvent;
        
        public bool IsRunning { get; private set; }

        public AnimationRunner(AnimationEvents animationEvents)
        {
            animationEvents.OnExecuteSkillEvent -= OnExecute;
            animationEvents.OnExecuteSkillEvent += OnExecute;
            
            animationEvents.OnReleaseSkillEvent -= OnRelease;
            animationEvents.OnReleaseSkillEvent += OnRelease;
        }

        private void OnExecute() => OnExecuteEvent?.Invoke();

        private void OnRelease()
        {
            IsRunning = false;
            
            OnReleaseEvent?.Invoke();
        }
        
        public void Run() => IsRunning = true;
    }
}