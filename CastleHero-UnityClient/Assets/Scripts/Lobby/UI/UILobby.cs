using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Lobby.UI.Inventory.Popup;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UILobby : UIMain
    {
        [SerializeField] private Button _quest;
        [SerializeField] private Button _mail;
        [SerializeField] private Button _attendence;
        [SerializeField] private Button _setting;
        
        [SerializeField] private Button _characters;
        [SerializeField] private Button _inventory;
        [SerializeField] private Button _summon;
        [SerializeField] private Button _dungeon;

        private void Awake()
        {
            this.SubscribeButton(_characters, ()=> Context.popupManager.Open<PopupCharacterList>().Forget());
            this.SubscribeButton(_inventory, ()=> Context.popupManager.Open<PopupInventory>().Forget());
            this.SubscribeButton(_quest, ()=> Context.popupManager.Open<PopupQuest>().Forget());
            this.SubscribeButton(_mail, ()=> Context.popupManager.Open<PopupMail>().Forget());
            this.SubscribeButton(_setting, ()=> Context.popupManager.Open<PopupSetting>().Forget());
            this.SubscribeButton(_dungeon, ()=> Context.popupManager.Open<PopupDungeon>().Forget());
        }
    }
}