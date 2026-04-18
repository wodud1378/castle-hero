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
        private StateManager<LobbyState> _lobbyState;
        private IDBProvider _db;
        private IUserRepository _userRepo;
        private IPopupManager _popups;
        private UIToolTip _toolTip;
        private EntranceHolder _entrance;

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

            var sl = ServiceLocator.Instance;
            _lobbyState = sl.Get<StateManager<LobbyState>>();
            _db = sl.Get<IDBProvider>();
            _userRepo = sl.Get<IUserRepository>();
            _popups = sl.Get<IPopupManager>();
            _toolTip = sl.Get<UIToolTip>();
            _entrance = sl.Get<EntranceHolder>();

            this.SubscribeButton(cheatButton, () => uiTest.Open());
            this.SubscribeButton(characters, OpenPopup<PopupCharacterList>);
            this.SubscribeButton(inventory, OpenPopup<PopupInventory>);
            this.SubscribeButton(quest, OpenPopup<PopupQuest>);
            this.SubscribeButton(mail, OpenPopup<PopupMail>);
            this.SubscribeButton(setting, OpenPopup<PopupSetting>);
            this.SubscribeButton(dungeon, OpenPopup<PopupDungeon>);
            this.SubscribeButton(summon, OpenPopup<PopupSummon>);
            this.SubscribeButton(castle, () => _lobbyState.CurrentState = LobbyState.Castle);
            this.SubscribeButton(shop, () => _lobbyState.CurrentState = LobbyState.Shop);
            this.SubscribeButton(noAdsMark, () =>
            {
                var noAds = _db.Shop.Find(x => x.category == ShopCategory.NoAds);
                if (!noAds.IsValid)
                    return;

                OpenToolTip(noAds.name, noAdsMark);
            });

            SubscribeContractMark(0);
            SubscribeContractMark(1);

            var products = _userRepo.ShopRecord.Products;
            products
                .ChangeAsObservable()
                .Subscribe(UpdateMarks)
                .AddTo(this);

            UpdateMarks(products);

            ProcessLink();
        }

        private void ProcessLink()
        {
            var entrance = _entrance.Current;
            if (entrance.state != State.Lobby || entrance.link == Entrance.Link.None)
                return;

            UnitInfo unit = null;
            switch (entrance.link)
            {
                case Entrance.Link.LevelUp:
                    unit = _userRepo.UnitForLevelUp();
                    break;
                case Entrance.Link.RateUp:
                    unit = _userRepo.UnitForUpgrade();
                    break;
                case Entrance.Link.Equipment:
                    unit = _userRepo.UnitForUpgradeEquipments(out bool openDungeon);
                    if (openDungeon)
                        OpenPopup<PopupDungeon>();
                    break;
            }

            if (unit != null)
                OpenPopup<PopupCharacter>(unit);
        }

        private void SubscribeContractMark(int index)
        {
            if (!index.IsValidIndex(contractMarks))
                return;

            var contracts = _db.Shop.FindAll(x => x.category == ShopCategory.Contract);
            if (contracts.Count < index + 1)
                return;

            var mark = contractMarks[index];
            this.SubscribeButton(mark, () => OpenToolTip(contracts[index].name, mark));
        }

        private void OpenToolTip(string text, Button root) =>
            _toolTip.Open(text, root.transform as RectTransform, 1f, 1f);

        private void UpdateMarks(IEnumerable<Product> collection)
        {
            var products = collection.ToList();

            bool HasProduct(int id, DateTime currentTime) =>
                products.Any(p => p.shopId == id && p.expireDate > currentTime);

            var currentTime = ServerTime.Now;
            var noAds = _db.Shop.Find(x => x.category == ShopCategory.NoAds);
            var contracts = _db.Shop.FindAll(x => x.category == ShopCategory.Contract);

            noAdsMark.gameObject.SetActive(noAds.IsValid && HasProduct(noAds.Id, currentTime));
            contractMarks[0].gameObject.SetActive(contracts.Count > 0 && HasProduct(contracts[0].Id, currentTime));
            contractMarks[1].gameObject.SetActive(contracts.Count > 1 && HasProduct(contracts[1].Id, currentTime));
        }

        protected override void OnBack()
        {
        }

        private void OpenPopup<T>() where T : PopupBase => _popups.Open<T>();

        private void OpenPopup<T>(params object[] param) where T : PopupBase => _popups.Open<T>(param);
    }
}
