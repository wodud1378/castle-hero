using System;
using UnityEngine;
using CastleHero.GamePlay.Unit.Behaviours;

namespace CastleHero.View.Unit
{
    /// <summary>
    /// Animator Event 를 받아 게임플레이로 전달하는 View 컴포넌트.
    /// Animator 의 이벤트 프레임에서 OnHit/OnReleaseAttack/OnExecuteSkill/OnReleaseSkill 콜백을 호출.
    /// GamePlay(Attack, Skill)는 IAnimationEventProvider 인터페이스로만 구독.
    /// </summary>
    public sealed class AnimationEvents : MonoBehaviour, IAnimationEventProvider
    {
        public event Action OnHitEvent;
        public event Action OnReleaseAttackEvent;
        public event Action OnExecuteSkillEvent;
        public event Action OnReleaseSkillEvent;

        // Animator 이벤트 타깃. 메서드명이 기존과 동일해야 기존 Animator 클립에서 호출됨.
        public void OnHit() => OnHitEvent?.Invoke();
        public void OnReleaseAttack() => OnReleaseAttackEvent?.Invoke();
        public void OnExecuteSkill() => OnExecuteSkillEvent?.Invoke();
        public void OnReleaseSkill() => OnReleaseSkillEvent?.Invoke();
    }
}
