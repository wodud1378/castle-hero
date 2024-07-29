using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UIItemSlot : UISlot
    {
        public enum QuantityDisplay
        {
            Default,
            ValueOnly,
        }
        
        [SerializeField] private TMP_Text _quantity;
        [SerializeField] private GameObject _portraitRoot;
        
        public Color QuantityLabelColor
        {
            get => _quantity.color;
            set => _quantity.text = _quantity.text.WithColor(value);
        }
        
        public IItem Item { get; private set; }
        public ItemEntity Entity { get; private set; }

        public ReactiveProperty<QuantityDisplay> quantityDisplay = new();

        private void Awake()
        {
            quantityDisplay
                .Subscribe()
                .AddTo(this);
        }

        public UniTask Init(IItem item)
        {
            if(!Storage.db.items.TryFind(item.ItemId, out var entity))
                return UniTask.CompletedTask;

            return Init(item, entity);
        }
        
        public UniTask Init(IItem item, ItemEntity entity)
        {
            Item = item;
            Entity = entity;

            UpdateQuantity(quantityDisplay.Value);
            
            var portraitTask = UpdatePortrait();
            var initTask = Init(entity.icon, entity.name);

            return UniTask.WhenAll(portraitTask, initTask);
        }
        
        private async UniTask UpdatePortrait()
        {
            if (_portraitRoot == null || Item is not EquipItem equipItem)
                return;
            
            _portraitRoot.SetActive(false);

            int character = equipItem.character;
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

        private void UpdateQuantity(QuantityDisplay mode)
        {
            if (Item == null || _quantity == null)
                return;

            var quantity = $"{Item.Quantity:N0}";
            switch (mode)
            {
                case QuantityDisplay.Default:
                    _quantity.text = $"{quantity} / {9999:N0}";
                    break;
                case QuantityDisplay.ValueOnly:
                    _quantity.text = $"{quantity}";
                    break;
            }
        }
    }
}