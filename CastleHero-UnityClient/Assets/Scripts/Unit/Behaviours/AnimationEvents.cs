using System;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class AnimationEvents : MonoBehaviour
    {
        #region Attack

        public event Action OnHitEvent;
        public event Action OnReleaseAttackEvent;

        public void OnHit() => OnHitEvent?.Invoke();
        public void OnReleaseAttack() => OnReleaseAttackEvent?.Invoke();

        #endregion

        #region Skill

        public event Action OnExecuteSkillEvent;
        public event Action OnReleaseSkillEvent; 
        
        public void OnExecuteSkill() => OnExecuteSkillEvent?.Invoke();
        public void OnReleaseSkill() => OnReleaseSkillEvent?.Invoke();

        #endregion
    }
}