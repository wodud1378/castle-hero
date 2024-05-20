using System.Linq;
using RGLabs.Common;
using RGLabs.Common.Pattern;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Unit.Finding;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class Skill10001 : ISkill
    {
        public ReactiveProperty<SkillState> State { get; } = new();
        
        private readonly UnitBehaviour _owner;
        private readonly SkillEntity _data;
        private readonly FindUnits _finder;
        private float _currentTime;
        private bool _hasTargets;

        public Skill10001(UnitBehaviour owner, SkillEntity data)
        {
            _owner = owner;
            _data = data;
            _finder = new FindUnits(new Collider2D[Constants.BufferSize], 10);
            _currentTime = data.coolTime;
            owner
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Subscribe(Update)
                .AddTo(owner);

            var animationEvent = _owner.Core.animationEvent;
            animationEvent.OnExecuteSkillEvent += Execute;
            animationEvent.OnReleaseSkillEvent += Release;
        }
        
        public void Run()
        {
            State.Value = SkillState.Running;
            _hasTargets = _finder.Update(_owner.position);

            var renderController = _owner.Core.renderController;
            renderController.SetAnimation(UnitCore.AnimationsHash[UnitCore.States.Skill]);
        }

        private void Execute()
        {
            if(_hasTargets)
                ExecuteOnTargets();
            
            SetInvincible();
        }

        private void Release() => State.Value = SkillState.Wait;

        private void Update(float deltaTime)
        {
            if (State.Value != SkillState.Wait)
                return;
            
            _currentTime = Mathf.Clamp(_currentTime - deltaTime, 0f, float.MaxValue);
            
            if (_currentTime <= 0f)
                State.Value = SkillState.Ready;
        }

        private void ExecuteOnTargets()
        {
            var status = _owner.status;
            var healAmount = status[(Status.Type)_data.stats[0]] * _data.values[0];
            var atkAmount = status[(Status.Type)_data.stats[1]] * _data.values[1];
            foreach (var unit in _finder.Found)
            {
                if (unit.IsAlley(_owner))
                {
                    new HealEvent
                    {
                        From = _owner,
                        To = unit,
                        Amount = healAmount
                    }.Publish();
                }
                else
                {
                    new AtkEvent
                    {
                        From = _owner,
                        To = unit,
                        Amount = atkAmount
                    }.Publish();
                }
            }
        }

        private void SetInvincible()
        {
            var characters = Storage.inGameRepository.characters.Value
                .Where(x => x.IsValid() && x.Data.team == _owner.Data.team);

            foreach (var character in characters)
            {
                character.Core.SetInvincible(5f);
            }
        }
    }
}