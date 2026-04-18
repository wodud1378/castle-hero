using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;
using CastleHero.View.Lobby.UI.Popup;

namespace CastleHero.View.Lobby.Shop.Actions
{
    /// <summary>
    /// UIShop / PopupPurchase 에서 네트워크 호출 로직을 분리한 서비스.
    /// </summary>
    public sealed class ShopAction
    {
        private readonly INetworkServiceProvider _network;
        private readonly IPopupManager _popups;

        public ShopAction(IServiceLocator sl)
        {
            _network = sl.Get<INetworkServiceProvider>();
            _popups = sl.Get<IPopupManager>();
        }

        public UniTask<Result> RefreshProducts()
        {
            return _network.Shop.RefreshProducts();
        }

        public async UniTask BuyItem(PaymentType type, int id)
        {
            var result = await _network.Shop.BuyItem(type, id);
            if (!result.IsSuccess)
                _popups.Open<PopupCommon>(result.error);
            else
                _popups.Open<PopupReceivedItems>(result.data.pack);
        }
    }
}
