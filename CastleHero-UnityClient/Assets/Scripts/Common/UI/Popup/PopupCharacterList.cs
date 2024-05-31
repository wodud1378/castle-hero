using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.InGame.UI;
using RGLabs.Network.Model;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace RGLabs.Common.UI.Popup
{
    public class PopupCharacterList : PopupBase
    {
        public enum Tab
        {
            Storage,
            Collections,
        }
        
        private enum SortOption
        {
            Id,
            HigherLevel,
            LowerLevel,
            HigherRate,
            LowerRate
        }

        private static readonly Dictionary<SortOption, IComparer<UnitInfo>> Comparers =
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
        [SerializeField] private UICharacterList _characterList;
        
        public readonly ReactiveProperty<Tab> tab = new();
        
        private readonly ReactiveProperty<SortOption> _sortOption = new();
        
        public override UniTask Open()
        {
            base.Open();

            Storage.userRepository.characters
                .ChangeAsObservable()
                .Skip(1)
                .Subscribe(OnCharacterCollectionChanged)
                .AddTo(this);

            _sortOption
                .Skip(1)
                .Subscribe(OnSortOptionChanged)
                .AddTo(this);
            
            tab
                .DistinctUntilChanged()
                .Subscribe(OnTabChanged)
                .AddTo(this);

            return UniTask.CompletedTask;
        }

        private void OnTabChanged(Tab value)
        {
            if (value == Tab.Storage)
            {
                UpdateList(Storage.userRepository.characters.ToArray(), _sortOption.Value);
                return;
            }

            var db = Storage.db.units;
            var characters = db
                .Where(x => x.Id / 10000 == 1)
                .Select(x => new UnitInfo { id = x.Id })
                .ToArray();
            
            UpdateList(characters, SortOption.Id);
        }

        private void OnCharacterCollectionChanged(IEnumerable<UnitInfo> characters)
        {
            if (tab.Value == Tab.Collections)
                return;
            
            UpdateList(characters.ToArray(), _sortOption.Value);
        }
        
        private void OnSortOptionChanged(SortOption option)
        {
            UpdateList(Storage.userRepository.characters.ToArray(), option);
        }

        private async void UpdateList(UnitInfo[] characters, SortOption option)
        {
            Array.Sort(characters, Comparers[option]);

            await _characterList.Init(characters);
        }
    }
}