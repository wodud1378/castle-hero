using System.Collections.Generic;
using RGLabs.InGame.Effects.Behaviours;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class EffectBody : MonoBehaviour
    {
        public Transform top;
        public Transform middle;
        public Transform bottom;

        private readonly List<Effect> _attachments = new();
        
        private Transform _fallBack;

        private void Awake()
        {
            _fallBack = bottom != null ? middle : transform;
        }

        public void Attach(Effect effect)
        {
            Transform parent = effect.slot switch
            {
                Effect.Slot.Top => top,
                Effect.Slot.Middle => middle,
                Effect.Slot.Bottom => bottom,
                _ => _fallBack
            };
            
            var tr = effect.transform;
            tr.position = parent.position;
            tr.localScale = parent.localScale;
            
            var subscription = effect
                .UpdateAsObservable()
                .Subscribe(_ => tr.position = parent.position)
                .AddTo(this);

            effect
                .OnDisableAsObservable()
                .Subscribe(_=> subscription.Dispose())
                .AddTo(this);
            
            _attachments.Add(effect);
        }

        public void Clear()
        {
            _attachments.ForEach(x=> x.Stop());
            _attachments.Clear();
        }
    }
}