using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.UI
{
    public class UIDungeonSlot : UISlot
    {
        [SerializeField] private UIDayOfWeek _dayOfWeek;
        //[SerializeField] private UIRewardList _rewardList;
        
        public DungeonType Type { get; private set; }
        public DungeonDetailType DetailType { get; private set; }

        public UniTask Init(DungeonType type, DungeonDetailType detailType)
        {
            Type = type;
            DetailType = detailType;

            var data = Storage.db.dungeons.Where(x => x.type == type).First();
            //var openDays = data.OpenDaysOfWeek();
            var openDays = new List<DayOfWeek>
            {
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
            };
            
            var dow = NetworkService.CurrentTimeByLocal().DayOfWeek;

            bool isOpened = openDays.Contains(dow);
            state.Value = isOpened
                ? State.Default
                : State.Dim;

            _dayOfWeek.values.Update(openDays);
            
            return base.Init(data.image);
            // var baseTask = base.Init(entity.image);
            // if (initialState == State.Dim)
            // {
            //     _rewardList.gameObject.SetActive(false);
            //     return baseTask;
            // }
            //
            // _rewardList.gameObject.SetActive(true);
            // var rewardTask = _rewardList.Init(entity);
            //
            // return UniTask.WhenAll(baseTask, rewardTask);
        }
    }
}