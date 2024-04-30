using System.Collections.Generic;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;

namespace RGLabs.Unit
{
    public class Attack
    {
        public void Process(UnitBehaviour root, List<UnitBehaviour> targets)
        {
            foreach (var target in targets)
            {
                if (!target.IsValid())
                    continue;

                var data = new AtkEvent
                {
                    from = root,
                    to = target,
                    amount = root.status.atk,
                    critical = root.status.critical,
                    criticalMul = root.status.criticalAtk
                };

                data.Publish();
            }
        }
    }
}