using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Network.Service;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Dungeon.prefab")]
    public class PopupDungeon : PopupBase
    {
        [SerializeField] private UISlot[] _dayOfWeeks;
        
        private readonly ReactiveProperty<DayOfWeek> _dayOfWeek = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            _dayOfWeek
                .Subscribe(UpdateUI)
                .AddTo(this);
        }

        public override UniTask Open()
        {
            var currentTime = NetworkService.CurrentTime();
            
            
            return base.Open();
        }

        private void UpdateUI(DayOfWeek dow)
        {
        }
    }
}