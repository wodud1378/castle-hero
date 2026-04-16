using System;

namespace CastleHero.GamePlay.Unit.Behaviours
{
    /// <summary>
    /// 유닛 애니메이션 이벤트 제공. 구현체는 View (MonoBehaviour, Animator Event 수신).
    /// Attack/Skill 등이 이 이벤트를 구독해 로직 트리거.
    /// </summary>
    public interface IAnimationEventProvider
    {
        event Action OnHitEvent;
        event Action OnReleaseAttackEvent;
        event Action OnExecuteSkillEvent;
        event Action OnReleaseSkillEvent;
    }
}
