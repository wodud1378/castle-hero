using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using UnityEngine;

namespace RGLabs.Lobby.UI.Inventory
{
    public class UIEquipmentSlot : UIItemSlot
    {
        [SerializeField] private UIGrade _grade;
        
        public UniTask Init(EquipItem item)
        {
            if (!Storage.db.items.TryFind(item.ItemId, out var entity))
                return UniTask.CompletedTask;
            
            return Init(item, entity);
        }

        public UniTask Init(EquipItem item, ItemEntity entity)
        {
            var option = entity.GetEquipmentOption();
            
            if(_grade != null)
                _grade.Set(option.grade);
            
            return base.Init(item, entity);
        }
    }
}