using System.Linq;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Skill.Components;
using RGLabs.Utility;
using UniRx;

namespace RGLabs.Unit.Skill.Character
{
    public class Skill10021 : ISkill
    {
        public UnitBehaviour Owner { get; set; }
        public SkillEntity Data { get; set; }
        public IBound Bound { get; set; }
        public ITargeting Targeting { get; set; }
        
        public ReactiveProperty<SkillState> State { get; }
   
        public void Run()
        {
            BuffsOnGroup();
            AttackEnemies();
        }

        public void Init()
        {
        }

        private void BuffsOnGroup()
        {
            var characters = Storage.inGameRepository.characters.Value
                .Where(x=> x.IsValid());

            int group = Data.groups[0];
            var status = Owner.status;
            float increaseTime = Data.duration;
            float increaseValue = status[(Status.Type)Data.stats[0]] * Data.values[0]; 
            
            foreach (var character in characters)
            {
                if (character.Data.team != group)
                    continue;
                
                character.status.speed.fixedAdjust.Increase(increaseValue, increaseTime);
            }
        }

        private void AttackEnemies()
        {
            if (!Targeting.HasTargets())
                return;

            float atkAmount = Owner.status[(Status.Type)Data.stats[1]] * Data.values[1]; 
            Targeting.Targets.ForEach(x=> new AtkEvent
            {
                Type = DamageType.Normal,
                From = Owner,
                To = x,
                Amount = atkAmount
            }.Publish());
        }
    }
}