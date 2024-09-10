using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Inventory.Popup;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Service.Test;
using RGLabs.Network.Shared;
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

        [SerializeField] private Button _noAdsMark;
        [SerializeField] private Button[] _contractMarks;

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
            this.SubscribeButton(_noAdsMark, () =>
            {
                var noAds = Storage.db.shop.Find(x => x.category == ShopCategory.NoAds);
                if (!noAds.IsValid)
                    return;

                OpenToolTip(noAds.name, _noAdsMark);
            });

            SubScribeContractMark(0);
            SubScribeContractMark(1);

            var products = Storage.userRepository.shopRecord.products;
            products
                .ChangeAsObservable()
                .Subscribe(UpdateMarks)
                .AddTo(this);

            UpdateMarks(products);

            ProcessLink();
        }

        private void ProcessLink()
        {
            var entrance = Storage.entranceData;
            if (entrance.state != State.Lobby || entrance.link == Entrance.Link.None)
                return;

            var repository = Storage.userRepository;
            UnitInfo unit = null;
            switch (entrance.link)
            {
                case Entrance.Link.LevelUp:
                    unit = repository.UnitForLevelUp();
                    break;
                case Entrance.Link.RateUp:
                    unit = repository.UnitForUpgrade();
                    break;
                case Entrance.Link.Equipment:
                    unit = Storage.userRepository.UnitForUpgradeEquipments(out bool openDungeon);
                    if (openDungeon)
                        OpenPopup<PopupDungeon>();
                    break;
            }

            if (unit != null)
                OpenPopup<PopupCharacter>(unit);
        }

        private void SubScribeContractMark(int index)
        {
            if (!index.IsValidIndex(_contractMarks))
                return;

            var contracts = Storage.db.shop.FindAll(x => x.category == ShopCategory.Contract);
            if (contracts.Count < index + 1)
                return;

            var mark = _contractMarks[index];
            this.SubscribeButton(mark, () => OpenToolTip(contracts[index].name, mark));
        }

        private void OpenToolTip(string text, Button root) =>
            Context.toolTip.Open(text, root.transform as RectTransform, 0.5f, 1f);

        private void UpdateMarks(IEnumerable<Product> collection)
        {
            var products = collection.ToList();

            bool HasProduct(int id, DateTime currentTime) =>
                products.Any(p => p.shopId == id && p.expireDate > currentTime);

            var currentTime = NetworkService.CurrentTimeByLocal();
            var noAds = Storage.db.shop.Find(x => x.category == ShopCategory.NoAds);
            var contracts = Storage.db.shop.FindAll(x => x.category == ShopCategory.Contract);

            _noAdsMark.gameObject.SetActive(noAds.IsValid && HasProduct(noAds.Id, currentTime));
            _contractMarks[0].gameObject.SetActive(contracts.Count > 0 && HasProduct(contracts[0].Id, currentTime));
            _contractMarks[1].gameObject.SetActive(contracts.Count > 1 && HasProduct(contracts[1].Id, currentTime));
        }

        protected override void OnBack()
        {
        }

        private void OpenPopup<T>() where T : PopupBase => Context.popups.Open<T>();

        private void OpenPopup<T>(params object[] param) where T : PopupBase => Context.popups.Open<T>(param);
    }
}