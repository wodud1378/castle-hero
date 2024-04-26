using System;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.System;

namespace RGLabs.InGame.Unit
{
    public class Recovery : IUpdate, IDisposable
    {
        private readonly UnitBehaviour _unit;
        private readonly Ability _ability;

        private float _currentTime;
        
        public Recovery(UnitBehaviour unit)
        {
            _unit = unit;
            _unit.OnDead += OnUnitDead;
            _unit.autoRelease = false;
            _ability = _unit.Status.recovery;
        }

        public void Dispose()
        {
            _unit.OnDead -= OnUnitDead;
        }

        private void OnUnitDead(UnitBehaviour unit)
        {
            
        }

        public void ProcessUpdate(float deltaTime)
        {
            if (_unit.state.Value != UnitBehaviour.States.Dead)
                return;

            _currentTime += deltaTime;
            if (_currentTime < _ability)
                return;
            
            _unit.Init(_unit.Data);
        }
    }
}