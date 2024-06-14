using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Use.prefab")]
    public class PopupUseItem : PopupItemBase<UIItemSlot, IItem, IItemEntity>
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
        }

        protected override void OnDataInitialized()
        {
            _description.text = Entity.Desc;
            
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
            if (Entity is ConsumableEntity { option: ConsumeOption.Ap or ConsumeOption.Exp } consumable)
            {
                _effect.text = string.Format(consumable.consumeDesc, consumable.optionValue * count)
                    .WithPositiveColor();
                
                _effect.gameObject.SetActive(true);
                return;
            }
            
            _effect.gameObject.SetActive(false);
        }
        
        private void SetActiveSlider()
        {
            bool isActive = false;
            var type = Entity.Id.ItemType();
            if (type == ItemTypeCode.Chest)
            {
                isActive = true;
            }
            else if (type == ItemTypeCode.Ingredient)
            {
                isActive = true;
            }
            else if (Entity is ConsumableEntity consumableEntity)
            {
                var option = consumableEntity.option;
                isActive = option is ConsumeOption.Ap;
            }

            _useCount.gameObject.SetActive(isActive);
            _slider.gameObject.SetActive(isActive);
            _slider.maxValue = Item.Quantity;
        }

        private void OnUse()
        {
            if (Entity is ConsumableEntity consumableEntity)
            {
                var option = consumableEntity.option;
                switch (option)
                {
                    case ConsumeOption.SummonTicket:
                        MoveToDrawCharacter();
                        break;
                    default:
                        Use();
                        break;
                }
            }
            else if (Entity is IngredientEntity)
                Use();
        }

        private void Use()
        {
            // TODO 사용 로직.
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