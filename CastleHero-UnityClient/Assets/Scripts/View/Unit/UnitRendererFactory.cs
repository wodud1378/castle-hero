using Spine.Unity;
using UnityEngine;
using CastleHero.GamePlay.Unit.Components;

namespace CastleHero.View.Unit
{
    /// <summary>
    /// GameObject 루트에서 Animator / SkeletonMecanim 을 찾아 UnitRenderer 를 구성.
    /// Context.LoadAsync 단계에서 IUnitRendererFactory 로 ServiceLocator 등록.
    /// </summary>
    public sealed class UnitRendererFactory : IUnitRendererFactory
    {
        public IUnitRenderer Create(Transform root, bool enableAnimation)
        {
            var skeleton = root.GetComponentInChildren<SkeletonMecanim>();
            var animator = root.GetComponentInChildren<Animator>();
            return new UnitRenderer(root, skeleton, animator, enableAnimation);
        }
    }
}
