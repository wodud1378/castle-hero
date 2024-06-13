using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.InGame.System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIRecoverSlot : UISlot
    {
        [SerializeField] private Image _gauge;
        
        public UniTask InitAsync(WaitRecover data)
        {
            data.summary
                .Subscribe(OnUpdate)
                .AddTo(this);
            
            return Init(data.behaviour.Data.icon);
        }

        private void OnUpdate((float left, float total) summary) => _gauge.fillAmount = summary.left / summary.total;
    }
}