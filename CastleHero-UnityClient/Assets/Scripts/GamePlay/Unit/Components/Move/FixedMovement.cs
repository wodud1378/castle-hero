using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components.Move
{
    public class FixedMovement : IMovement
    {
        public bool Enabled { get; set; }
        
        public Vector2 Default { get; set; }
        public UnitActor CurrentTarget { get; set; }
        public Finder Finder => null;

        public Vector2 Position
        {
            get => Default;
            set { }
        }

        public bool TryMoveToTarget() => false;

        public bool TryMoveToDefault() => false;

        public void Stop()
        {
        }

        public void SetSpeed(float value)
        {
        }
    }
}