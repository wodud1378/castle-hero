using Spine;
using Spine.Unity;
using UnityEngine;

namespace RGLabs.InGame.Unit
{
    public class RenderController
    {
        private readonly Skeleton _skeleton;
        private readonly Animator _animator;

        public RenderController(SkeletonRenderer renderer, Animator animator)
        {
            _skeleton = renderer != null ? renderer.skeleton : null;
            _animator = animator;
        }

        public void ApplySkin(string skinName)
        {
            if (_skeleton == null)
                return;

            if (string.IsNullOrEmpty(skinName))
                return;

            _skeleton.SetSkin(skinName);
        }

        public void SetAnimation(int hash)
        {
            if (_animator != null)
                _animator.SetTrigger(hash);
        }
    }
}