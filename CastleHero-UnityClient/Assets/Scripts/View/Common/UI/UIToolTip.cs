using System;
using System.Collections.Generic;
using CastleHero.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.Common.UI
{
    public class UIToolTip : MonoBehaviour
    {
        public readonly Queue<Action> onClosedQueue = new();

        [field:SerializeField] public RectTransform Root { get; private set; }

        [FormerlySerializedAs("_close")]
        [SerializeField] private Button close;
        [FormerlySerializedAs("_label")]
        [SerializeField] private TMP_Text label;

        public string Text
        {
            get => label.text;
            set => label.text = value;
        }

        private void Awake()
        {
            this.SubscribeButton(close, Close);
        }

        public void Open(string text, RectTransform parent, float pivotX, float pivotY)
        {
            label.text = text;

            gameObject.SetActive(true);

            Root.Attach(parent, new Vector2(pivotX, pivotY));
        }

        public void Close()
        {
            while (onClosedQueue.Count > 0)
            {
                onClosedQueue.Dequeue().Invoke();
            }

            gameObject.SetActive(false);
        }
    }
}
