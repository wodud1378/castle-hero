using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Network.Service;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Dungeon.prefab")]
    public class PopupDungeon : PopupBase
    {
        [SerializeField] private UIDayOfWeek[] _dayOfWeeks;
        [SerializeField] private UIDungeonList _dungeonList;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private UIDungeonSelect _select;

        private readonly ReactiveProperty<DayOfWeek> _dayOfWeek = new();
        private UniTask _updateTask;

        private void Start()
        {
            for (var dow = DayOfWeek.Sunday; dow <= DayOfWeek.Saturday; ++dow)
            {
                _dayOfWeeks[(int)dow].values.Update(new[] { dow });
            }

            _dungeonList.OnSlotClickEvent += OnClickSlot;
            _dayOfWeek
                .Subscribe(UpdateUI)
                .AddTo(this);
        }

        private async void OnClickSlot(UIDungeonSlot slot)
        {
            if (slot.state.Value == UIState.State.Dim)
                return;

            if (_select.IsOpened)
            {
                _select.Close();
                
                if (slot.Type == _select.Type && slot.DetailType == _select.DetailType)
                    return;
            }
            
            _select.Open(slot.Type, slot.DetailType);
            var rectTr = (_select.transform as RectTransform)!;
            rectTr.SetSiblingIndex(slot.transform.GetSiblingIndex() + 1);
            Reposition(rectTr);

            var selected = await _select.SelectTask;
            if (!selected.IsValid)
                return;

            Storage.userRepository.entrance.Value = new GameEntrance
            {
                type = GameType.Dungeon,
                id = selected.Id
            };

            Context.Transition.CurrentState = State.Prepare;
            
            Close();
        }

        private void Reposition(RectTransform targetItem)
        {
            Vector2 contentSize = _scroll.content.rect.size;
            Vector2 viewportSize = _scroll.viewport.rect.size;

            // 타겟 아이템의 로컬 좌표에서 Content 기준의 위치를 계산
            Vector3 itemLocalPosition = targetItem.localPosition;
            
            // 타겟 아이템이 중앙에 오도록 스크롤 뷰의 normalizedPosition을 계산
            _scroll.verticalNormalizedPosition = Mathf.Clamp01(1 - (itemLocalPosition.y + (targetItem.rect.height / 2)) / (contentSize.y - viewportSize.y));
        }

        public override UniTask Open()
        {
            _dayOfWeek.Value = NetworkService.CurrentTimeByLocal().DayOfWeek;

            return _updateTask;
        }

        private void UpdateUI(DayOfWeek value)
        {
            SetHighlightDayOfWeek(value);

            _updateTask = UpdateList(value);
        }

        private void SetHighlightDayOfWeek(DayOfWeek value)
        {
            for (var dow = DayOfWeek.Sunday; dow <= DayOfWeek.Saturday; ++dow)
            {
                int index = (int)dow;
                _dayOfWeeks[index].state.Value = value == dow
                    ? UIState.State.Highlighted
                    : UIState.State.Default;
            }
        }

        private UniTask UpdateList(DayOfWeek dow)
        {
            var hashSet = new HashSet<(DungeonType main, DungeonDetailType sub)>();
            Storage.db.dungeons.ForEach(x => hashSet.Add((x.type, x.detailType)));

            var list = hashSet.ToList();
            var db = Storage.db.dungeons;
            list.Sort((x, y) =>
            {
                var openDaysX = db.Where(e => e.type == x.main).First().OpenDaysOfWeek();
                var openDaysY = db.Where(e => e.type == y.main).First().OpenDaysOfWeek();
                int compareDow = (openDaysX.Contains(dow) ? 0 : 1).CompareTo(openDaysY.Contains(dow) ? 0 : 1);
                if (compareDow != 0)
                    return compareDow;

                var compareMain = x.main.CompareTo(y.main);
                if (compareMain != 0)
                    return compareMain;

                return x.sub.CompareTo(y.sub);
            });

            return _dungeonList.Init(list);
        }
    }
}