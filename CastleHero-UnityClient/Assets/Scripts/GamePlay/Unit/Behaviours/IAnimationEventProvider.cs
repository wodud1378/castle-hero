using System;

namespace CastleHero.GamePlay.Unit.Behaviours
{
    public interface IAnimationEventProvider
    {
        IObservable<UniRx.Unit> OnHit { get; }
        IObservable<UniRx.Unit> OnReleaseAttack { get; }
        IObservable<UniRx.Unit> OnExecuteSkill { get; }
        IObservable<UniRx.Unit> OnReleaseSkill { get; }
    }
}
