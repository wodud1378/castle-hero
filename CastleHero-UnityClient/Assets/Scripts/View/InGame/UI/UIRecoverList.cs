using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Repositories;
using CastleHero.Utility;
using UniRx;
using CastleHero.GamePlay.InGame;

namespace CastleHero.View.InGame.UI
{
    public class UIRecoverList : UIListAdapter<UIRecoverSlot, WaitRecover>
    {
        public void Init()
        {
            var recovers = InGameSession.Current.Recovers;
            recovers
                .ObserveCountChanged()
                .ThrottleFrame(1)
                .Subscribe(_ => base.Init(recovers).Forget())
                .AddTo(this);
        }
        
        protected override UniTask SetItem(UIRecoverSlot slot, WaitRecover data) => slot.InitAsync(data);
    }
}