using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UILobby : UIMain
    {
        [SerializeField] private Button _characters;
        [SerializeField] private Button _inventory;

        private void Awake()
        {
            this.SubscribeButton(_characters, OpenCharacterPopup);
            this.SubscribeButton(_inventory, OpenInventoryPopup);
        }

        private async void OpenCharacterPopup()
        {
            await Context.popupManager.Open<PopupCharacterList>();
        }
        
        private async void OpenInventoryPopup()
        {
            await Context.popupManager.Open<PopupInventory>();
        }
    }
}