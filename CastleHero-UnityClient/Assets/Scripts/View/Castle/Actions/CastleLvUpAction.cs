using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Network.Service;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;

namespace CastleHero.View.Castle.Actions
{
    /// <summary>
    /// UICastle 에서 성 레벨업 네트워크 호출과 사운드 재생을 분리한 서비스.
    /// UICastle 은 UI 바인딩만 담당하고, 서버 호출은 이 클래스가 수행한다.
    /// </summary>
    public sealed class CastleLvUpAction
    {
        private readonly INetworkServiceProvider _network;
        private readonly IPopupManager _popups;
        private readonly ISoundManager _sounds;
        private readonly SoundPath _soundPath;

        public CastleLvUpAction(IServiceLocator sl)
        {
            _network = sl.Get<INetworkServiceProvider>();
            _popups = sl.Get<IPopupManager>();
            _sounds = sl.Get<ISoundManager>();
            _soundPath = sl.Get<SoundPath>();
        }

        /// <summary>
        /// 성 레벨업 네트워크 호출을 수행한다.
        /// 성공 시 true 반환 + 사운드 재생, 실패 시 에러 팝업을 띄운 뒤 false 를 반환한다.
        /// </summary>
        public async UniTask<bool> Execute()
        {
            var result = await _network.Castle.LvUp();
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return false;
            }

            _sounds.PlaySfx(_soundPath.levelUp);
            return true;
        }
    }
}
