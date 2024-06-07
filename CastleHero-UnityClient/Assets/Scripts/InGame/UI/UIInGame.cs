using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private Button _pause;
        [SerializeField] private RectTransform _damageRoot;
        [SerializeField] private DamagePrefabs _damagePrefabs;
        [SerializeField] private string _healPrefab;
        [SerializeField] private string _shieldPrefab;
        
        private void Awake()
        {
            this.SubscribeButton(_pause, ()=> SetPause(true));
            this.SubscribeMessage<Result>(OnResult);
            this.SubscribeMessage<AtkResult>(OnAtkResult);
            this.SubscribeMessage<HealResult>(OnHealResult);
            this.SubscribeMessage<ShieldResult>(OnShieldResult);
        }
        
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
            uiDamage.Container = Storage.poolContainer;
            uiDamage.Show(result);
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

            return await Storage.poolContainer.GetItem<UIDamage>(prefab);
        }

        public void Init()
        {
            DeadCharacters.Init();
        }

        private void SetPause(bool isPause)
        {
            if(isPause)
                Pause.Open();
            else
                Pause.Close();
        }

        // private void OnApplicationFocus(bool hasFocus)
        // {
        //     SetPause(!hasFocus);
        // }

        private async void OnResult(Result result)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            
            Result.Open(result.isCleared);
        }

        public override void Dispose()
        {
            base.Dispose();
            
            DeadCharacters.Dispose();
        }
    }
}