using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.Data.Model;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Network;
namespace CastleHero.View.Lobby.UI
{
    public class UIDungeonSlot : UISlot
    {
        [FormerlySerializedAs("_dayOfWeek")]
        [SerializeField] private UIDayOfWeek dayOfWeek;
        //[SerializeField] private UIRewardList _rewardList;

        public DungeonEntity Entity { get; private set; }

        public UniTask Init(DungeonEntity entity)
        {
            Entity = entity;

            bool isOpened = entity.IsOpened(out var openDays);

#if UNITY_EDITOR
            if (NetworkConfig.Current != null && NetworkConfig.Current.openAllDungeons)
                isOpened = true;
#endif

            state.Value = isOpened
                ? State.Default
                : State.Dim;

            dayOfWeek.values.Update(openDays);

            return base.Init(Entity.image);
        }
    }
}
