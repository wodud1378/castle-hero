using System.Linq;
using RGLabs.Common;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class Skill10021 : ISkill
    {
        public readonly UnitBehaviour owner;
        public readonly SkillEntity data;

        private readonly FindUnits _finder;
        private bool _recovered;

        public Skill10021(UnitBehaviour owner, SkillEntity entity)
        {
            _finder = new FindUnits(new Collider2D[Constants.BufferSize], 10);
        }

        public bool IsReady()
        {
            return true;
        }
        
        public bool TryExecute()
        {
            bool casted = false;
            if (_finder.Update(owner.position))
            {
                ExecuteOnTargets();
                casted = true;
            }

            if (!_recovered && TryRecovery())
            {
                _recovered = true;
                casted = true;
            }

            return casted;
        }

        private void ExecuteOnTargets()
        {
            var status = owner.status;
            var healAmount = status[(Status.Type)data.stats[0]] * data.values[0];
            var atkAmount = status[(Status.Type)data.stats[1]] * data.values[1];
            foreach (var unit in _finder.Found)
            {
                if (unit.IsAlley(owner))
                {
                    new HealEvent
                    {
                        From = owner,
                        To = unit,
                        Amount = healAmount
                    }.Publish();
                }
                else
                {
                    new AtkEvent
                    {
                        From = owner,
                        To = unit,
                        Amount = atkAmount
                    }.Publish();
                }
            }
        }

        private bool TryRecovery()
        {
            var recovers =
                Storage.inGameRepository.recovers.Where(x => x.behaviour.Data.team == owner.Data.team);

            bool success = false;
            foreach (var recover in recovers)
            {
                success = true;
                recover.leftTime = 0f;
            }

            return success;
        }
    }
}