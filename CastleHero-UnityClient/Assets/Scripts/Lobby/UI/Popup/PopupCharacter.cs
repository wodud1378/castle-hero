using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Inventory;
using RGLabs.Lobby.UI.Inventory.Popup;
using RGLabs.Network;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Unit;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_CharInfo.prefab")]
    public class PopupCharacter : PopupBase
    {
        private static readonly Dictionary<Elemental.Type, string> ElementalIcons = new()
        {
            { Elemental.Type.None, "Sprites/Global/UI/Symbol_Elemental_Earth.png" },
            { Elemental.Type.Earth, "Sprites/Global/UI/Symbol_Elemental_Earth.png" },
            { Elemental.Type.Fire, "Sprites/Global/UI/Symbol_Elemental_Fire.png" },
            { Elemental.Type.Water, "Sprites/Global/UI/Symbol_Elemental_Water.png" },
            { Elemental.Type.Wind, "Sprites/Global/UI/Symbol_Elemental_Wind.png" },
        };

        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _lv;
        [SerializeField] private GameObject[] _stars;
        [SerializeField] private RectTransform _prefabRoot;
        [SerializeField] private UILevel _level;
        [SerializeField] private UIStatusText[] _statusTexts;
        [SerializeField] private List<Button> _emptySlotButtons;
        [SerializeField] private List<UIEquipmentSlot> _equipments;
        
        [SerializeField] private GameObject _atkElementRoot;
        [SerializeField] private AddressableImage _atkElement;
        [SerializeField] private TMP_Text _atkElementLv;
        
        [SerializeField] private GameObject _defElementRoot;
        [SerializeField] private AddressableImage _defElement;
        [SerializeField] private TMP_Text _defElementLv;

        [SerializeField] private Button _levelUp;
        [SerializeField] private Button _upgrade;

        private readonly ReactiveProperty<UnitInfo> _unit = new();
        private GameObject _character;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_levelUp, OnLevelUp);
            this.SubscribeButton(_upgrade, OnUpgrade);

            Storage.userRepository.characters
                .WhenUpdate(_unit, x => _unit.Value = x)
                .AddTo(this);

            _unit
                .Subscribe(OnUnitChanged)
                .AddTo(this);

            for (var slot = EquipmentSlot.Weapon; slot <= EquipmentSlot.Necklace; ++slot)
            {
                var inner = slot;
                this.SubscribeButton(_emptySlotButtons[(int)slot], () => Equip(inner));
            }

            _equipments.ForEach(x =>
                x.OnClick += s =>
                {
                    if (s is UIEquipmentSlot { Item: not null } equipmentSlot)
                    {
                        Context.popups.Open<PopupEquipItem>(equipmentSlot.Item);
                    }
                });
        }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is UnitInfo unit)
                _unit.Value = unit;

            return UniTask.CompletedTask;
        }

        private void OnUnitChanged(UnitInfo info)
        {
            if (info == null)
                return;

            if (!Storage.db.units.TryFind(_unit.Value.id, out var unitEntity))
                return;

            if (!Storage.db.balances.TryFind(_unit.Value.id, out var balanceEntity))
                return;

            _name.text = unitEntity.name;
            _level.Set(_unit.Value);

            int lv = _unit.Value.lv;
            int rate = _unit.Value.rate;

            SetCharacter(unitEntity.uiPrefab).Forget();
            UpdateRate(rate);

            var equipments = Storage.userRepository.EquipItems(info.equipments).ToList();

            UpdateUI(lv, rate, unitEntity, balanceEntity, equipments);
            UpdateEquipmentSlots(equipments);
        }

        private async UniTask SetCharacter(string dataPath)
        {
            if (_character != null)
                Addressables.ReleaseInstance(_character);

            _prefabRoot.gameObject.SetActive(false);

            _character = await Addressables.InstantiateAsync(dataPath, _prefabRoot);
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

        private async void Equip(EquipmentSlot slot)
        {
            var items = Storage.userRepository.inventory.items
                .OfType<EquipItem>()
                .Where(x => x.slot == (int)slot);

            var selection = await Context.popups.OpenAsync<PopupSelectItem>(items);

            bool closeWithNoSelection = false;
            bool confirm = false;
            EquipItem equipItem = null;
            while (!closeWithNoSelection && !confirm)
            {
                selection.BeginSelect(false);

                var selected = await selection.SelectTask;
                if (selected != null)
                {
                    if (selected is EquipItem e && e.slot == (int)slot)
                    {
                        var compare = await Context.popups.OpenAsync<PopupCompareEquipment>(_unit.Value, e);
                        compare.BeginSelect(true);

                        confirm = await compare.SelectTask;

                        if (confirm)
                            equipItem = e;
                    }
                }
                else
                {
                    closeWithNoSelection = true;
                }
            }

            if (closeWithNoSelection)
                return;

            selection.Close();

            var result = await NetworkService.Character.Equip(_unit.Value.id, equipItem.Guid);
            if (!result.IsSuccess)
                Context.popups.Open<PopupCommon>(result.error);
        }

        private void UpdateEquipmentSlots(List<EquipItem> equipments)
        {
            if (equipments == null)
                return;

            for (var slot = EquipmentSlot.Weapon; slot <= EquipmentSlot.Necklace; ++slot)
            {
                int index = (int)slot;
                var item = equipments.Find(x => x.slot == index);
                var ui = _equipments[index];
                if (item == null)
                    ui.gameObject.SetActive(false);
                else
                {
                    ui.gameObject.SetActive(true);
                    ui.Init(item).Forget();
                }
            }
        }

        private void UpdateUI(int lv, int rate, UnitEntity unit, UnitBalanceEntity balance,
            List<EquipItem> equipments)
        {
            var elemental = new Elemental();
            var baseStatus = BaseStatus(lv, rate, unit, balance);
            var equipStatus = equipments.Total(ref elemental);
            foreach (var label in _statusTexts)
            {
                var type = label.type;
                var baseVal = baseStatus.GetValueOrDefault(type, 0f);
                var equip = equipStatus.GetValueOrDefault(type, 0f);

                label.SetText(baseVal, equip);
            }

            _atkElementLv.text = $"LV.{elemental.atkLv}";
            bool hasAtkType = elemental.atkType != Elemental.Type.None;
            if (hasAtkType)
            {
                _atkElementRoot.SetActive(true);
                _atkElement.Set(ElementalIcons[elemental.atkType]).Forget();
            }
            else
                _atkElementRoot.SetActive(false);
            
            _defElementLv.text = $"LV.{elemental.defLv}";
            bool hasDefType = elemental.defType != Elemental.Type.None;
            if (hasDefType)
            {
                _defElementRoot.SetActive(true);
                _defElement.Set(ElementalIcons[elemental.defType]).Forget();
            }
            else
                _defElementRoot.SetActive(false);
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

        private void OnLevelUp() => Context.popups.Open<PopupLevelUp>(_unit.Value);

        private void OnUpgrade() => Context.popups.Open<PopupRateUp>(_unit.Value);
    }
}