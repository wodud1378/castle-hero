using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.System;
using RGLabs.InGame.Utility;

namespace RGLabs.InGame.Unit
{
    public class Attack
    {
        private readonly DataStream<AdjustHpRequest> _stream = InGameContext.streams.atk;

        public void Process(UnitBehaviour root, List<UnitBehaviour> targets)
        {
            foreach (var target in targets)
            {
                if (!target.IsValid())
                    continue;
                
                _stream.Emit(new AdjustHpRequest
                {
                    from = root,
                    to = target,
                    amount = root.Status.atk,
                    critical = root.Status.critical,
                    criticalMul = root.Status.criticalAtk
                });
            }
        }
    }
}