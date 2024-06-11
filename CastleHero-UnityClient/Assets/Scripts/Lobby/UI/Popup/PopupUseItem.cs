using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    public class PopupUseItem : PopupItemBase<IItem, IItemEntity>
    {
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Slider _slider;
        [SerializeField] private Button _consume;
        [SerializeField] private TMP_Text _effect;
        
        protected override void InitSubscriptions()
        {
            base.InitSubscriptions();
            
            this.SubscribeButton(_consume, Use);

            _slider.onValueChanged
                .AsObservable()
                .Subscribe(x=> UpdateUI((int)x))
                .AddTo(this);
        }

        protected override void OnDataInitialized()
        {
            _slider.value = 0;
            _slider.maxValue = Item.Quantity;
        }

        private void UpdateUI(int value)
        {
            if (Entity is ConsumableEntity { option: ConsumeOption.Ap or ConsumeOption.Exp } consumable)
            {
                _effect.text = string.Format(consumable.consumeDesc, consumable.optionValue * value);
                _effect.gameObject.SetActive(true);
                return;
            }
            
            _effect.gameObject.SetActive(false);
        }

        private void Use()
        {
            // TODO 사용 로직
        }
    }
}