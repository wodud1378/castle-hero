using Cysharp.Threading.Tasks;
using CastleHero.Common.InApp;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    public interface IShopService
    {
        void RegisterIAP(IAPManager iap);
        string InAppPrice(string productKey, int fallBack = -1);
        UniTask<Result> RefreshProducts();
        UniTask<Result<Pack>> ReceiveSubscribedItems();
        UniTask<Result<ItemBought>> BuyItem(PaymentType type, int id);
    }
}
