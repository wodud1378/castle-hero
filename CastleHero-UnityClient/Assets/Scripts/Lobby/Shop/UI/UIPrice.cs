using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIPrice : UISlot
    {
        public async UniTask Init(ShopItemEntity entity, Product product)
        {
            bool isInApp = !string.IsNullOrEmpty(entity.inApp);
            var paymentType = ShopHelper.GetPaymentType(entity, product);
            var spritePath = paymentType switch
            {
                PaymentType.Free => string.Empty,
                PaymentType.Ad => "Sprites/Global/UI/Icon_Ads.png",
                PaymentType.Default => !isInApp && Storage.db.items.TryFind(entity.costId, out var itemEntity)
                    ? itemEntity.icon
                    : string.Empty,
                _ => string.Empty
            };

            const string freeText = "FREE";
            string text = paymentType switch
            {
                PaymentType.Free => freeText,
                PaymentType.Ad => string.Empty,
                PaymentType.Default => isInApp
                    ? NetworkService.Shop.InAppPrice(entity.inApp, entity.costValue)
                    : $"{entity.costValue:N0}",
                _ => string.Empty
            };

            label.alignment = paymentType == PaymentType.Default
                ? TextAlignmentOptions.Right
                : TextAlignmentOptions.Center;

            await base.Init(spritePath, text);

            icon.gameObject.SetActive(icon.sprite == null);
            label.gameObject.SetActive(!string.IsNullOrEmpty(label.text));
        }
    }
}