using System;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Skill.Components
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

        private readonly IUnitRenderer _renderer;

        public AnimationRunner(UnitBehaviour owner)
        {
            _renderer = owner.Core.Renderer;

            var animationEvents = owner.Core.AnimationEvent;
            
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
            _renderer.SetAnimation(UnitCore.AnimationsHash[UnitCore.States.Skill]);
            IsRunning = true;
        }
    }
}