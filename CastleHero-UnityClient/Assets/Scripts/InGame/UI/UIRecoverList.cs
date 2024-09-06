using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.InGame.System;
using RGLabs.Utility;
using UniRx;

namespace RGLabs.InGame.UI
{
    public class UIRecoverList : UIListAdapter<UIRecoverSlot, WaitRecover>
    {
        public void Init()
        {
            var recovers = Storage.inGameRepository.recovers;
            recovers
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x => base.Init(x).Forget())
                .AddTo(this);
        }
        
        protected override UniTask SetItem(UIRecoverSlot slot, WaitRecover data) => slot.InitAsync(data);
    }
}