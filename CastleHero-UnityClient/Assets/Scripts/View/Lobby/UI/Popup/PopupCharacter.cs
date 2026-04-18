using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.UI;
using CastleHero.View.Lobby.UI.Actions;
using CastleHero.View.Lobby.UI.Inventory;
using CastleHero.View.Lobby.UI.Inventory.Actions;
using CastleHero.View.Lobby.UI.Inventory.Popup;
using CastleHero.Network.Shared;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_CharInfo.prefab")]
    public class PopupCharacter : PopupBase
    {
        private static readonly Dictionary<ElementalType, string> ElementalIcons = new()
        {
            { ElementalType.None, "Sprites/Global/UI/Symbol_Elemental_Earth.png" },
            { ElementalType.Earth, "Sprites/Global/UI/Symbol_Elemental_Earth.png" },
            { ElementalType.Fire, "Sprites/Global/UI/Symbol_Elemental_Fire.png" },
            { ElementalType.Water, "Sprites/Global/UI/Symbol_Elemental_Water.png" },
            { ElementalType.Wind, "Sprites/Global/UI/Symbol_Elemental_Wind.png" },
        };

        [FormerlySerializedAs("_name")]
        [SerializeField] private TMP_Text name;
        [FormerlySerializedAs("_lv")]
        [SerializeField] private TMP_Text lv;
        [FormerlySerializedAs("_stars")]
        [SerializeField] private GameObject[] stars;
        [FormerlySerializedAs("_prefabRoot")]
        [SerializeField] private RectTransform prefabRoot;
        [FormerlySerializedAs("_level")]
        [SerializeField] private UILevel level;
        [FormerlySerializedAs("_statusTexts")]
        [SerializeField] private UIStatusText[] statusTexts;
        [FormerlySerializedAs("_emptySlotButtons")]
        [SerializeField] private List<Button> emptySlotButtons;
        [FormerlySerializedAs("_equipments")]
        [SerializeField] private List<UIEquipmentSlot> equipments;

        [FormerlySerializedAs("_atkElementRoot")]
        [SerializeField] private GameObject atkElementRoot;
        [FormerlySerializedAs("_atkElement")]
        [SerializeField] private AddressableImage atkElement;
        [FormerlySerializedAs("_atkElementLv")]
        [SerializeField] private TMP_Text atkElementLv;

        [FormerlySerializedAs("_defElementRoot")]
        [SerializeField] private GameObject defElementRoot;
        [FormerlySerializedAs("_defElement")]
        [SerializeField] private AddressableImage defElement;
        [FormerlySerializedAs("_defElementLv")]
        [SerializeField] private TMP_Text defElementLv;

        [FormerlySerializedAs("_levelUp")]
        [SerializeField] private Button levelUp;
        [FormerlySerializedAs("_upgrade")]
        [SerializeField] private Button upgrade;

        private readonly ReactiveProperty<UnitInfo> _unit = new();
        private GameObject _character;

        private IUserRepository _userRepo;
        private IDBProvider _db;
        private IPopupManager _popups;
        private EquipAction _equipAction;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _userRepo = sl.Get<IUserRepository>();
            _db = sl.Get<IDBProvider>();
            _popups = sl.Get<IPopupManager>();
            _equipAction = sl.Get<EquipAction>();

            this.SubscribeButton(levelUp, OnLevelUp);
            this.SubscribeButton(upgrade, OnUpgrade);

            _userRepo.Characters
                .WhenUpdate(_unit, x => _unit.Value = x)
                .AddTo(this);

            _unit
                .Subscribe(OnUnitChanged)
                .AddTo(this);

            for (var slot = EquipmentSlot.Weapon; slot <= EquipmentSlot.Necklace; ++slot)
            {
                var inner = slot;
                this.SubscribeButton(emptySlotButtons[(int)slot], () => Equip(inner));
            }

            equipments.ForEach(x =>
                x.OnClick += s =>
                {
                    if (s is UIEquipmentSlot { Item: not null } equipmentSlot)
                    {
                        _popups.Open<PopupEquipItem>(equipmentSlot.Item);
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

            if (!_db.Units.TryFind(_unit.Value.id, out var unitEntity))
                return;

            if (!_db.Balances.TryFind(_unit.Value.id, out var balanceEntity))
                return;

            name.text = unitEntity.name;
            level.Set(_unit.Value);

            int lvVal = _unit.Value.lv;
            int rate = _unit.Value.rate;

            SetCharacter(unitEntity.uiPrefab).SafeForget();
            UpdateRate(rate);

            var equipmentsData = _userRepo.EquipItems(info.equipments).ToList();

            UpdateUI(lvVal, rate, unitEntity, balanceEntity, equipmentsData);
            UpdateEquipmentSlots(equipmentsData);
        }

        private async UniTask SetCharacter(string dataPath)
        {
            if (_character != null)
                Addressables.ReleaseInstance(_character);

            prefabRoot.gameObject.SetActive(false);

            _character = await Addressables.InstantiateAsync(dataPath, prefabRoot);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            prefabRoot.gameObject.SetActive(true);
        }

        private void UpdateRate(int rate)
        {
            for (int i = 0, length = stars.Length; i < length; ++i)
            {
                stars[i].SetActive((i + 1) <= rate);
            }
        }

        private async UniTask Equip(EquipmentSlot slot)
        {
            var items = _userRepo.Inventory.Items
                .OfType<EquipItem>()
                .Where(x => x.slot == (int)slot);

            var selection = await _popups.OpenAsync<PopupSelectItem>(items);

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
                        var compare = await _popups.OpenAsync<PopupCompareEquipment>(_unit.Value, e);
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

            await _equipAction.Equip(_unit.Value.id, equipItem.Guid);
        }

        private void UpdateEquipmentSlots(List<EquipItem> equipmentsData)
        {
            if (equipmentsData == null)
                return;

            for (var slot = EquipmentSlot.Weapon; slot <= EquipmentSlot.Necklace; ++slot)
            {
                int index = (int)slot;
                var item = equipmentsData.Find(x => x.slot == index);
                var ui = equipments[index];
                if (item == null)
                    ui.gameObject.SetActive(false);
                else
                {
                    ui.gameObject.SetActive(true);
                    ui.Init(item);
                }
            }
        }

        private void UpdateUI(int lvVal, int rate, UnitEntity unit, UnitBalanceEntity balance,
            List<EquipItem> equipmentsData)
        {
            var elemental = new Elemental();
            var baseStatus = CharacterStatusHelper.BaseStatus(lvVal, rate, unit, balance);
            var equipStatus = equipmentsData.Total(ref elemental);
            foreach (var label in statusTexts)
            {
                var type = label.type;
                var baseVal = baseStatus.GetValueOrDefault(type, 0f);
                var equip = equipStatus.GetValueOrDefault(type, 0f);

                label.SetText(baseVal, equip);
            }

            atkElementLv.text = $"LV.{elemental.atkLv}";
            bool hasAtkType = elemental.atkType != ElementalType.None;
            if (hasAtkType)
            {
                atkElementRoot.SetActive(true);
                atkElement.Set(ElementalIcons[elemental.atkType]).SafeForget();
            }
            else
                atkElementRoot.SetActive(false);

            defElementLv.text = $"LV.{elemental.defLv}";
            bool hasDefType = elemental.defType != ElementalType.None;
            if (hasDefType)
            {
                defElementRoot.SetActive(true);
                defElement.Set(ElementalIcons[elemental.defType]).SafeForget();
            }
            else
                defElementRoot.SetActive(false);
        }

        private void OnLevelUp() => _popups.Open<PopupLevelUp>(_unit.Value);

        private void OnUpgrade() => _popups.Open<PopupRateUp>(_unit.Value);
    }
}
