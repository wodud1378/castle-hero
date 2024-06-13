using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using UnityEngine;

namespace RGLabs.Lobby.UI.Inventory
{
    public class UIEquipmentSlot : UIItemSlot
    {
        [SerializeField] private UIGrade _grade;

        public UniTask Init(EquipItem item)
        {
            if (!Storage.db.itemDBAccessor.TryLoad(item.Id, out var entity) ||
                entity is not EquipmentEntity equipmentEntity)
                return UniTask.CompletedTask;
            
            return Init(item, equipmentEntity);
        }

        public UniTask Init(EquipItem item, EquipmentEntity entity)
        {
            if(_grade != null)
                _grade.Set(entity.grade);
            
            return base.Init(item, entity);
        }
    }
}