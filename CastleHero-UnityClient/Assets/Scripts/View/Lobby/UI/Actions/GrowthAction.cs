using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;

namespace CastleHero.View.Lobby.UI.Actions
{
    public sealed class CharacterGrowthAction
    {
        private readonly INetworkServiceProvider _network;
        private readonly IPopupManager _popups;
        private readonly ISoundManager _sounds;
        private readonly SoundPath _soundPath;

        public CharacterGrowthAction(IServiceLocator sl)
        {
            _network = sl.Get<INetworkServiceProvider>();
            _popups = sl.Get<IPopupManager>();
            _sounds = sl.Get<ISoundManager>();
            _soundPath = sl.Get<SoundPath>();
        }

        public async UniTask<UnitGrowth> Execute(
            GrowthAction action,
            int unitId,
            int consumeItemId,
            int consumeQuantity)
        {
            var result = await _network.Character.Growth(action, unitId, consumeItemId, consumeQuantity);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return null;
            }

            _sounds.PlaySfx(_soundPath.levelUp);
            return result.data;
        }
    }
}
