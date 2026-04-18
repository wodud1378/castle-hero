using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.Shop.UI;
using CastleHero.View.Lobby.Shop.Actions;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.Common.Pattern;

namespace CastleHero.View.Lobby.Shop.Popup
{
    [PrefabPath("Shop/Popup/Popup_Purchase.prefab")]
    public class PopupPurchase : PopupBase
    {
        [FormerlySerializedAs("_name")]
        [SerializeField] private TMP_Text name;
        [FormerlySerializedAs("_desc")]
        [SerializeField] private TMP_Text desc;
        [FormerlySerializedAs("_price")]
        [SerializeField] private UIPrice price;
        [FormerlySerializedAs("_confirm")]
        [SerializeField] private Button confirm;
        [FormerlySerializedAs("_adMark")]
        [SerializeField] private GameObject adMark;

        private PaymentType _type;
        private int _id;

        private ShopAction _action;

        protected override void OnAwake()
        {
            base.OnAwake();

            _action = ServiceLocator.Instance.Get<ShopAction>();

            this.SubscribeButton(confirm, OnConfirm);
        }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters[0] is not ShopItemEntity entity ||
                parameters[1] is not null && parameters[1] is not Product)
            {
                Close();
                return UniTask.CompletedTask;
            }

            var product = (Product)parameters[1];

            _id = entity.Id;
            name.text = entity.name;
            desc.text = entity.desc;
            _type = ShopHelper.GetPaymentType(entity, product);

            bool isAd = _type == PaymentType.Ad;

            adMark.SetActive(isAd);
            price.gameObject.SetActive(!isAd);

            if (!isAd)
                price.Init(entity, product);

            return UniTask.CompletedTask;
        }

        private async UniTask OnConfirm()
        {
            await _action.BuyItem(_type, _id);
            Close();
        }
    }
}
