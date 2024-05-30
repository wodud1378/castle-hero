using RGLabs.Data.Model;
using RGLabs.Network.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UIConsumeItem : UIItemInformation<Item, ConsumableEntity>
    {
        [SerializeField] private TMP_Text _max;
        [SerializeField] private Slider _slider;
        
        protected override void Construct(Item item, ConsumableEntity entity)
        {
            _slider.maxValue = item.Quantity;
        }
    }
}