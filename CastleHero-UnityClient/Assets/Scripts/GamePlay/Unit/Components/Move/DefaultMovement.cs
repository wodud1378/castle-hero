using PolyNav;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.Utility;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components.Move
{
    public class DefaultMovement : IMovement
    {
        public Finder Finder { get; }

        public Vector2 Position
        {
            get => _agent.position;
            set => _agent.position = value;
        }

        private readonly UnitBehaviour _owner;
        private readonly PolyNavAgent _agent;

        public bool Enabled
        {
            get => _agent.enabled;
            set => _agent.enabled = value;
        }
        
        public Vector2 Default { get; set; }
        
        public UnitBehaviour CurrentTarget { get; private set; }

        public DefaultMovement(UnitBehaviour owner, Finder finder, PolyNavAgent navAgent)
        {
            _owner = owner;
            _agent = navAgent;
            
            Finder = finder;
        }

        public bool TryMoveToTarget()
        {
            var selected = Select();
            if (selected == null)
                return false;

            if (CurrentTarget != selected)
            {
                if (CurrentTarget.IsValid())
                    CurrentTarget.OnDead -= OnUnitDead;

                selected.OnDead -= OnUnitDead;
                selected.OnDead += OnUnitDead;
                CurrentTarget = selected;
            }
            
            StartMove(CurrentTarget.Position);
            return true;
        }

        public void SetSpeed(float value)
        {
            _agent.maxSpeed = value;
        }
        
        private UnitBehaviour Select()
        {
            var overriden = Finder.Override;
            if (overriden.IsValid())
                return overriden;

            float rangeStat = _owner.Status.moveRange;
            if (CurrentTarget.IsValid())
            {
                float distance = (CurrentTarget.Position - _owner.Position).sqrMagnitude;
                float range = Mathf.Pow(rangeStat, 2);

                if (distance <= range)
                    return CurrentTarget;
            }
            
            Finder.detection.SetRange(rangeStat, rangeStat);
            return !Finder.Update(_owner.Position)
                ? null
                : Finder.Found.Count > 0
                    ? Finder.Found[0]
                    : null;
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