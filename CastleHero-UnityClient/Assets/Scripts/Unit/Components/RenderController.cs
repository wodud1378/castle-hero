using Spine.Unity;
using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class RenderController
    {
        public readonly Animator animator;
        public readonly SkeletonMecanim skeletonMecanim;
        public readonly bool enableAnimation;

        // Null-Check 비용 소모를 줄이기 위해 캐싱.
        private readonly bool _hasSkeleton;
        private readonly bool _hasAnimator;

        public RenderController(SkeletonMecanim skeletonMecanim, Animator animator, bool enableAnimation)
        {
            this.skeletonMecanim = skeletonMecanim;
            this.animator = animator;
            this.enableAnimation = enableAnimation;

            _hasSkeleton = skeletonMecanim != null;
            _hasAnimator = animator != null;
        }

        public void ApplySkin(string skinName)
        {
            if (!_hasSkeleton)
                return;

            if (string.IsNullOrEmpty(skinName))
                return;

            var skeleton = skeletonMecanim.skeleton;
            skeleton.SetSkin(skinName);
            skeleton.SetToSetupPose();
        }

        public void SetAnimation(int hash)
        {
            if (!enableAnimation || !_hasAnimator)
                return;
            
            animator.SetTrigger(hash);
        }

        public void SetFloat(int hash, float value)
        {
            if (!enableAnimation || !_hasAnimator)
                return;

            animator.SetFloat(hash, value);
        }
    }
}