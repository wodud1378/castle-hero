using System;
using PolyNav;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.Utility;
using UniRx;
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

        private readonly UnitActor _owner;
        private readonly PolyNavAgent _agent;

        public bool Enabled
        {
            get => _agent.enabled;
            set => _agent.enabled = value;
        }
        
        public Vector2 Default { get; set; }

        public UnitActor CurrentTarget { get; private set; }

        private IDisposable _targetDeadSub;

        public DefaultMovement(UnitActor owner, Finder finder, PolyNavAgent navAgent)
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
                _targetDeadSub?.Dispose();
                _targetDeadSub = selected.OnDead.Take(1).Subscribe(OnUnitDead);
                CurrentTarget = selected;
            }

            StartMove(CurrentTarget.Position);
            return true;
        }

        public void SetSpeed(float value)
        {
            _agent.maxSpeed = value;
        }
        
        private UnitActor Select()
        {
            return UnitHelper.SelectTarget(Finder, _owner, CurrentTarget, _owner.Status.moveRange);
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

        private void OnUnitDead(UnitActor unit)
        {
            if(unit == CurrentTarget)
                Stop();
        }
    }
}