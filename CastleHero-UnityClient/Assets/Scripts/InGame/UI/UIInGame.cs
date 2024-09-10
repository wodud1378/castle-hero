using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.UI
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

        [SerializeField] private TMP_Text _timerText;
        
        [SerializeField] private Button _pause;
        [SerializeField] private Button _speedUp;
        [SerializeField] private RectTransform _damageRoot;
        [SerializeField] private DamagePrefabs _damagePrefabs;
        [SerializeField] private string _healPrefab;
        [SerializeField] private string _shieldPrefab;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeButton(_pause, ()=> SetPause(true));
            this.SubscribeButton(_speedUp, () =>
            {
                var repository = Storage.inGameRepository;
                repository.speedUp.Value = !repository.speedUp.Value;

                Time.timeScale = repository.speedUp.Value
                    ? 2f
                    : 1f;
            });
            this.SubscribeMessage<GameResult>(OnResult);
            this.SubscribeMessage<AtkResult>(OnAtkResult);
            this.SubscribeMessage<HealResult>(OnHealResult);
            this.SubscribeMessage<ShieldResult>(OnShieldResult);
        }
        
        protected override void OnBack() { }

        private async void OnAtkResult(AtkResult result)
        {
            var ev = (AtkEvent)result.Event;
            var uiDamage = await GetUIDamage(ev.Type, result.IsCritical);
            if (uiDamage == null)
                return;

            Show(uiDamage, result.Event);
        }
        
        private async void OnHealResult(HealResult result)
        {
            var uiDamage = await GetUIDamage(_healPrefab);
            if (uiDamage == null)
                return;

            Show(uiDamage, result.Event);
        }
        
        private async void OnShieldResult(ShieldResult result)
        {
            var uiDamage = await GetUIDamage(_shieldPrefab);
            if (uiDamage == null)
                return;

            Show(uiDamage, result.Event);
        }

        private void Show(UIDamage uiDamage, IUnitEvent result)
        {
            var tr = uiDamage.transform;
            tr.SetParent(_damageRoot);
            tr.localScale = Vector3.one;

            var pos = uiDamage.showOn switch
            {
                UIDamage.ShowOn.Direction => UIDamagePosOnTop(result),
                UIDamage.ShowOn.Top => UIDamagePosOnDirection(result),
                _ => default
            };
            
            uiDamage.Container = Context.poolContainer;
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
            
            var closest = to.Collider.ClosestPoint(from.position);
            return closest * Random.Range(0.9f, 1.1f);
        }

        private async UniTask<UIDamage> GetUIDamage(DamageType type, bool isCritical)
        {
            string prefab = string.Empty;
            if (isCritical)
                prefab =_damagePrefabs.critical;
            else
            {
                switch (type)
                {
                    case DamageType.Normal:
                        prefab = _damagePrefabs.normal;
                        break;
                    case DamageType.Debuff:
                        prefab = _damagePrefabs.debuff;
                        break;
                }
            }

            return await GetUIDamage(prefab);
        }

        private async UniTask<UIDamage> GetUIDamage(string prefab)
        {
            if (string.IsNullOrEmpty(prefab))
                return null;

            return await Context.poolContainer.GetItem<UIDamage>(prefab);
        }

        public void Init()
        {
            DeadCharacters.Init();
            
            Storage.inGameRepository.leftTime
                .Subscribe(x => _timerText.text = $"{(int)x / 60:D2}:{(int)x % 60:D2}")
                .AddTo(this);
        }

        private void SetPause(bool isPause)
        {
            if(isPause)
                Pause.Open();
            else
                Pause.Close();
        }

        private async void OnResult(GameResult result)
        {
            if (!result.isCleared)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
            }
            
            Result
                .Open(result)
                .Forget();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            DeadCharacters.Dispose();
        }
    }
}