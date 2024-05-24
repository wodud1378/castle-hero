using RGLabs.InGame.Behaviours.Effects;
using RGLabs.InGame.Effects;
using RGLabs.InGame.Effects.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class EffectBody : MonoBehaviour
    {
        public Transform top;
        public Transform middle;
        public Transform bottom;

        private Transform _fallBack;

        private void Awake()
        {
            _fallBack = middle != null ? middle : transform;
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
    }
}