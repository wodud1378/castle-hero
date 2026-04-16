using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components
{
    /// <summary>
    /// 유닛의 렌더 컴포넌트(Animator/Spine)를 감싼 IUnitRenderer 구현체를 생성.
    /// View 모듈에서 구현체를 등록하여 GamePlay 쪽이 View 참조 없이 사용 가능.
    /// </summary>
    public interface IUnitRendererFactory
    {
        IUnitRenderer Create(Transform root, bool enableAnimation);
    }
}
