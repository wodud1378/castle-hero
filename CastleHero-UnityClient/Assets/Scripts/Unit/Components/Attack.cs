using System.Collections.Generic;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;

namespace RGLabs.Unit.Components
{
    public class Attack
    {
        private readonly UnitBehaviour _unit;
        
        public Attack(UnitBehaviour unit)
        {
            _unit = unit;
        }
        
        public void Process(List<UnitBehaviour> targets)
        {
            foreach (var target in targets)
            {
                if (!target.IsValid())
                    continue;

                var status = _unit.status;
                var data = new AtkEvent
                {
                    from = _unit,
                    to = target,
                    amount = status.atk,
                    critical = status.critical,
                    criticalMul = status.criticalAtk
                };

                data.Publish();
            }
        }
    }
}