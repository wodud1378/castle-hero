using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.Shop.UI;
using CastleHero.View.Lobby.UI.Popup;
using CastleHero.Network.Service;
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

        protected override void OnAwake()
        {
            base.OnAwake();

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

            return isAd
                ? UniTask.CompletedTask
                : price.Init(entity, product);
        }

        private async UniTask OnConfirm()
        {
            var result = await ServiceLocator.Get<INetworkServiceProvider>().Shop.BuyItem(_type, _id);
            if (!result.IsSuccess)
                ServiceLocator.Get<IPopupManager>().Open<PopupCommon>(result.error);
            else
                ServiceLocator.Get<IPopupManager>().Open<PopupReceivedItems>(result.data.pack);

            Close();
        }
    }
}
