using System;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class AnimationEvents : MonoBehaviour
    {
        public event Action OnHitEvent;
        public event Action OnReleaseAttackEvent;

        public void OnHit() => OnHitEvent?.Invoke();
        public void OnReleaseAttack() => OnReleaseAttackEvent?.Invoke();
    }
}