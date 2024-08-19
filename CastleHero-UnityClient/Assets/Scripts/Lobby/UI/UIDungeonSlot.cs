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
        
        public UniTask Init(DungeonEntity entity)
        {
            Entity = entity;
            
            _dayOfWeek.values.Update(entity.OpenDaysOfWeek());
            
            return base.Init(entity.image);
        }
    }
}