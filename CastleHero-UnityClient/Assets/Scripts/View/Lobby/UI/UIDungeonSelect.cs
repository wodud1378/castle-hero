using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.Prepare.UI;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI
{
    public class UIDungeonSelect : MonoBehaviour, ISelect<DungeonEntity>
    {
        [FormerlySerializedAs("_prev")]
        [SerializeField] private Button prev;
        [FormerlySerializedAs("_next")]
        [SerializeField] private Button next;
        [FormerlySerializedAs("_confirm")]
        [SerializeField] private Button confirm;
        [FormerlySerializedAs("_lv")]
        [SerializeField] private TMP_Text lv;
        [FormerlySerializedAs("_rewardList")]
        [SerializeField] private UIRewardList rewardList;

        public UniTask<DungeonEntity> SelectTask => _ctSource.Task;

        private UniTaskCompletionSource<DungeonEntity> _ctSource;

        public DungeonEntity Entity => _entity.Value;

        public bool IsOpened => gameObject.activeSelf;

        private int AvailableLv
        {
            get
            {
                var record = ServiceLocator.Get<IUserRepository>().GameRecord.Dungeon
                    .FirstOrDefault(x => x.layer == _entity.Value.Layer);

                return record != null
                    ? Mathf.Min(record.lastClearedLv + 1, _listOfSameType[^1].Lv)
                    : 1;
            }
        }

        private readonly ReactiveProperty<DungeonEntity> _entity = new();

        private List<DungeonEntity> _listOfSameType;

        private bool _closeAfterSelect;

        private void Awake()
        {
            this.SubscribeButton(confirm, Confirm);
            this.SubscribeButton(prev, OnPrev);
            this.SubscribeButton(next, OnNext);

            _entity
                .Skip(1)
                .Subscribe(OnDataChanged)
                .AddTo(this);

            gameObject.SetActive(false);
        }

        private void OnDataChanged(DungeonEntity entity)
        {
            if (!entity.IsValid)
                return;

            lv.text = $"LV.{entity.lv}";

            int index = _listOfSameType.FindIndex(x => x.lv == entity.lv);
            prev.gameObject.SetActive(index > 0);
            next.gameObject.SetActive(index < _listOfSameType.Count - 1 && _listOfSameType[index + 1].lv <= AvailableLv);

            rewardList.Init(entity.GetRewardsForDisplay()).Forget();
        }

        private void Confirm()
        {
            if (!TrySelectComplete(_entity.Value))
                return;

            if (_closeAfterSelect)
                Close();
        }

        private void OnPrev() => AdjustIndex(-1);

        private void OnNext() => AdjustIndex(1);

        private void AdjustIndex(int adjust)
        {
            int index = _listOfSameType.FindIndex(x => x.lv == _entity.Value.lv) + adjust;
            if (!index.IsValidIndex(_listOfSameType))
                return;

            _entity.Value = _listOfSameType[index];
        }

        public void BeginSelect(bool closeAfterSelect)
        {
            _ctSource = new UniTaskCompletionSource<DungeonEntity>();
            _closeAfterSelect = closeAfterSelect;
        }

        public void Open(int layer)
        {
            gameObject.SetActive(true);

            BeginSelect(true);

            _listOfSameType = ServiceLocator.Get<IDBProvider>().Dungeons.FindAll(x => x.Layer == layer);
            _listOfSameType.Sort((x, y) => x.lv.CompareTo(y.lv));

            _entity.Value = _listOfSameType.Find(x => x.lv == AvailableLv);
        }

        public void Close()
        {
            TrySelectComplete(ServiceLocator.Get<IDBProvider>().Dungeons.FallBackEntity());

            gameObject.SetActive(false);
        }

        private bool TrySelectComplete(DungeonEntity entity)
        {
            if (_ctSource == null)
                return false;

            _ctSource.TrySetResult(entity);
            _ctSource = null;

            return true;
        }
    }
}
