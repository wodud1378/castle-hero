using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Lobby.UI.Inventory.Popup;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
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

        [SerializeField] private TMP_Dropdown _sortOptions;
        [SerializeField] private TMP_Dropdown _groups;
        [SerializeField] private ToggleGroup _tabToggle;
        [SerializeField] private UICharacterList _characterList;
        
        private UnitInfo[] Characters
        {
            get
            {
                switch (tab.Value)
                {
                    case Tab.Storage: return Storage.userRepository.characters.units
                        .Where(x=> x.id != Constants.BarricadeId)
                        .ToArray();
                    case Tab.Collections:
                        return Storage.db.units
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

        public ClickMethod clickMethod;

        public EquipParam equipParam;
        
        private UniTask _updateTask;
        
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
            
            this.UpdateAsObservable()
                .Select(_ => _tabToggle.ActiveToggles().FirstOrDefault(t => t.isOn))
                .DistinctUntilChanged()
                .Select(toggle => Enum.Parse<Tab>(toggle.gameObject.name))
                .Subscribe(selected => tab.Value = selected)
                .AddTo(this);

            _sortOptions.onValueChanged
                .AsObservable()
                .Subscribe(x => sortOption.Value = (SortOption)x)
                .AddTo(this);
            
            tab.CombineLatest(sortOption, (t, s) => (t, s))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateUI())
                .AddTo(this);
            
            Storage.userRepository.characters.units
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(_=> UpdateUI())
                .AddTo(this);

            _characterList.OnSlotClickEvent -= OnClickSlot;
            _characterList.OnSlotClickEvent += OnClickSlot;
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

            return _updateTask;
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

            _updateTask =_characterList.Init(characters);
        }
        
        private void OnClickSlot(UICharacterSlot slot)
        {
            if (TrySelectComplete(slot.Info))
            {
                if(_closeAfterSelect)
                    Close();
                
                return;
            }
            
            switch (clickMethod)
            {
                case ClickMethod.Select:
                    Context.popups.Open<PopupCharacter>(slot.Info);
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
            
            Context.popups.Open<PopupCompareEquipment>(equipParam.unit, equipParam.item);
            Close();

            equipParam.item = null;
            equipParam.unit = null;
        }
    }
}