using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI
{
    public class UIItemSlot : UISlot
    {
        public enum QuantityDisplay
        {
            Default,
            ValueOnly,
        }

        [FormerlySerializedAs("_quantity")]
        [SerializeField] private TMP_Text quantity;
        [FormerlySerializedAs("_quantityPrefix")]
        [SerializeField] private string quantityPrefix;
        [FormerlySerializedAs("_quantitySuffix")]
        [SerializeField] private string quantitySuffix;

        protected IDBProvider _db;

        public Color QuantityLabelColor
        {
            get => quantity.color;
            set => quantity.text = quantity.text.WithColor(value);
        }

        public IItem Item { get; private set; }
        public ItemEntity Entity { get; private set; }

        public ReactiveProperty<QuantityDisplay> quantityDisplay = new();

        protected override void OnAwake()
        {
            base.OnAwake();
            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();

            quantityDisplay
                .Subscribe()
                .AddTo(this);
        }

        public void Init(IItem item)
        {
            if (!_db.Items.TryFind(item.ItemId, out var entity))
                return;

            Init(item, entity);
        }

        public void Init(IItem item, ItemEntity entity)
        {
            Item = item;
            Entity = entity;

            UpdateQuantity(quantityDisplay.Value);

            Init(entity.icon, entity.name);
        }

        private void UpdateQuantity(QuantityDisplay mode)
        {
            if (Item == null || quantity == null)
                return;

            var qty = $"{Item.Quantity:N0}";

            switch (mode)
            {
                case QuantityDisplay.Default:
                    quantity.text = $"{qty} / {9999:N0}";
                    break;
                case QuantityDisplay.ValueOnly:
                    quantity.text = $"{quantityPrefix}{qty}{quantitySuffix}";
                    break;
            }
        }
    }
}
