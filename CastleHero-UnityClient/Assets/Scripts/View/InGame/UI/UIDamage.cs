using System;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CastleHero.View.InGame.UI
{
    public class UIDamage : PoolItemBase
    {
        public enum ShowOn
        {
            Top,
            Direction
        }

        [FormerlySerializedAs("_animator")]
        [SerializeField] private Animator animator;
        [FormerlySerializedAs("_label")]
        [SerializeField] private TMP_Text label;

        public ShowOn showOn;

        private RectTransform _rectTransform;

        private void Awake() => _rectTransform = transform as RectTransform;

        public void Show(int amount, Vector2 position)
        {
            label.text = amount.ToString();
            _rectTransform.position = position;
        }
    }
}
