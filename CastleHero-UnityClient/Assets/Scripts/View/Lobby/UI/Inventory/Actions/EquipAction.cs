using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;

namespace CastleHero.View.Lobby.UI.Inventory.Actions
{
    /// <summary>
    /// PopupEquipItem / PopupCharacter 에서 장비 장착/해제 네트워크 호출을 분리한 서비스.
    /// View 는 UI 바인딩만 담당하고, 서버 호출은 이 클래스가 수행한다.
    /// </summary>
    public sealed class EquipAction
    {
        private readonly INetworkServiceProvider _network;
        private readonly IPopupManager _popups;

        public EquipAction(IServiceLocator sl)
        {
            _network = sl.Get<INetworkServiceProvider>();
            _popups = sl.Get<IPopupManager>();
        }

        /// <summary>
        /// 장비 장착 네트워크 호출을 수행한다.
        /// 성공 시 true, 실패 시 에러 팝업을 띄운 뒤 false 를 반환한다.
        /// </summary>
        public async UniTask<bool> Equip(int unitId, string equipGuid)
        {
            var result = await _network.Character.Equip(unitId, equipGuid);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 장비 해제 네트워크 호출을 수행한다.
        /// 성공 시 true, 실패 시 에러 팝업을 띄운 뒤 false 를 반환한다.
        /// </summary>
        public async UniTask<bool> Release(int unitId, string equipGuid)
        {
            var result = await _network.Character.Release(unitId, equipGuid);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return false;
            }

            return true;
        }
    }
}
