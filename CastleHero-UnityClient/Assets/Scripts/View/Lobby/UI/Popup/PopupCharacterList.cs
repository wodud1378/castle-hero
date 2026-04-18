using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.View.Lobby.UI.Inventory.Popup;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Character.prefab")]
    public class PopupCharacterList : PopupBase, ISelect<UnitInfo>
    {
        public struct EquipParam
        {
            public EquipItem item;
            public UnitInfo unit;
        }

        public enum ClickMethod
        {
            Select,
            Equip,
        }

        public enum Tab
        {
            Storage,
            Collections,
        }

        public enum SortOption
        {
            Id = -1,
            HigherLevel,
            LowerLevel,
            HigherRate,
            LowerRate
        }

        private static readonly Dictionary<SortOption, IComparer<UnitInfo>> Comparer =
            new()
            {
                { SortOption.Id, new IdDescendingComparer() },
                { SortOption.HigherLevel, new LvDescendingComparer() },
                { SortOption.LowerLevel, new LvAscendingComparer() },
                { SortOption.HigherRate, new RateDescendingComparer() },
                { SortOption.LowerRate, new RateAscendingComparer() },
            };

        [FormerlySerializedAs("_sortOptions")]
        [SerializeField] private TMP_Dropdown sortOptions;
        [FormerlySerializedAs("_groups")]
        [SerializeField] private TMP_Dropdown groups;
        [FormerlySerializedAs("_tabToggle")]
        [SerializeField] private ToggleGroup tabToggle;
        [FormerlySerializedAs("_characterList")]
        [SerializeField] private UICharacterList characterList;

        private UnitInfo[] Characters
        {
            get
            {
                switch (tab.Value)
                {
                    case Tab.Storage: return _userRepo.Characters.Units
                        .Where(x => x.id != Constants.BarricadeId)
                        .ToArray();
                    case Tab.Collections:
                        return _db.Units
                            .Where(x => x.Id / 10000 == 1)
                            .Select(x => new UnitInfo { id = x.Id })
                            .ToArray();
                }

                return null;
            }
        }

        private IComparer<UnitInfo> Sort
        {
            get
            {
                if (tab.Value == Tab.Collections)
                    return Comparer[SortOption.Id];

                return Comparer[sortOption.Value];
            }
        }

        public readonly ReactiveProperty<Tab> tab = new();
        public readonly ReactiveProperty<SortOption> sortOption = new();

        [NonSerialized]
        public ClickMethod clickMethod;

        public EquipParam equipParam;

        private IUserRepository _userRepo;
        private IDBProvider _db;
        private IPopupManager _popups;

        public UniTask<UnitInfo> SelectTask => _ctSource.Task;

        private UniTaskCompletionSource<UnitInfo> _ctSource;
        private bool _closeAfterSelect;

        public void BeginSelect(bool closeAfterSelect)
        {
            _ctSource = new UniTaskCompletionSource<UnitInfo>();
            _closeAfterSelect = closeAfterSelect;
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _userRepo = sl.Get<IUserRepository>();
            _db = sl.Get<IDBProvider>();
            _popups = sl.Get<IPopupManager>();

            this.UpdateAsObservable()
                .Select(_ => tabToggle.ActiveToggles().FirstOrDefault(t => t.isOn))
                .DistinctUntilChanged()
                .Select(toggle => Enum.Parse<Tab>(toggle.gameObject.name))
                .Subscribe(selected => tab.Value = selected)
                .AddTo(this);

            sortOptions.onValueChanged
                .AsObservable()
                .Subscribe(x => sortOption.Value = (SortOption)x)
                .AddTo(this);

            tab.CombineLatest(sortOption, (t, s) => (t, s))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateUI())
                .AddTo(this);

            _userRepo.Characters.Units
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateUI())
                .AddTo(this);

            characterList.OnSlotClickEvent -= OnClickSlot;
            characterList.OnSlotClickEvent += OnClickSlot;
        }

        public override UniTask Open() => Open(Tab.Storage, SortOption.HigherLevel);

        public override UniTask Open(params object[] parameters)
        {
            Tab tabParam;
            SortOption sortParam;

            try { tabParam = (Tab)parameters[0]; }
            catch { tabParam = default; }

            try { sortParam = (SortOption)parameters[1]; }
            catch { sortParam = SortOption.HigherLevel; }

            tab.Value = tabParam;
            sortOption.Value = sortParam;

            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            base.OnClose();

            TrySelectComplete(null);
        }

        private void UpdateUI()
        {
            var characters = Characters;
            Array.Sort(characters, Sort);

            characterList.Init(characters);
        }

        private void OnClickSlot(UICharacterSlot slot)
        {
            if (TrySelectComplete(slot.Info))
            {
                if (_closeAfterSelect)
                    Close();

                return;
            }

            switch (clickMethod)
            {
                case ClickMethod.Select:
                    _popups.Open<PopupCharacter>(slot.Info);
                    break;
                case ClickMethod.Equip:
                    OpenEquipmentCompare(slot.Info);
                    break;
            }
        }

        private bool TrySelectComplete(UnitInfo unit)
        {
            if (_ctSource == null)
                return false;

            _ctSource.TrySetResult(unit);
            _ctSource = null;
            return true;
        }

        private void OpenEquipmentCompare(UnitInfo unit)
        {
            equipParam.unit = unit;
            if (equipParam.item == null || equipParam.unit == null)
                return;

            _popups.Open<PopupCompareEquipment>(equipParam.unit, equipParam.item);
            Close();

            equipParam.item = null;
            equipParam.unit = null;
        }
    }
}
