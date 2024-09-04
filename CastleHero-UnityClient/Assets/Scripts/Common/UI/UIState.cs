using NaughtyAttributes;
using UniRx;
using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UIState : MonoBehaviour
    {
        public enum State
        {
            Default,
            Dim,
            Highlighted,
        }
        
        [SerializeField] private bool _grayScaleOnDim;
        [ShowIf("_grayScaleOnDim")]
        public UIGrayScale grayScale;
        [HideIf("_grayScaleOnDim")]
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

                    if (_grayScaleOnDim)
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
            if (_grayScaleOnDim && grayScale == null)
            {
                grayScale = gameObject.AddComponent<UIGrayScale>();
            }
        }
    }
}