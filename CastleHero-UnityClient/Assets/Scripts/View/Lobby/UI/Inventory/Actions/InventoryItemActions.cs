using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;
using CastleHero.View.Lobby.UI.Inventory.Popup;
using CastleHero.View.Lobby.UI.Popup;

namespace CastleHero.View.Lobby.UI.Inventory.Actions
{
    public sealed class InventoryItemActions
    {
        private readonly IPopupManager _popups;
        private readonly IDBProvider _db;
        private readonly INetworkServiceProvider _network;

        public InventoryItemActions(IPopupManager popups, IDBProvider db, INetworkServiceProvider network)
        {
            _popups = popups;
            _db = db;
            _network = network;
        }

        public async UniTask Sell((IItem item, int quantity)[] selected)
        {
            if (selected == null || selected.Length == 0)
                return;

            var price = selected
                .Select(x =>
                {
                    if (!_db.Items.TryFind(x.item.ItemId, out var entity))
                        return 0;
                    return entity.sellPrice * x.quantity;
                }).Sum();

            string popupText = $"{price:N0} 골드에 판매하시겠습니까?";
            var selectSource = new UniTaskCompletionSource<bool>();
            var param = new PopupCommon.ButtonParam[]
            {
                new()
                {
                    action = PopupCommon.ButtonAction.Confirm,
                    onClick = () => selectSource.TrySetResult(true)
                },
                new()
                {
                    action = PopupCommon.ButtonAction.Cancel,
                    onClick = () => selectSource.TrySetResult(false)
                }
            };

            _popups.Open<PopupCommon>(popupText, param);

            if (!await selectSource.Task)
                return;

            var result = await _network.Inventory.Sell(
                selected.Select(x => x.item).ToArray(),
                selected.Select(x => x.quantity).ToArray());

            if (!result.IsSuccess)
                _popups.Open<PopupCommon>(result.error);
        }

        public async UniTask Equip(EquipItem item)
        {
            var equipPopup = await _popups.OpenAsync<PopupEquipItem>(item);
            equipPopup.BeginSelect(true);

            var equipItem = await equipPopup.SelectTask;
            if (equipItem == null)
                return;

            var characterList = await _popups.OpenAsync<PopupCharacterList>();
            characterList.BeginSelect(true);

            var unit = await characterList.SelectTask;
            if (unit == null)
                return;

            var compare = await _popups.OpenAsync<PopupCompareEquipment>(unit, equipItem);
            compare.BeginSelect(true);

            var confirm = await compare.SelectTask;
            if (!confirm)
                return;

            var result = await _network.Character.Equip(unit.id, equipItem.Guid);
            if (!result.IsSuccess)
                _popups.Open<PopupCommon>(result.error);
        }
    }
}
