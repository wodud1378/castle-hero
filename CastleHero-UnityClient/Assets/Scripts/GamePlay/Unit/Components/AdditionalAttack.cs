using System.Collections.Generic;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components
{
    public class AdditionalAttack
    {
        public class Info
        {
            public UnitActor unit;
            public float amount;
            public string effect;

            public float leftTime;
        }

        public readonly UnitActor owner;
        
        private readonly List<Info> _fixedCollection = new();
        private readonly List<Info> _timedCollection = new();

        public AdditionalAttack(UnitActor owner)
        {
            this.owner = owner;

            owner
                .UpdateAsObservable()
                .Subscribe(_ => Update())
                .AddTo(owner);
        }

        public void Add(UnitActor unit, float amount, string effect, float leftTime = 0f)
        {
            var info = new Info
            {
                unit = unit,
                amount = amount,
                effect = effect,
                leftTime = leftTime
            };
            
            if(leftTime > 0f)
                _timedCollection.Add(info);
            else
                _fixedCollection.Add(info);
        }

        public void Execute(UnitActor target)
        {
            ExecuteInternal(target, _fixedCollection);
            ExecuteInternal(target, _timedCollection);
        }

        private void ExecuteInternal(UnitActor target, List<Info> collection)
        {
            foreach (var info in collection)
            {
                new AtkEvent
                {
                    Type = DamageType.Normal,
                    From = info.unit == null ? owner : info.unit,
                    To = target,
                    Amount = info.amount,
                }.Publish();

                if (!string.IsNullOrEmpty(info.effect))
                {
                    new ProjectileEvent
                    {
                        Prefab = info.effect,
                        From = owner,
                        To = target
                    }.Publish();
                }
            }
        }

        private void Update()
        {
            _timedCollection.ForEach(x=>x.leftTime -= Time.deltaTime);
            _timedCollection.RemoveAll(x => x.leftTime <= 0f);
        }
    }
}