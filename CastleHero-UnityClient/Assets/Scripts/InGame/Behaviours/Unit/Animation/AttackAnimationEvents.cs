using System;
using RGLabs.InGame.Behaviours.Unit.Components;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Unit.Animation
{
    public class AttackAnimationEvents : MonoBehaviour
    {
        public event Action OnHitEvent;
        
        public void OnHit()
        {
            OnHitEvent?.Invoke();
        }
    }
}