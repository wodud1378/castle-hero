using System;
using UnityEngine;
using CastleHero.GamePlay.Unit.Behaviours;
using UniRx;

namespace CastleHero.View.Unit
{
    public sealed class AnimationEvents : MonoBehaviour, IAnimationEventProvider
    {
        private readonly Subject<UniRx.Unit> _onHit = new();
        private readonly Subject<UniRx.Unit> _onReleaseAttack = new();
        private readonly Subject<UniRx.Unit> _onExecuteSkill = new();
        private readonly Subject<UniRx.Unit> _onReleaseSkill = new();

        IObservable<UniRx.Unit> IAnimationEventProvider.OnHit => _onHit;
        IObservable<UniRx.Unit> IAnimationEventProvider.OnReleaseAttack => _onReleaseAttack;
        IObservable<UniRx.Unit> IAnimationEventProvider.OnExecuteSkill => _onExecuteSkill;
        IObservable<UniRx.Unit> IAnimationEventProvider.OnReleaseSkill => _onReleaseSkill;

        public void OnHit() => _onHit.OnNext(UniRx.Unit.Default);
        public void OnReleaseAttack() => _onReleaseAttack.OnNext(UniRx.Unit.Default);
        public void OnExecuteSkill() => _onExecuteSkill.OnNext(UniRx.Unit.Default);
        public void OnReleaseSkill() => _onReleaseSkill.OnNext(UniRx.Unit.Default);
    }
}
