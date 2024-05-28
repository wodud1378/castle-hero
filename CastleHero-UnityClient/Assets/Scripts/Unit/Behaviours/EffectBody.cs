using System.Collections.Generic;
using RGLabs.InGame.Effects.Behaviours;
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
            Transform pos = effect.slot switch
            {
                Effect.Slot.Top => top,
                Effect.Slot.Middle => middle,
                Effect.Slot.Bottom => bottom,
                _ => null
            };

            pos ??= _fallBack;

            var tr = effect.transform;
            tr.SetParent(pos);
            tr.localScale = Vector3.one;
            tr.localPosition = Vector3.zero;
        }

        public void Clear()
        {
            _attachments.ForEach(x=> x.Stop());
        }
    }
}