using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using TMPro;
using UnityEngine;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopItemSlot : UISlot
    {
        public int Id { get; private set; }

        [SerializeField] private TMP_Text _leftTime;
        [SerializeField] private TMP_Text _purchaseCount;

        public UniTask Init(ShopItemEntity entity)
        {
            Id = entity.Id;

            return base.Init(entity.image, entity.name);
        }
    }
}