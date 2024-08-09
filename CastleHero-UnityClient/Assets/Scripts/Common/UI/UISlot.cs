using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UISlot : MonoBehaviour, IDisposable, IPointerClickHandler
    {
        public enum State
        {
            Diminished,
            Default,
            Highlighted,
        }
        
        public event Action<UISlot> OnClick;
        public GameObject highlight;
        public GameObject diminish;
        public Image icon;
        public TMP_Text label;

        public readonly ReactiveProperty<State> state = new();

        public Color LabelColor
        {
            get => label.color;
            set => label.text = label.text.WithColor(value);
        }

        private CancellationTokenSource _ctSource;

        private void Awake()
        {
            state
                .Subscribe(x =>
                {
                    if(highlight != null)
                        highlight.SetActive(x == State.Highlighted);
                    if(diminish != null)
                        diminish.SetActive(x == State.Diminished);
                })
                .AddTo(this);
        }

        public async UniTask Init(string spritePath, string text = "")
        {
            _ctSource?.Cancel();
            _ctSource = new();
            
            icon.enabled = false;

            Sprite sprite;
            try
            {
                sprite = await spritePath.Load<Sprite>(_ctSource.Token);
            }
            catch
            {
                Debug.LogError($"Sprite Not Found. path=\"{spritePath}\"");
                sprite = null;
            }

            Init(sprite, text);
        }

        private void Init(Sprite sprite = null, string text = "")
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }

            if (label != null)
                label.text = text;
        }

        public virtual void Dispose()
        {
            _ctSource?.Cancel();
            _ctSource?.Dispose();
            _ctSource = null;
            
            icon.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(this);
    }
}