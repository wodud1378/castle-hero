using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Pattern;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;
using CastleHero.View.Lobby.UI.Popup;

namespace CastleHero.View.Lobby.UI.Actions
{
    /// <summary>
    /// PopupSummon 에서 네트워크 호출과 Repository 갱신 로직을 분리한 서비스.
    /// PopupSummon 은 UI 위젯 바인딩만 담당하고, 소환 실행과 서버 호출은 이 클래스가 수행한다.
    /// </summary>
    public sealed class SummonAction
    {
        private readonly INetworkServiceProvider _network;
        private readonly IPopupManager _popups;
        private readonly IUserRepository _userRepo;

        public SummonAction(IServiceLocator sl)
        {
            _network = sl.Get<INetworkServiceProvider>();
            _popups = sl.Get<IPopupManager>();
            _userRepo = sl.Get<IUserRepository>();
        }

        public async UniTask<Summon> SummonOnce(int eventId, int costIndex)
        {
            var result = await _network.Summon.SummonOnce(eventId, costIndex);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return null;
            }

            return result.data;
        }

        public async UniTask<Summon> SummonTenth(int eventId, int costIndex)
        {
            var result = await _network.Summon.SummonTenth(eventId, costIndex);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return null;
            }

            return result.data;
        }

        public void ApplySummonResult(Summon summon)
        {
            var repository = _userRepo;
            foreach (var summoned in summon.list)
            {
                switch (summoned)
                {
                    case SummonedSoul soul:
                        repository.Inventory.Add(new Item { ItemId = soul.Id, Quantity = soul.quantity });
                        break;
                    case SummonedUnit unit:
                        repository.Characters.Add(new UnitInfo { id = unit.Id, lv = unit.lv, rate = unit.rate });
                        break;
                }
            }
        }

        public bool HasEnoughItem(int costId, int cost, bool openPopup = true)
        {
            var repo = _userRepo;
            var currency = repo.Currency;
            bool isEnough;
            switch (costId)
            {
                case Constants.GoldId:
                    isEnough = currency.Gold.Value >= cost;
                    break;
                case Constants.PaidDiaId or Constants.FreeDiaId:
                    isEnough = currency.FreeDia.Value + currency.PaidDia.Value >= cost;
                    break;
                default:
                {
                    var item = repo.Inventory.Items.FirstOrDefault(x => x.ItemId == costId);
                    isEnough = item != null && item.Quantity >= cost;
                    break;
                }
            }

            if (!isEnough && openPopup)
                _popups.Open<PopupCommon>("재화 혹은 아이템 부족해유");

            return isEnough;
        }
    }
}
