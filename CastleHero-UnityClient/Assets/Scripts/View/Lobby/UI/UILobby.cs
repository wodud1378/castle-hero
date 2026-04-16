using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.UI.Inventory.Popup;
using CastleHero.View.Lobby.UI.Popup;
using CastleHero.Network.Service;
using CastleHero.View.Test;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI
{
    public class UILobby : UIMain
    {
        [FormerlySerializedAs("_quest")]
        [SerializeField] private Button quest;
        [FormerlySerializedAs("_mail")]
        [SerializeField] private Button mail;
        [FormerlySerializedAs("_attendence")]
        [SerializeField] private Button attendence;
        [FormerlySerializedAs("_setting")]
        [SerializeField] private Button setting;
        [FormerlySerializedAs("_shop")]
        [SerializeField] private Button shop;

        [FormerlySerializedAs("_characters")]
        [SerializeField] private Button characters;
        [FormerlySerializedAs("_inventory")]
        [SerializeField] private Button inventory;
        [FormerlySerializedAs("_summon")]
        [SerializeField] private Button summon;
        [FormerlySerializedAs("_dungeon")]
        [SerializeField] private Button dungeon;
        [FormerlySerializedAs("_castle")]
        [SerializeField] private Button castle;

        [FormerlySerializedAs("_noAdsMark")]
        [SerializeField] private Button noAdsMark;
        [FormerlySerializedAs("_contractMarks")]
        [SerializeField] private Button[] contractMarks;

        // Test.
        [FormerlySerializedAs("_uiTest")]
        [SerializeField] private UITest uiTest;
        [FormerlySerializedAs("_cheatButton")]
        [SerializeField] private Button cheatButton;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(cheatButton, () => uiTest.Open());
            this.SubscribeButton(characters, OpenPopup<PopupCharacterList>);
            this.SubscribeButton(inventory, OpenPopup<PopupInventory>);
            this.SubscribeButton(quest, OpenPopup<PopupQuest>);
            this.SubscribeButton(mail, OpenPopup<PopupMail>);
            this.SubscribeButton(setting, OpenPopup<PopupSetting>);
            this.SubscribeButton(dungeon, OpenPopup<PopupDungeon>);
            this.SubscribeButton(summon, OpenPopup<PopupSummon>);
            this.SubscribeButton(castle, () => ServiceLocator.Get<StateManager<LobbyState>>().CurrentState = LobbyState.Castle);
            this.SubscribeButton(shop, () => ServiceLocator.Get<StateManager<LobbyState>>().CurrentState = LobbyState.Shop);
            this.SubscribeButton(noAdsMark, () =>
            {
                var noAds = ServiceLocator.Get<IDBProvider>().Shop.Find(x => x.category == ShopCategory.NoAds);
                if (!noAds.IsValid)
                    return;

                OpenToolTip(noAds.name, noAdsMark);
            });

            SubScribeContractMark(0);
            SubScribeContractMark(1);

            var products = ServiceLocator.Get<IUserRepository>().ShopRecord.Products;
            products
                .ChangeAsObservable()
                .Subscribe(UpdateMarks)
                .AddTo(this);

            UpdateMarks(products);

            ProcessLink();
        }

        private void ProcessLink()
        {
            var entrance = ServiceLocator.Get<EntranceHolder>().Current;
            if (entrance.state != State.Lobby || entrance.link == Entrance.Link.None)
                return;

            var repository = ServiceLocator.Get<IUserRepository>();
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
                    unit = ServiceLocator.Get<IUserRepository>().UnitForUpgradeEquipments(out bool openDungeon);
                    if (openDungeon)
                        OpenPopup<PopupDungeon>();
                    break;
            }

            if (unit != null)
                OpenPopup<PopupCharacter>(unit);
        }

        private void SubScribeContractMark(int index)
        {
            if (!index.IsValidIndex(contractMarks))
                return;

            var contracts = ServiceLocator.Get<IDBProvider>().Shop.FindAll(x => x.category == ShopCategory.Contract);
            if (contracts.Count < index + 1)
                return;

            var mark = contractMarks[index];
            this.SubscribeButton(mark, () => OpenToolTip(contracts[index].name, mark));
        }

        private void OpenToolTip(string text, Button root) =>
            ServiceLocator.Get<UIToolTip>().Open(text, root.transform as RectTransform, 1f, 1f);

        private void UpdateMarks(IEnumerable<Product> collection)
        {
            var products = collection.ToList();

            bool HasProduct(int id, DateTime currentTime) =>
                products.Any(p => p.shopId == id && p.expireDate > currentTime);

            var currentTime = ServerTime.Now;
            var noAds = ServiceLocator.Get<IDBProvider>().Shop.Find(x => x.category == ShopCategory.NoAds);
            var contracts = ServiceLocator.Get<IDBProvider>().Shop.FindAll(x => x.category == ShopCategory.Contract);

            noAdsMark.gameObject.SetActive(noAds.IsValid && HasProduct(noAds.Id, currentTime));
            contractMarks[0].gameObject.SetActive(contracts.Count > 0 && HasProduct(contracts[0].Id, currentTime));
            contractMarks[1].gameObject.SetActive(contracts.Count > 1 && HasProduct(contracts[1].Id, currentTime));
        }

        protected override void OnBack()
        {
        }

        private void OpenPopup<T>() where T : PopupBase => ServiceLocator.Get<IPopupManager>().Open<T>();

        private void OpenPopup<T>(params object[] param) where T : PopupBase => ServiceLocator.Get<IPopupManager>().Open<T>(param);
    }
}
