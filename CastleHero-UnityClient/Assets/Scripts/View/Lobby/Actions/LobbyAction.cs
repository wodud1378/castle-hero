using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;

namespace CastleHero.View.Lobby.Actions
{
    /// <summary>
    /// 로비 씬 진입 시 수행하는 네트워크 호출(스태미나 갱신, 구독 상품 수령)을 담당하는 Action.
    /// </summary>
    public class LobbyAction
    {
        private readonly IUserRepository _userRepo;
        private readonly IDBProvider _db;
        private readonly INetworkServiceProvider _network;

        public LobbyAction(IServiceLocator sl)
        {
            _userRepo = sl.Get<IUserRepository>();
            _db = sl.Get<IDBProvider>();
            _network = sl.Get<INetworkServiceProvider>();
        }

        /// <summary>
        /// 스태미나를 서버와 동기화한다.
        /// </summary>
        public async UniTask<Result> UpdateStamina()
        {
            return await _network.User.UpdateStamina();
        }

        /// <summary>
        /// 구독 상품의 보상 수령 조건을 확인하고, 조건에 맞으면 수령 요청을 보낸다.
        /// 수령할 상품이 없으면 null 을 반환한다.
        /// </summary>
        public async UniTask<Result<Pack>> TryReceiveSubscribedItems()
        {
            var products = _userRepo.ShopRecord.Products;
            if (products == null || products.Count == 0)
                return null;

            var currentTime = ServerTime.Now;
            var hasProducts = products
                .Any(x =>
                {
                    if (x.expireDate <= currentTime)
                        return false;

                    if ((currentTime.Date - x.updatedAt.Date).TotalDays <= 0)
                        return false;

                    if (!_db.Shop.TryFind(x.shopId, out var entity))
                        return false;

                    if (!_db.ShopGroup.TryFind(entity.groupId, out var groupEntity))
                        return false;

                    return groupEntity is { ids: { Length: > 0 }, quantities: { Length: > 0 } };
                });

            if (!hasProducts)
                return null;

            return await _network.Shop.ReceiveSubscribedItems();
        }
    }
}
