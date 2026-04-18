using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.GamePlay.InGame.Behaviours;
using CastleHero.View.InGame.Behaviours;
using CastleHero.GamePlay.InGame.System;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

using CastleHero.GamePlay.InGame;
namespace CastleHero.View.InGame.UI
{
    [Serializable]
    public struct DamagePrefabs
    {
        public string normal;
        public string critical;
        public string debuff;
    }

    public class UIInGame : UIMain
    {
        [field:SerializeField] public UIRecoverList DeadCharacters { get; private set; }
        [field:SerializeField] public UIGameResult Result { get; private set; }
        [field:SerializeField] public UIPause Pause { get; private set; }
        [field:SerializeField] public UIGlobalSkill GlobalSkill { get; private set; }

        [FormerlySerializedAs("_timerText")]
        [SerializeField] private TMP_Text timerText;

        [FormerlySerializedAs("_pause")]
        [SerializeField] private Button pause;
        [FormerlySerializedAs("_speedUp")]
        [SerializeField] private Button speedUp;
        [FormerlySerializedAs("_damageRoot")]
        [SerializeField] private RectTransform damageRoot;
        [FormerlySerializedAs("_damagePrefabs")]
        [SerializeField] private DamagePrefabs damagePrefabs;
        [FormerlySerializedAs("_healPrefab")]
        [SerializeField] private string healPrefab;
        [FormerlySerializedAs("_shieldPrefab")]
        [SerializeField] private string shieldPrefab;

        private CastleHero.Data.Repositories.SettingRepository _settings;
        private PoolContainer _poolContainer;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _settings = sl.Get<CastleHero.Data.Repositories.SettingRepository>();
            _poolContainer = sl.Get<PoolContainer>();

            this.SubscribeButton(pause, () => SetPause(true));
            this.SubscribeButton(speedUp, () =>
            {
                _settings.speedUp.Value = !_settings.speedUp.Value;

                Time.timeScale = _settings.speedUp.Value
                    ? 2f
                    : 1f;
            });
            this.SubscribeMessage<GameResult>(OnResult);
            this.SubscribeMessage<AtkResult>(r => OnAtkResult(r).SafeForget());
            this.SubscribeMessage<HealResult>(r => OnHealResult(r).SafeForget());
            this.SubscribeMessage<ShieldResult>(r => OnShieldResult(r).SafeForget());
        }

        protected override void OnBack() { }

        private async UniTask OnAtkResult(AtkResult result)
        {
            var ev = (AtkEvent)result.Event;
            var uiDamage = await GetUIDamage(ev.Type, result.IsCritical);
            if (uiDamage == null)
                return;

            Show(uiDamage, result.Event);
        }

        private async UniTask OnHealResult(HealResult result)
        {
            var uiDamage = await GetUIDamage(healPrefab);
            if (uiDamage == null)
                return;

            Show(uiDamage, result.Event);
        }

        private async UniTask OnShieldResult(ShieldResult result)
        {
            var uiDamage = await GetUIDamage(shieldPrefab);
            if (uiDamage == null)
                return;

            Show(uiDamage, result.Event);
        }

        private void Show(UIDamage uiDamage, IUnitEvent result)
        {
            var tr = uiDamage.transform;
            tr.SetParent(damageRoot);
            tr.localScale = Vector3.one;

            var pos = uiDamage.showOn switch
            {
                UIDamage.ShowOn.Direction => UIDamagePosOnTop(result),
                UIDamage.ShowOn.Top => UIDamagePosOnDirection(result),
                _ => default
            };

            uiDamage.Container = _poolContainer;
            uiDamage.Show((int)result.Amount, pos);
        }

        private Vector2 UIDamagePosOnTop(IUnitEvent ev)
        {
            var bounds = ev.To.Collider.bounds;
            return new Vector2(bounds.center.x, bounds.max.y);
        }

        private Vector2 UIDamagePosOnDirection(IUnitEvent ev)
        {
            var from = ev.From;
            var to = ev.To;
            if (!to.IsValid() || !from.IsValid())
                return UIDamagePosOnTop(ev);

            var closest = to.Collider.ClosestPoint(from.Position);
            return closest * Random.Range(0.9f, 1.1f);
        }

        private async UniTask<UIDamage> GetUIDamage(DamageType type, bool isCritical)
        {
            string prefab = string.Empty;
            if (isCritical)
                prefab = damagePrefabs.critical;
            else
            {
                switch (type)
                {
                    case DamageType.Normal:
                        prefab = damagePrefabs.normal;
                        break;
                    case DamageType.Debuff:
                        prefab = damagePrefabs.debuff;
                        break;
                }
            }

            return await GetUIDamage(prefab);
        }

        private UniTask<UIDamage> GetUIDamage(string prefab)
        {
            if (string.IsNullOrEmpty(prefab))
                return UniTask.FromResult<UIDamage>(null);

            _poolContainer.TryGet<UIDamage>(prefab, out var item);
            return UniTask.FromResult(item);
        }

        public void Init()
        {
            DeadCharacters.Init();

            InGameSession.Current.LeftTime
                .Subscribe(x => timerText.text = $"{(int)x / 60:D2}:{(int)x % 60:D2}")
                .AddTo(this);
        }

        private void SetPause(bool isPause)
        {
            if (isPause)
                Pause.Open();
            else
                Pause.Close();
        }

        private void OnResult(GameResult result) => OnResultAsync(result).SafeForget();

        private async UniTaskVoid OnResultAsync(GameResult result)
        {
            if (!result.IsCleared)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
            }

            Result
                .Open(result)
                .SafeForget();
        }

        public override void Dispose()
        {
            base.Dispose();

            DeadCharacters.Dispose();
        }
    }
}
