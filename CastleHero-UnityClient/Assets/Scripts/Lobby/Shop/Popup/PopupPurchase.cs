using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Data.Model;
using RGLabs.Lobby.Shop.UI;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.Shop.Popup
{
    [PrefabPath("Shop/Popup/Popup_Purchase.prefab")]
    public class PopupPurchase : PopupBase
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _desc;
        [SerializeField] private UIPrice _price;
        [SerializeField] private Button _confirm;
        [SerializeField] private GameObject _adMark;

        private PaymentType _type;
        private int _id;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_confirm, OnConfirm);
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
            _name.text = entity.name;
            _desc.text = entity.desc;
            _type = ShopHelper.GetPaymentType(entity, product);

            bool isAd = _type == PaymentType.Ad;

            _adMark.SetActive(isAd);
            _price.gameObject.SetActive(!isAd);

            return isAd
                ? UniTask.CompletedTask
                : _price.Init(entity, product);
        }

        private async void OnConfirm()
        {
            var result = await NetworkService.Shop.BuyItem(_type, _id);
            if (!result.IsSuccess)
                Context.popups.Open<PopupCommon>(result.error);
            else
                Context.popups.Open<PopupReceivedItems>(result.data.pack);

            Close();
        }
    }
}