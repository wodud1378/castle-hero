using PolyNav;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using UnityEngine;

namespace RGLabs.Unit.Components.Move
{
    public class DefaultMovement : IMovement
    {
        private readonly UnitBehaviour _owner;
        private readonly FindMoveTarget _finder;
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
            set
            {
                _currentTarget = value;

                if (_currentTarget != null)
                    _currentTarget.OnDead += OnUnitDead;
            }
        }

        public FindMoveTarget Finder { get; }

        private UnitBehaviour _currentTarget;

        public DefaultMovement(UnitBehaviour owner, FindMoveTarget finder)
        {
            _owner = owner;
            _agent = owner.Core.navAgent;
            
            Finder = finder;
        }

        public bool TryMoveToTarget()
        {
            Finder.detection.SetRange(_owner.status.moveRange);
            
            CurrentTarget = null;
            if (!_finder.Update(_agent.position))
                return false;

            CurrentTarget = _finder.Found[0];
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