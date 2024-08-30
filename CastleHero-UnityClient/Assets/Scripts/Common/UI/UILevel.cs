using DG.Tweening;
using RGLabs.Data;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UILevel : MonoBehaviour
    {
        [SerializeField] private Color _lvColor;
        [SerializeField] private TMP_Text _lvText;
        [SerializeField] private Slider _gauge;
        [SerializeField] private TMP_Text _percentage;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private float _transitionTime;

        [SerializeField] private Sprite _overrideGaugeSprite;

        private readonly ReactiveProperty<int> _lv = new(0);
        private readonly ReactiveProperty<int> _exp = new(0);

        private readonly ReactiveProperty<int> _nextLv = new(0);
        private readonly ReactiveProperty<int> _nextExp = new(0);

        private readonly BoolReactiveProperty _hasOverride = new(false);
        
        private void Awake()
        {
            Observable
                .CombineLatest(
                    _lv,
                    _exp,
                    _nextLv,
                    _nextExp,
                    _hasOverride,
                    (l, e, nL, nE, b)
                        => ((l, e), (nL, nE), b))
                .Subscribe(OnDataChanged)
                .AddTo(this);
        }

        public void Set(UnitInfo info) => Set(info.lv, info.exp);

        public void Set(int lv, int exp)
        {
            _lv.Value = lv;
            _exp.Value = exp; 
        }
        
        public void SetOverride(int nextLv, int exp)
        {
            _nextLv.Value = nextLv;
            _nextExp.Value = exp;
            _hasOverride.Value = true;
        }

        public void ReleaseOverride() => _hasOverride.Value = false;
        
        private void OnDataChanged(((int lv, int exp) current, (int lv, int exp) next, bool hasOverride) data)
        {
            var current = data.current;
            var next = data.next;

            bool hasOverride = data.hasOverride;
            _lvText.text = hasOverride && current.lv != next.lv
                ? $"{ToLvText(current.lv).WithColor(_lvColor)} -> {ToLvText(next.lv).WithPositiveColor()}"
                : $"{ToLvText(current.lv).WithColor(_lvColor)}";

            int maxLv = Storage.db.levels.MaxLv;
            bool isMaxLv = hasOverride
                ? next.lv == maxLv
                : current.lv == maxLv;
            
            int id = hasOverride ? next.lv : current.lv;
            int currentExp = hasOverride ? next.exp : current.exp;
            int maxExp = !Storage.db.levels.TryFind(id, out var entity) ? next.exp : entity.exp;
            float ratio = isMaxLv ? 1f : (float)currentExp / maxExp;
            //_gauge.image.overrideSprite = hasOverride ? _overrideGaugeSprite : null;
            _gauge.maxValue = maxExp;
            _gauge.value = currentExp;
            _percentage.text = isMaxLv ? "Max" : $"{ratio * 100f:F1}%";
            _value.text = isMaxLv ? "-" : $"{currentExp}/{maxExp}";
        }

        private string ToLvText(int lv) => $"Lv.{lv}";
    }
}