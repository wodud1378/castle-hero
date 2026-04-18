using CastleHero.View.Common.UI;
using CastleHero.Data.Repositories;
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
                .Subscribe(_ => base.Init(recovers))
                .AddTo(this);
        }

        protected override void SetItem(UIRecoverSlot slot, WaitRecover data) => slot.InitAsync(data);
    }
}