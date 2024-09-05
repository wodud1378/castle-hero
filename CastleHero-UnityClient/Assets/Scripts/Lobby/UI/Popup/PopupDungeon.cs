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

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Dungeon.prefab")]
    public class PopupDungeon : PopupBase
    {
        [SerializeField] private UIDayOfWeek[] _dayOfWeeks;
        [SerializeField] private UIDungeonList _dungeonList;

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

        private void OnClickSlot(UIDungeonSlot slot)
        {
            if (slot.state.Value == UIState.State.Dim)
                return;

            Storage.userRepository.entrance.Value = new GameEntrance
            {
                type = GameType.Dungeon,
                id = slot.Entity.Id
            };

            Context.Transition.CurrentState = State.Prepare;
            
            CloseAsync()
                .Forget();
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
            var dungeonRecords = Storage.userRepository.gameRecord.dungeon;
            var db = Storage.db.dungeons;

            var list = new List<DungeonEntity>();
            for (var t = DungeonType.Assault; t <= DungeonType.Invasion; ++t)
            {
                var type = t;
                var record = dungeonRecords.FirstOrDefault(x => x.type == (int)type);

                var byType = db.FindAll(x => x.type == type);
                byType.Sort((x, y) => x.lv.CompareTo(y.lv));

                int lv = record != null
                    ? Mathf.Min(record.lv + 1, byType[^1].lv)
                    : 1;

                var index = byType.FindIndex(x => x.lv == lv);
                if (index.IsValidIndex(byType))
                    list.Add(byType[index]);
            }

            list.Sort((x, y) =>
            {
                var openDaysX = x.OpenDaysOfWeek();
                var openDaysY = y.OpenDaysOfWeek();
                int compareDow = (openDaysX.Contains(dow) ? 0 : 1).CompareTo(openDaysY.Contains(dow) ? 0 : 1);
                if (compareDow != 0)
                    return compareDow;

                return x.type.CompareTo(y.type);
            });

            return _dungeonList.Init(list);
        }
    }
}