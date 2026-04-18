using System;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using UniRx;
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

        public AnimationRunner(UnitActor owner)
        {
            _renderer = owner.Combat.Renderer;

            var animationEvents = owner.Combat.AnimationEvent;

            animationEvents.OnExecuteSkill.Subscribe(_ => OnExecute()).AddTo(owner);
            animationEvents.OnReleaseSkill.Subscribe(_ => OnRelease()).AddTo(owner);
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
            _renderer.SetAnimation(CombatController.AnimationsHash[UnitState.States.Skill]);
            IsRunning = true;
        }
    }
}