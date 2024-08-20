using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Prepare.UI;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.UI
{
    public class UIDungeonSlot : UISlot
    {
        [SerializeField] private UIDayOfWeek _dayOfWeek;
        [SerializeField] private UIRewardList _rewardList;

        public DungeonEntity Entity { get; private set; }

        public UniTask Init(DungeonEntity entity, State initialState)
        {
            Entity = entity;

            state.Value = initialState;

            _dayOfWeek.values.Update(entity.OpenDaysOfWeek());

            var baseTask = base.Init(entity.image);
            if (initialState == State.Dim)
            {
                _rewardList.gameObject.SetActive(false);
                return baseTask;
            }

            _rewardList.gameObject.SetActive(true);
            var rewardTask = _rewardList.Init(entity);

            return UniTask.WhenAll(baseTask, rewardTask);
        }
    }
}