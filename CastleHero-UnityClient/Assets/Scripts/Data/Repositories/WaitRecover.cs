using System;
using CastleHero.Common.Behaviours;
using UniRx;

namespace CastleHero.Data.Repositories
{
    public class WaitRecover : IDisposable
    {
        public IUnitActor actor;
        public UnityEngine.Vector2 position;
        public float time;

        public readonly ReactiveProperty<float> leftTime = new();
        public readonly ReactiveProperty<(float left, float total)> summary = new();

        private IDisposable _subscription;
        private ReactiveCollection<WaitRecover> _root;

        public void Bind(ReactiveCollection<WaitRecover> root, IDisposable subscription)
        {
            leftTime
                .Select(x => (x, time))
                .DistinctUntilChanged()
                .Subscribe(x => summary.Value = x);

            _root = root;
            _subscription = subscription;
        }

        public void Clear()
        {
            _subscription?.Dispose();

            summary.Dispose();
            leftTime.Dispose();
        }

        public void Dispose()
        {
            _root?.Remove(this);

            Clear();
        }
    }
}
