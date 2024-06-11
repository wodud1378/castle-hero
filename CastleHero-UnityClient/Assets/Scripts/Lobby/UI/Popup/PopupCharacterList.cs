using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Network.Model;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Character.prefab")]
    public class PopupCharacterList : PopupBase
    {
        public enum Tab
        {
            Storage,
            Collections,
        }

        public enum SortOption
        {
            Id,
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
                    case Tab.Storage: return Storage.userRepository.characters
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

                return Comparer[_sortOption.Value];
            }
        }
        
        public readonly ReactiveProperty<Tab> tab = new();

        private readonly ReactiveProperty<SortOption> _sortOption = new();
        private UniTask _updateTask;
        
        protected override void InitSubscriptions()
        {
            base.InitSubscriptions();
            
            this.UpdateAsObservable()
                .Select(_ => _tabToggle.ActiveToggles().FirstOrDefault(t => t.isOn))
                .DistinctUntilChanged()
                .Select(toggle => Enum.Parse<Tab>(toggle.gameObject.name))
                .Subscribe(selected => tab.Value = selected)
                .AddTo(this);
            
            tab.CombineLatest(_sortOption, (t, s) => (t, s))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateUI())
                .AddTo(this);
        }

        public override UniTask Open() => OpenTask(Tab.Storage, SortOption.HigherLevel);

        public override UniTask OpenTask(params object[] parameters)
        {
            Tab tabParam;
            SortOption sortParam;
            
            try { tabParam = (Tab)parameters[0]; }
            catch { tabParam = default; }

            try { sortParam = (SortOption)parameters[1]; }
            catch { sortParam = SortOption.HigherLevel; }

            tab.Value = tabParam;
            _sortOption.Value = sortParam;

            return _updateTask;
        }

        private void UpdateUI()
        { 
            var characters = Characters;
            Array.Sort(characters, Sort);

            _updateTask =_characterList.Init(characters);
        }
    }
}