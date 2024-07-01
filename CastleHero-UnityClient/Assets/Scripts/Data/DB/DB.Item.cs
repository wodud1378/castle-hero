using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Item_Use_Table", "use")]
    public class ConsumableDB : DB<ConsumableEntity> { }
    
    [DB("Item_Chest_Table", "chest")]
    public class ChestDB : DB<ChestEntity> { }
    
    [DB("Item_Equip_Table", "equip")]
    public class EquipmentDB : DB<EquipmentEntity> { }

    [DB("Item_Parts_Table", "parts")]
    public class IngredientDB : DB<IngredientEntity> { }
}