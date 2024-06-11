using PolyNav;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Components.Move
{
    public class DefaultMovement : IMovement
    {
        public Finder Finder { get; }
        
        private readonly UnitBehaviour _owner;
        private readonly PolyNavAgent _agent;

        public bool Enabled
        {
            get => _agent.enabled;
            set => _agent.enabled = value;
        }
        
        public Vector2 Default { get; set; }

        public UnitBehaviour CurrentTarget
        {
            get => _currentTarget;
            private set
            {
                _currentTarget = value;
                
                if (_currentTarget.IsValid())
                {
                    _currentTarget.OnDead -= OnUnitDead;
                    _currentTarget.OnDead += OnUnitDead;
                }
            }
        }

        private UnitBehaviour _currentTarget;

        public DefaultMovement(UnitBehaviour owner, Finder finder, PolyNavAgent navAgent)
        {
            _owner = owner;
            _agent = navAgent;
            
            Finder = finder;
        }

        public bool TryMoveToTarget()
        {
            float range = _owner.status.moveRange;
            Finder.detection.SetRange(range, range);

            if (!Finder.Update(_owner.position))
                return false;

            CurrentTarget = !_currentTarget.IsValid() ? Finder.Found[0] :
                Finder.Found.Contains(_currentTarget) ? _currentTarget : Finder.Found[0];
            
            StartMove(CurrentTarget.position);
            return true;
        }

        public bool TryMoveToDefault()
        {
            if ((Default - _agent.position).magnitude < _agent.stoppingDistance)
                return false;
            
            StartMove(Default);
            return true;
        }
        
        public void Stop()
        {
            _agent.Stop();
        }

        private void StartMove(Vector2 position)
        {
            _agent.SetDestination(position);
        }

        private void OnUnitDead(UnitBehaviour unit)
        {
            if(unit == CurrentTarget)
                Stop();
            
            unit.OnDead -= OnUnitDead;
        }
    }
}