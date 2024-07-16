using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Item_Use_Table")]
    public class ConsumableDB : DB<ConsumableEntity> { }
    
    [DB("Item_Chest_Table")]
    public class ChestDB : DB<ChestEntity> { }
    
    [DB("Item_Equip_Table")]
    public class EquipmentDB : DB<EquipmentEntity> { }

    [DB("Item_Parts_Table")]
    public class IngredientDB : DB<IngredientEntity> { }
    
    public class ItemDB : DB<ItemEntity> {}
}