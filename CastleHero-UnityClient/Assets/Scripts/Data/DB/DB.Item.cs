using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Item_Table", "items")]
    public class ItemDB : DB<ItemEntity> {}
    
    [DB("Item_Equip_Status_Table", "equip_status")]
    public class EquipItemStatDB : DB<EquipItemStatEntity> {}
}