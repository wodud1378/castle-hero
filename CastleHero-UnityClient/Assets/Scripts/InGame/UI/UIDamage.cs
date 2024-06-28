using System;
using RGLabs.Common.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.UI
{
    public class UIDamage : PoolItemBase
    {
        public enum ShowOn
        {
            Top,
            Direction
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _label;
        
        public ShowOn showOn;

        private RectTransform _rectTransform;

        private void Awake() => _rectTransform = transform as RectTransform;

        public void Show(int amount, Vector2 position)
        {
            _label.text = amount.ToString();
            _rectTransform.position = position;
        }
    }
}