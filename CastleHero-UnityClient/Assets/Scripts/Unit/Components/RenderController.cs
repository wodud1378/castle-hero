using Spine.Unity;
using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class RenderController
    {
        private readonly SkeletonMecanim _skeletonMecanim;
        private readonly Animator _animator;
        private readonly bool _enableAnimation;

        // Null-Check 비용 소모를 줄이기 위해 캐싱.
        private readonly bool _hasSkeleton;
        private readonly bool _hasAnimator;

        public RenderController(SkeletonMecanim skeletonMecanim, Animator animator, bool enableAnimation)
        {
            _skeletonMecanim = skeletonMecanim;
            _animator = animator;
            _enableAnimation = enableAnimation;

            _hasSkeleton = skeletonMecanim != null;
            _hasAnimator = animator != null;
        }

        public void ApplySkin(string skinName)
        {
            if (!_hasSkeleton)
                return;

            if (string.IsNullOrEmpty(skinName))
                return;

            var skeleton = _skeletonMecanim.skeleton;
            skeleton.SetSkin(skinName);
            skeleton.SetToSetupPose();
        }

        public void SetAnimation(int hash)
        {
            if (!_enableAnimation || !_hasAnimator)
                return;

            _animator.SetTrigger(hash);
        }

        public void SetFloat(int hash, float value)
        {
            if (!_enableAnimation || !_hasAnimator)
                return;

            _animator.SetFloat(hash, value);
        }
    }
}