using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;

namespace CastleHero.View.InGame.Actions
{
    /// <summary>
    /// 게임 클리어 네트워크 요청 및 유저 데이터 갱신을 담당하는 Action.
    /// </summary>
    public class GameClearAction
    {
        private readonly IUserRepository _userRepo;
        private readonly INetworkServiceProvider _network;

        public GameClearAction(IServiceLocator sl)
        {
            _userRepo = sl.Get<IUserRepository>();
            _network = sl.Get<INetworkServiceProvider>();
        }

        /// <summary>
        /// 게임 클리어 네트워크 요청을 보낸다.
        /// </summary>
        public async UniTask<Result<GameCleared>> RequestClear(GameType type, int id)
        {
            return await _network.Game.Clear(type, id);
        }

        /// <summary>
        /// 유저 데이터를 서버에서 갱신하여 UserRepository 에 반영한다.
        /// Loading.Tasks 에 추가할 UniTask 를 반환한다.
        /// </summary>
        public UniTask RefreshUserData()
        {
            return _network.User.GetUserData()
                .ContinueWith(x => _userRepo.Update(x.data));
        }
    }
}
