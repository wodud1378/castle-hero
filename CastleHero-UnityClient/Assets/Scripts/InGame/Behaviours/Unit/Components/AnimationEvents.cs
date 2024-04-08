using System;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Unit.Components
{
    public class AnimationEvents : MonoBehaviour
    {
        public event Action OnHitEvent;
        public event Action OnReleaseAttackEvent;

        public void OnHit() => OnHitEvent?.Invoke();
        public void OnReleaseAttack() => OnReleaseAttackEvent?.Invoke();
    }
}