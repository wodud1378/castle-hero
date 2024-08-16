using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Stage.UI;
using UnityEngine;

namespace RGLabs.Lobby.UI
{
    public class UIDungeonSlot : UISlot
    {
        [SerializeField] private UIRewardList _rewardList;
        
        public UniTask Init(DungeonEntity entity)
        {
            return base.Init(entity.image, entity.OpenDaysOfWeekText());
        }
    }
}