using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UILobby : UIMain
    {
        [SerializeField] private Button _characters;

        private void Awake()
        {
            this.SubscribeButton(_characters, OpenCharacterPopup);
        }

        private async void OpenCharacterPopup()
        {
            var popup = await Context.popupManager.Open<PopupCharacterList>();
            popup.tab.Value = PopupCharacterList.Tab.Storage;
        }
    }
}