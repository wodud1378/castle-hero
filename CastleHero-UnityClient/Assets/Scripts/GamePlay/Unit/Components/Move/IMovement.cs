using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components.Move
{
    public interface IMovement
    {
        public bool Enabled { get; set; }
        
        public Vector2 Default { get; set; }
        
        public UnitActor CurrentTarget { get; }
        
        public Finder Finder { get; }
        
        public Vector2 Position { get; set; }

        public bool TryMoveToTarget();

        public bool TryMoveToDefault();
        
        public void Stop();

        public void SetSpeed(float value);
    }
}