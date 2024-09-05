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
            var openDays = data.OpenDaysOfWeek();
            var dow = NetworkService.CurrentTimeByLocal().DayOfWeek;
            
            state.Value = openDays.Contains(dow)
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