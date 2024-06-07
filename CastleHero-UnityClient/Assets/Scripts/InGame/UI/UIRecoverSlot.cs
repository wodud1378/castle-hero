using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.InGame.System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIRecoverSlot : UIItemSlot
    {
        [SerializeField] private Image _gauge;
        
        public async UniTask InitAsync(WaitRecover data)
        {
            await base.InitAsync(data.behaviour.Data.icon, string.Empty, default);

            data.summary
                .Subscribe(OnUpdate)
                .AddTo(this);
        }

        private void OnUpdate((float left, float total) summary) => _gauge.fillAmount = summary.left / summary.total;
    }
}