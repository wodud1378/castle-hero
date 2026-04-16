using CastleHero.GamePlay.Unit.Behaviours;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Effects
{
    public interface IEffect
    {
        public float Duration { get; set; }

        public void SetForward(Vector2 forward);
        public void Run(Vector2 startAt = default);
        public void Stop();
        public void SetTarget(UnitBehaviour unit);
        public void SetTarget(Vector2 position);
    }
}