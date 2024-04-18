using Spine;
using Spine.Unity;
using UnityEngine;

namespace RGLabs.InGame.Unit
{
    public class RenderController
    {
        private readonly SkeletonMecanim _skeletonMecanim;
        private readonly Animator _animator;

        public RenderController(SkeletonMecanim skeletonMecanim, Animator animator)
        {
            _skeletonMecanim = skeletonMecanim;
            _animator = animator;
        }

        public void ApplySkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName))
                return;
            
            if (_skeletonMecanim == null)
                return;
            
            var skeleton = _skeletonMecanim.skeleton;
            skeleton.SetSkin(skinName);
            skeleton.SetToSetupPose();
        }

        public void SetAnimation(int hash)
        {
            if (_animator != null)
                _animator.SetTrigger(hash);
        }
    }
}