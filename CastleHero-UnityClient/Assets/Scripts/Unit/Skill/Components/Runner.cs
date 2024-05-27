using System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;

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

        private readonly RenderController _renderController;
        
        public AnimationRunner(UnitBehaviour owner)
        {
            _renderController = owner.Core.renderController;
            
            var animationEvents = owner.Core.animationEvent;
            
            animationEvents.OnExecuteSkillEvent -= OnExecute;
            animationEvents.OnExecuteSkillEvent += OnExecute;
            
            animationEvents.OnReleaseSkillEvent -= OnRelease;
            animationEvents.OnReleaseSkillEvent += OnRelease;
        }

        private void OnExecute()
        {
            OnExecuteEvent?.Invoke();
        }

        private void OnRelease()
        {
            IsRunning = false;
            
            OnReleaseEvent?.Invoke();
        }
        
        public void Run()
        {
            _renderController.SetAnimation(UnitCore.AnimationsHash[UnitCore.States.Skill]);
            IsRunning = true;
        }
    }
}