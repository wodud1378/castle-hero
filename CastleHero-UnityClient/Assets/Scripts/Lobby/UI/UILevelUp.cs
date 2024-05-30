using RGLabs.Data.Model;
using RGLabs.Network.Model;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UILevelUp : UIConsumeItem
    {
        [SerializeField] private Image _image;
        
        protected override void Construct(Item item, ConsumableEntity entity)
        {
            base.Construct(item, entity);
            
            
        }
    }
}