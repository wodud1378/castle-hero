using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Dungeon.prefab")]
    public class PopupDungeon : PopupBase
    {
        [FormerlySerializedAs("_dayOfWeeks")]
        [SerializeField] private UIDayOfWeek[] dayOfWeeks;
        [FormerlySerializedAs("_dungeonList")]
        [SerializeField] private UIDungeonList dungeonList;
        [FormerlySerializedAs("_scroll")]
        [SerializeField] private ScrollRect scroll;
        [FormerlySerializedAs("_select")]
        [SerializeField] private UIDungeonSelect select;

        private readonly ReactiveProperty<DayOfWeek> _dayOfWeek = new();

        private IUserRepository _userRepo;
        private IDBProvider _db;
        private StateManager<LobbyState> _lobbyState;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _userRepo = sl.Get<IUserRepository>();
            _db = sl.Get<IDBProvider>();
            _lobbyState = sl.Get<StateManager<LobbyState>>();
        }

        private void Start()
        {
            for (var dow = DayOfWeek.Sunday; dow <= DayOfWeek.Saturday; ++dow)
            {
                dayOfWeeks[(int)dow].values.Update(new[] { dow });
            }

            dungeonList.OnSlotClickEvent += s => OnClickSlot(s).SafeForget();
            _dayOfWeek
                .Subscribe(UpdateUI)
                .AddTo(this);
        }

        private async UniTask OnClickSlot(UIDungeonSlot slot)
        {
            if (slot.state.Value == UIState.State.Dim)
                return;

            if (select.IsOpened)
            {
                select.Close();

                if (slot.Entity.Layer == select.Entity.Layer)
                    return;
            }

            select.Open(slot.Entity.Layer);
            var rectTr = (select.transform as RectTransform)!;
            rectTr.SetSiblingIndex(slot.transform.GetSiblingIndex() + 1);
            //Reposition(rectTr);

            var selected = await select.SelectTask;
            if (!selected.IsValid)
                return;

            _userRepo.Entrance.Value = new GameEntrance
            {
                type = GameType.Dungeon,
                id = selected.Id
            };

            _lobbyState.CurrentState = LobbyState.Prepare;

            Close();
        }

        private void Reposition(RectTransform targetItem)
        {
            Vector2 contentSize = scroll.content.rect.size;
            Vector2 viewportSize = scroll.viewport.rect.size;

            // 타겟 아이템의 로컬 좌표에서 Content 기준의 위치를 계산
            Vector3 itemLocalPosition = targetItem.localPosition;

            // 타겟 아이템이 중앙에 오도록 스크롤 뷰의 normalizedPosition을 계산
            scroll.verticalNormalizedPosition = Mathf.Clamp01(1 - (itemLocalPosition.y + (targetItem.rect.height / 2)) / (contentSize.y - viewportSize.y));
        }

        public override UniTask Open()
        {
            _dayOfWeek.Value = ServerTime.Now.DayOfWeek;

            return UniTask.CompletedTask;
        }

        private void UpdateUI(DayOfWeek value)
        {
            SetHighlightDayOfWeek(value);

            UpdateList(value);
        }

        private void SetHighlightDayOfWeek(DayOfWeek value)
        {
            for (var dow = DayOfWeek.Sunday; dow <= DayOfWeek.Saturday; ++dow)
            {
                int index = (int)dow;
                dayOfWeeks[index].state.Value = value == dow
                    ? UIState.State.Highlighted
                    : UIState.State.Default;
            }
        }

        private void UpdateList(DayOfWeek dow)
        {
            var list = new List<DungeonEntity>();
            _db.Dungeons.BinarySearch(x =>
            {
                if (list.FindIndex(exist => exist.Layer == x.Layer).IsValidIndex(list))
                    return;

                list.Add(x);
            });

            list.Sort((x, y) =>
            {
                var openDaysX = x.OpenDaysOfWeek();
                var openDaysY = y.OpenDaysOfWeek();
                int compareDow = (openDaysX.Contains(dow) ? 0 : 1).CompareTo(openDaysY.Contains(dow) ? 0 : 1);
                return compareDow != 0
                    ? compareDow
                    : x.Layer.CompareTo(y.Layer);
            });

            dungeonList.Init(list);
        }
    }
}
