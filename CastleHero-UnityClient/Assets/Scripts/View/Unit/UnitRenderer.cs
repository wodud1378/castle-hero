using Spine;
using Spine.Unity;
using UnityEngine;
using CastleHero.GamePlay.Unit.Components;

namespace CastleHero.View.Unit
{
    /// <summary>
    /// IUnitRenderer 의 Unity/Spine 구현체. UnitRendererFactory 가 루트 Transform 에서
    /// Animator, SkeletonMecanim 을 찾아 생성한다. GamePlay 는 IUnitRenderer 만 참조.
    /// LookDirection 은 Spine 이 있으면 Skeleton.ScaleX, 없으면 Transform.localScale 로 적용.
    /// </summary>
    public sealed class UnitRenderer : IUnitRenderer
    {
        private readonly Animator _animator;
        private readonly SkeletonMecanim _skeletonMecanim;
        private readonly Skeleton _skeleton;
        private readonly Transform _root;
        private readonly bool _enableAnimation;

        private readonly bool _hasAnimator;
        private readonly bool _hasSkeleton;

        public UnitRenderer(Transform root, SkeletonMecanim skeletonMecanim, Animator animator, bool enableAnimation)
        {
            _root = root;
            _skeletonMecanim = skeletonMecanim;
            _skeleton = skeletonMecanim != null ? skeletonMecanim.skeleton : null;
            _animator = animator;
            _enableAnimation = enableAnimation;

            _hasSkeleton = _skeleton != null;
            _hasAnimator = animator != null;
        }

        public void ApplySkin(string skinName)
        {
            if (!_hasSkeleton) return;
            if (string.IsNullOrEmpty(skinName)) return;

            _skeleton.SetSkin(skinName);
            _skeleton.SetToSetupPose();
        }

        public void SetAnimation(int hash)
        {
            if (!_enableAnimation || !_hasAnimator) return;
            _animator.SetTrigger(hash);
        }

        public void SetFloat(int hash, float value)
        {
            if (!_enableAnimation || !_hasAnimator) return;
            _animator.SetFloat(hash, value);
        }

        public void ApplyLookDirection(Vector2 direction)
        {
            if (_hasSkeleton)
            {
                // 좌우 반전만. (기존 Look.BySkeleton 동일 로직)
                _skeleton.ScaleX = direction.x <= 0 ? 1f : -1f;
            }
            else if (_root != null)
            {
                var scale = _root.localScale;
                float x = Mathf.Abs(scale.x);
                _root.localScale = new Vector3(direction.x <= 0 ? x : -x, scale.y, scale.z);
            }
        }
    }
}
