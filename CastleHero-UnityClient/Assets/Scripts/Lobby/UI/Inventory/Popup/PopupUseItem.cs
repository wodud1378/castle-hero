using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Use.prefab")]
    public class PopupUseItem : PopupItemBase<UIItemSlot, IItem>
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Button _use;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _useCount;
        [SerializeField] private TMP_Text _effect;
        
        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_use, OnUse);

            _slider.onValueChanged
                .AsObservable()
                .Subscribe(UpdateWithQuantity)
                .AddTo(this);
            
            _slider.value = 0f;
        }

        protected override void OnDataInitialized(IItem data)
        {
            _description.text = Entity.desc[0];

            SetActiveSlider();

            if (_slider.gameObject.activeSelf)
            {
                var last = _slider.value;
                _slider.value = Mathf.Clamp(last, 0, _slider.maxValue);
            }
        }

        private void UpdateWithQuantity(float value)
        {
            int toInt = (int)value;
            _useCount.text = (toInt).ToString();
            UpdateEffectText(toInt);
        }

        private void UpdateEffectText(int count)
        {
            if (!string.IsNullOrEmpty(Entity.desc[1]))
            {
                _effect.text = string.Format(Entity.desc[1], int.Parse(Entity.options[1]) * count)
                    .WithPositiveColor();

                _effect.gameObject.SetActive(true);
            }
            else
                _effect.gameObject.SetActive(false);
        }

        private void SetActiveSlider()
        {
            bool isActive = false;
            var type = Entity.type;
            switch (type)
            {
                case ItemType.Consumable:
                    var option = (ConsumeType)int.Parse(Entity.options[0]);
                    isActive = option is ConsumeType.Ap;
                    break;
                case ItemType.Ingredient:
                case ItemType.Chest:
                    isActive = true;
                    break;
            }

            _useCount.gameObject.SetActive(isActive);
            _slider.gameObject.SetActive(isActive);
            _slider.maxValue = item.Value.Quantity;
        }

        private void OnUse()
        {
            if (Entity.type == ItemType.Consumable)
            {
                var option = (ConsumeType)int.Parse(Entity.options[0]);
                switch (option)
                {
                    case ConsumeType.SummonTicket:
                        MoveToDrawCharacter(Entity.Id);
                        break;
                    default:
                        Use();
                        break;
                }
            }
            else if (Entity.type == ItemType.Ingredient)
                Use();
            else if (Entity.type == ItemType.Chest)
                OpenBox();
        }

        private void Use()
        {
            // TODO 사용 로직.
        }

        private async void OpenBox()
        {
            var result = await NetworkService.Item.OpenBox(item.Value.ItemId, (int)_slider.value);

            var currency = result.currency;
            var items = result.items;
            var leftItem = result.leftItem;

            var repository = Storage.userRepository;
            repository.currency.Add(currency);
            repository.inventory.Add(items);
            repository.inventory.Update(leftItem);
            
            Context.popupManager
                .OpenAsync<PopupReceivedItems>(currency, items)
                .Forget();

            if (leftItem.Quantity == 0)
            {
                CloseAsync().Forget();
                return;
            }

            item.Value = leftItem;
        }

        private void MoveToDrawCharacter(int ticketId)
        {
            if (!Storage.db.summons.TryFind(x => x.item.Contains(ticketId), out var entity))
                return;
            
            Context.popupManager
                .OpenAsync<PopupSummon>(entity)
                .Forget();
            
            CloseAsync()
                .Forget();
        }

        private void MoveToRefine()
        {
            // TODO 재련 페이지로 이동.
        }
    }
}