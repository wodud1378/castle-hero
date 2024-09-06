using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Inventory.Popup;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Service.Test;
using RGLabs.Utility;
using UniRx;
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
        [SerializeField] private Button _shop;

        [SerializeField] private Button _characters;
        [SerializeField] private Button _inventory;
        [SerializeField] private Button _summon;
        [SerializeField] private Button _dungeon;
        [SerializeField] private Button _castle;


        [SerializeField] private GameObject _noAdsMark;
        [SerializeField] private GameObject _contractMark01;
        [SerializeField] private GameObject _contractMark02;

        // Test.
        [SerializeField] private UITest _uiTest;
        [SerializeField] private Button _cheatButton;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_cheatButton, () => _uiTest.Open());
            this.SubscribeButton(_characters, OpenPopup<PopupCharacterList>);
            this.SubscribeButton(_inventory, OpenPopup<PopupInventory>);
            this.SubscribeButton(_quest, OpenPopup<PopupQuest>);
            this.SubscribeButton(_mail, OpenPopup<PopupMail>);
            this.SubscribeButton(_setting, OpenPopup<PopupSetting>);
            this.SubscribeButton(_dungeon, OpenPopup<PopupDungeon>);
            this.SubscribeButton(_summon, OpenPopup<PopupSummon>);
            this.SubscribeButton(_castle, () => Context.Transition.CurrentState = State.Castle);
            this.SubscribeButton(_shop, () => Context.Transition.CurrentState = State.Shop);

            Storage.userRepository.shopRecord.products
                .ChangeAsObservable()
                .Subscribe(products =>
                {
                    bool HasProduct(int id, DateTime currentTime) =>
                        products.Any(p => p.shopId == id && p.expireDate > currentTime);

                    var currentTime = NetworkService.CurrentTimeByLocal();
                    var noAds = Storage.db.shop.Find(x => x.category == ShopCategory.NoAds);
                    var contracts = Storage.db.shop.FindAll(x => x.category == ShopCategory.Contract);

                    _noAdsMark.SetActive(noAds.IsValid && HasProduct(noAds.Id, currentTime));
                    _contractMark01.SetActive(contracts.Count > 0 && HasProduct(contracts[0].Id, currentTime));
                    _contractMark02.SetActive(contracts.Count > 1 && HasProduct(contracts[1].Id, currentTime));
                })
                .AddTo(this);
        }

        public void ProcessLink(Entrance.Link link)
        {
            switch (link)
            {
                case Entrance.Link.LevelUp:
                case Entrance.Link.RateUp:
                case Entrance.Link.Equipment:
                    OpenPopup<PopupCharacterList>();
                    break;
            }
        }

        protected override void OnBack()
        {
        }

        private void OpenPopup<T>() where T : PopupBase => Context.popups.OpenAsync<T>().Forget();
    }
}