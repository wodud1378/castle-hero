using System.Collections.Generic;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.View.Unit.Effects;
using UnityEngine;

namespace CastleHero.View.Unit
{
    public class EffectBody : MonoBehaviour, IEffectBody
    {
        public Transform top;
        public Transform middle;
        public Transform bottom;

        private readonly List<IEffect> _attachments = new();

        private Transform _fallBack;

        private void Awake()
        {
            _fallBack = bottom != null ? middle : transform;
        }

        public void Attach(IEffect effect)
        {
            if (effect is not Effect concrete)
                return;

            Transform parent = concrete.slot switch
            {
                Effect.Slot.Top => top,
                Effect.Slot.Middle => middle,
                Effect.Slot.Bottom => bottom,
                _ => _fallBack
            };

            var inverse = 1f / transform.parent.localScale.y;
            var tr = concrete.transform;
            tr.SetParent(parent);
            tr.position = parent.position;
            tr.localScale = parent.localScale * inverse;

            _attachments.Add(effect);
        }

        public void Clear()
        {
            foreach (var effect in _attachments)
                effect.Stop();

            _attachments.Clear();
        }
    }
}
