using System;
using System.Collections.Generic;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIToolTip : MonoBehaviour
    {
        public readonly Queue<Action> onClosedQueue = new();
        
        [field:SerializeField] public RectTransform Root { get; private set; }
        
        [SerializeField] private Button _close;
        [SerializeField] private TMP_Text _label;

        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        private void Awake()
        {
            this.SubscribeButton(_close, Close);
        }

        public void Open(string text, RectTransform parent, float pivotX, float pivotY)
        {
            _label.text = text;
            
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