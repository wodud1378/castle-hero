using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;

namespace CastleHero.View.Lobby.UI.Actions
{
    /// <summary>
    /// PopupRefine 에서 네트워크 호출(Refine)을 분리한 서비스.
    /// PopupRefine 은 UI 바인딩만 담당하고, 서버 호출은 이 클래스가 수행한다.
    /// </summary>
    public sealed class RefineAction
    {
        private readonly INetworkServiceProvider _network;
        private readonly IPopupManager _popups;

        public RefineAction(IServiceLocator sl)
        {
            _network = sl.Get<INetworkServiceProvider>();
            _popups = sl.Get<IPopupManager>();
        }

        /// <summary>
        /// 장비 정련 네트워크 호출을 수행한다.
        /// 성공 시 true, 실패 시 에러 팝업을 띄운 뒤 false 를 반환한다.
        /// </summary>
        public async UniTask<bool> Execute(string equipGuid, int stoneItemId)
        {
            var result = await _network.Inventory.Refine(equipGuid, stoneItemId);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return false;
            }

            return true;
        }
    }
}
