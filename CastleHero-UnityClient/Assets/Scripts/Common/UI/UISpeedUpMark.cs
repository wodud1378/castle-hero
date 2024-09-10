using RGLabs.Data;
using Spine.Unity;
using UniRx;
using UnityEngine;

namespace RGLabs.Common.UI
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class UISpeedUpMark : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic _skeleton;

        private void Awake()
        {
            Storage.inGameRepository.speedUp
                .Subscribe(OnSpeedUpToggle)
                .AddTo(this);
        }

        private void OnSpeedUpToggle(bool isActive) 
            => _skeleton.AnimationState.SetAnimation(0, isActive ? "Enable" : "Disable", true);

        private void OnValidate()
        {
            if (_skeleton != null)
                return;

            _skeleton = GetComponent<SkeletonGraphic>();
        }
    }
}