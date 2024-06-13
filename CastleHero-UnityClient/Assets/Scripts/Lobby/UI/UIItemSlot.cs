using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UIItemSlot : UISlot
    {
        [SerializeField] private TMP_Text _quantity;
        [SerializeField] private GameObject _portraitRoot;
        
        public IItem Item { get; private set; }
        public IItemEntity Entity { get; private set; }
        
        public UniTask Init(IItem item, IItemEntity entity)
        {
            Item = item;
            Entity = entity;

            if (_quantity != null)
                _quantity.text = $"{item.Quantity} / 9999";
            
            if(_portraitRoot != null && item is EquipItem equipItem)
                SetPortrait(equipItem.character).Forget();
            
            return Init(entity.Icon, entity.Name);
        }
        
        private async UniTask SetPortrait(int character)
        {
            _portraitRoot.SetActive(false);
            
            if (character == 0)
                return;

            if (!Storage.db.units.TryFind(character, out var entity))
                return;

            var image = _portraitRoot.GetComponentInChildren<Image>();
            if (image == null)
                return;

            var sprite = await entity.icon.Load<Sprite>();
            image.sprite = sprite;
            _portraitRoot.SetActive(sprite != null);
        }
    }
}