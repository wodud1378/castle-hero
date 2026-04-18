using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;

namespace CastleHero.View.Lobby.Prepare.Actions
{
    /// <summary>
    /// 게임 시작 네트워크 요청 및 관련 상태 변경을 담당하는 Action.
    /// </summary>
    public class GameStartAction
    {
        private readonly IUserRepository _userRepo;
        private readonly INetworkServiceProvider _network;

        public GameStartAction(IServiceLocator sl)
        {
            _userRepo = sl.Get<IUserRepository>();
            _network = sl.Get<INetworkServiceProvider>();
        }

        /// <summary>
        /// 현재 Entrance 정보를 기반으로 게임 시작 네트워크 요청을 보낸다.
        /// </summary>
        public async UniTask<Result> RequestStart()
        {
            var entrance = _userRepo.Entrance.Value;
            return await _network.Game.Start(entrance.type, entrance.id);
        }
    }
}
