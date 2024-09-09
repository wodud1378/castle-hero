using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIPrice : UISlot
    {
        public async UniTask Init(ShopItemEntity entity, Product product)
        {
            var paymentType = ShopHelper.GetPaymentType(entity, product);
            var spritePath = paymentType switch
            {
                PaymentType.Free => string.Empty,
                PaymentType.Ad => "Sprites/Global/UI/Icon_Ads.png",
                PaymentType.Default => Storage.db.items.TryFind(entity.costId, out var itemEntity)
                    ? itemEntity.icon
                    : string.Empty,
                _ => string.Empty
            };

            const string freeText = "FREE";
            string text = paymentType switch
            {
                PaymentType.Free => freeText,
                PaymentType.Ad => string.Empty,
                PaymentType.Default => entity.costId == 0
                    ? $"\uffe6 {entity.costValue:N0}"
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