using System;
using System.Collections.Generic;
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
            _dayOfWeek.Value = NetworkService.CurrentTime().DayOfWeek;
            
            return UniTask.CompletedTask;
        }

        private async void UpdateUI(DayOfWeek value)
        {
            
            
            for (var dow = DayOfWeek.Sunday; dow <= DayOfWeek.Saturday; ++dow)
            {
                int index = (int)dow;
                _dayOfWeeks[index].state.Value = value == dow
                    ? UISlot.State.Highlighted
                    : UISlot.State.Default;
            }
        }
    }
}