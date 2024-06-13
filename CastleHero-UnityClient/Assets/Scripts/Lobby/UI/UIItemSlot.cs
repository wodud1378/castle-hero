using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using TMPro;
using UnityEngine;

namespace RGLabs.Lobby.UI
{
    public class UIItemSlot : UISlot
    {
        [SerializeField] private TMP_Text _quantity;
        
        public IItem Item { get; private set; }
        public IItemEntity Entity { get; private set; }
        
        public UniTask Init(IItem item, IItemEntity entity)
        {
            Item = item;
            Entity = entity;

            if (_quantity != null)
                _quantity.text = $"{item.Quantity} / 9999";
            
            return Init(entity.Icon, entity.Name);
        }
    }
}