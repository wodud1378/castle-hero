using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Effects.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class AdditionalAttack
    {
        public class Info
        {
            public UnitBehaviour unit;
            public float amount;
            public string effect;

            public float leftTime;
        }

        public readonly UnitBehaviour owner;
        
        private readonly List<Info> _fixedCollection = new();
        private readonly List<Info> _timedCollection = new();

        public AdditionalAttack(UnitBehaviour owner)
        {
            this.owner = owner;

            owner
                .UpdateAsObservable()
                .Subscribe(_ => Update())
                .AddTo(owner);
        }

        public void Add(UnitBehaviour unit, float amount, string effect, float leftTime = 0f)
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

        public void Execute(UnitBehaviour target)
        {
            ExecuteInternal(target, _fixedCollection);
            ExecuteInternal(target, _timedCollection);
        }

        private void ExecuteInternal(UnitBehaviour target, List<Info> collection)
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
                    Effect.Builder
                        .StartBuild(info.effect)
                        .To(target)
                        .From(owner.position)
                        .Run();
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