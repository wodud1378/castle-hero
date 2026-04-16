using NaughtyAttributes;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Common.UI
{
    public class UIState : MonoBehaviour
    {
        public enum State
        {
            Default,
            Dim,
            Highlighted,
        }

        [FormerlySerializedAs("_grayScaleOnDim")]
        [SerializeField] private bool grayScaleOnDim;
        [ShowIf("grayScaleOnDim")]
        public UIGrayScale grayScale;
        [HideIf("grayScaleOnDim")]
        public GameObject dim;
        public GameObject highlight;

        public readonly ReactiveProperty<State> state = new();

        protected virtual void OnAwake()
        {
            state
                .Subscribe(x =>
                {
                    if (highlight != null)
                        highlight.SetActive(x == State.Highlighted);

                    if (grayScaleOnDim)
                    {
                        grayScale.enabled.Value = x == State.Dim;
                    }
                    else
                    {
                        if (dim != null)
                            dim.SetActive(x == State.Dim);
                    }
                })
                .AddTo(this);
        }

        private void Awake() => OnAwake();

        private void OnValidate()
        {
            if (grayScaleOnDim && grayScale == null)
            {
                grayScale = gameObject.AddComponent<UIGrayScale>();
            }
        }
    }
}
