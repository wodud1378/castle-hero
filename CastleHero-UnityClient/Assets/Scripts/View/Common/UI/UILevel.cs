using DG.Tweening;
using CastleHero.Data;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Common.UI
{
    public class UILevel : MonoBehaviour
    {
        [FormerlySerializedAs("_lvColor")]
        [SerializeField] private Color lvColor;
        [FormerlySerializedAs("_lvText")]
        [SerializeField] private TMP_Text lvText;
        [FormerlySerializedAs("_gauge")]
        [SerializeField] private Slider gauge;
        [FormerlySerializedAs("_percentage")]
        [SerializeField] private TMP_Text percentage;
        [FormerlySerializedAs("_value")]
        [SerializeField] private TMP_Text value;
        [FormerlySerializedAs("_transitionTime")]
        [SerializeField] private float transitionTime;

        [FormerlySerializedAs("_overrideGaugeSprite")]
        [SerializeField] private Sprite overrideGaugeSprite;

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
            lvText.text = hasOverride && current.lv != next.lv
                ? $"{ToLvText(current.lv).WithColor(lvColor)} -> {ToLvText(next.lv).WithPositiveColor()}"
                : $"{ToLvText(current.lv).WithColor(lvColor)}";

            int maxLv = ServiceLocator.Get<IDBProvider>().Levels.MaxLv;
            bool isMaxLv = hasOverride
                ? next.lv == maxLv
                : current.lv == maxLv;

            int id = hasOverride ? next.lv : current.lv;
            int currentExp = hasOverride ? next.exp : current.exp;
            int maxExp = !ServiceLocator.Get<IDBProvider>().Levels.TryFind(id, out var entity) ? next.exp : entity.exp;
            float ratio = isMaxLv ? 1f : (float)currentExp / maxExp;
            //_gauge.image.overrideSprite = hasOverride ? _overrideGaugeSprite : null;
            gauge.maxValue = maxExp;
            gauge.value = currentExp;
            percentage.text = isMaxLv ? "Max" : $"{ratio * 100f:F1}%";
            value.text = isMaxLv ? "-" : $"{currentExp}/{maxExp}";
        }

        private string ToLvText(int lv) => $"Lv.{lv}";
    }
}
