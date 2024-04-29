using Spine.Unity;
using UnityEngine;

namespace RGLabs.InGame.Unit
{
    public class RenderController
    {
        private readonly SkeletonMecanim _skeletonMecanim;
        private readonly Animator _animator;

        // Null-Check 비용 소모를 줄이기 위해 캐싱.
        private readonly bool _hasSkeleton;
        private readonly bool _hasAnimator;

        public RenderController(SkeletonMecanim skeletonMecanim, Animator animator)
        {
            _skeletonMecanim = skeletonMecanim;
            _animator = animator;

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
            if (!_hasAnimator)
                return;

            _animator.SetTrigger(hash);
        }
    }
}