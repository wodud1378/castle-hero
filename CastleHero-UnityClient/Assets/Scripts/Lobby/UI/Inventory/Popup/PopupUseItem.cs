using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service.Item;
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

        private readonly ItemService _service = new();
        
        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_use, OnUse);
            
            _slider.onValueChanged
                .AsObservable()
                .Subscribe(UpdateWithQuantity)
                .AddTo(this);
        }

        protected override void OnDataInitialized()
        {
            if(Entity.desc is { Length: > 0 })
                _description.text = Entity.desc[0];
            
            SetActiveSlider();

            if (_slider.gameObject.activeSelf)
            {
                _slider.value = 0;
                UpdateWithQuantity(0);
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
            if(Entity.desc is { Length: > 0 })
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
            _slider.maxValue = Item.Quantity;
        }

        private void OnUse()
        {
            if (Entity.type == ItemType.Consumable)
            {
                var option = (ConsumeType)int.Parse(Entity.options[0]);
                switch (option)
                {
                    case ConsumeType.SummonTicket:
                        MoveToDrawCharacter();
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
            var result = await _service.OpenBox(Item.ItemId, (int)_slider.value);

            var currency = result.currency;
            var items = result.items;
            
            Storage.userRepository.Add(currency);
            Storage.userRepository.Add(items);
            Context.popupManager.Open<PopupReceivedItems>(currency, items).Forget();
        }

        private void MoveToDrawCharacter()
        {
            // TODO 캐릭터 뽑기로 이동.
        }

        private void MoveToRefine()
        {
            // TODO 재련 페이지로 이동.
        }
    }
}