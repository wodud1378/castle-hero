using CastleHero.Data;
using Spine.Unity;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Common.UI
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class UISpeedUpMark : MonoBehaviour
    {
        [FormerlySerializedAs("_skeleton")]
        [SerializeField] private SkeletonGraphic skeleton;

        private void Awake()
        {
            ServiceLocator.Get<ISettingRepository>().speedUp
                .Subscribe(OnSpeedUpToggle)
                .AddTo(this);
        }

        private void OnSpeedUpToggle(bool isActive)
            => skeleton.AnimationState.SetAnimation(0, isActive ? "Enable" : "Disable", true);

        private void OnValidate()
        {
            if (skeleton != null)
                return;

            skeleton = GetComponent<SkeletonGraphic>();
        }
    }
}
