using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components
{
    /// <summary>
    /// 유닛 렌더링 추상화. 구현체는 View 모듈에 위치 (Animator/Spine 의존).
    /// UnitCore 는 이 인터페이스만 알고 View 구현체는 몰라야 함.
    /// </summary>
    public interface IUnitRenderer
    {
        void ApplySkin(string skinName);
        void SetAnimation(int hash);
        void SetFloat(int hash, float value);

        /// <summary>
        /// 유닛의 시선/좌우 반전 적용. 방향 벡터 x 성분 기반으로 Skeleton.ScaleX 또는 Transform.localScale 뒤집기.
        /// </summary>
        void ApplyLookDirection(Vector2 direction);
    }
}
