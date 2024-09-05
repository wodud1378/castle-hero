using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using TMPro;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIPrice : UISlot
    {
        public UniTask Init(PaymentType paymentType, int costId, int costValue)
        {
            var spritePath = paymentType switch
            {
                PaymentType.Free => string.Empty,
                PaymentType.Ad => "Sprites/Global/UI/Icon_Ads.png",
                PaymentType.Default => Storage.db.items.TryFind(costId, out var itemEntity)
                    ? itemEntity.icon
                    : string.Empty,
                _ => string.Empty
            };

            const string freeText = "FREE";
            string text = paymentType switch
            {
                PaymentType.Free => freeText,
                PaymentType.Ad => string.Empty,
                PaymentType.Default => costId == 0
                    ? $"\uffe6 {costValue:N0}"
                    : $"{costValue:N0}",
                _ => string.Empty
            };

            label.alignment = text == freeText
                ? TextAlignmentOptions.Center
                : TextAlignmentOptions.Right;

            return base.Init(spritePath, text);
        }
    }
}