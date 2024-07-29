using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Inventory;
using RGLabs.Network.Shared;
using RGLabs.Unit;
using RGLabs.Utility;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_CharInfo.prefab")]
    public class PopupCharacter : PopupBase
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _lv;
        [SerializeField] private GameObject[] _stars;
        [SerializeField] private RectTransform _prefabRoot;
        [SerializeField] private UILevel _level;
        [SerializeField] private UIStatusText[] _statusTexts;
        [SerializeField] private UIEquipmentSlot[] _equipments;

        [SerializeField] private Button _levelUp;
        [SerializeField] private Button _upgrade;

        private UnitInfo _unit;

        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeButton(_levelUp, OnLevelUp);
            this.SubscribeButton(_upgrade, OnUpgrade);
        }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is UnitInfo unit)
                Init(unit);

            return UniTask.CompletedTask;
        }

        private void Init(UnitInfo info)
        {
            _unit = info;
            
            if (!Storage.db.units.TryFind(_unit.id, out var unitEntity))
                return;

            if (!Storage.db.balances.TryFind(_unit.id, out var balanceEntity))
                return;

            _name.text = unitEntity.name;
            _level.Set(_unit);

            int lv = _unit.lv;
            int rate = _unit.rate;

            SetCharacter(unitEntity.uiPrefab).Forget();
            UpdateRate(rate);

            var equipments = Storage.userRepository.EquipItems(info.equipments).ToArray();
            
            UpdateStatusTexts(lv, rate, unitEntity, balanceEntity, equipments);
            UpdateEquipmentSlots(equipments);
        }

        private async UniTask SetCharacter(string dataPath)
        {
            _prefabRoot.gameObject.SetActive(false);
            
            await Addressables.InstantiateAsync(dataPath, _prefabRoot);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            _prefabRoot.gameObject.SetActive(true);
        }

        private void UpdateRate(int rate)
        {
            for (int i = 0, length = _stars.Length; i < length; ++i)
            {
                _stars[i].SetActive((i + 1) <= rate);
            }
        }

        private void UpdateEquipmentSlots(EquipItem[] equipments)
        {
            if (equipments == null)
                return;
            
            int index = 0;
            while (index.IsValidIndex(equipments, _equipments))
            {
                _equipments[index].Init(equipments[index]).Forget();
                ++index;
            }
        }

        private void UpdateStatusTexts(int lv, int rate, UnitEntity unit, UnitBalanceEntity balance,
            EquipItem[] equipments)
        {
            var baseStatus = BaseStatus(lv, rate, unit, balance);
            var equipStatus = equipments.Total();

            foreach (var label in _statusTexts)
            {
                var type = label.type;
                var baseVal = baseStatus.GetValueOrDefault(type, 0f);
                var equip = equipStatus.GetValueOrDefault(type, 0f);

                label.SetText(baseVal, equip);
            }
        }

        private Dictionary<Status.Type, float> BaseStatus(int lv, int rate, UnitEntity unit, UnitBalanceEntity balance)
        {
            balance.AdditionalStatus(lv, rate, out var additional, out _);

            return new Dictionary<Status.Type, float>
            {
                { Status.Type.Hp, unit.hp + additional.GetValueOrDefault(Status.Type.Hp) },
                { Status.Type.Atk, unit.atk + additional.GetValueOrDefault(Status.Type.Atk) },
                { Status.Type.Critical, unit.critical + additional.GetValueOrDefault(Status.Type.Critical) },
                { Status.Type.CriticalAtk, unit.criticalAtk + additional.GetValueOrDefault(Status.Type.CriticalAtk) },
                { Status.Type.AtkSpeed, unit.atkSpeed + additional.GetValueOrDefault(Status.Type.AtkSpeed) },
                { Status.Type.MoveSpeed, unit.speed + additional.GetValueOrDefault(Status.Type.MoveSpeed) },
                { Status.Type.AtkRange, unit.atkRange + additional.GetValueOrDefault(Status.Type.AtkRange) },
                { Status.Type.MoveRange, unit.moveRange + additional.GetValueOrDefault(Status.Type.MoveRange) },
            };
        }

        private void OnLevelUp() => Context.popupManager.OpenAsync<PopupLevelUp>(_unit).Forget();

        private void OnUpgrade()=> Context.popupManager.OpenAsync<PopupRateUp>(_unit).Forget();
    }
}