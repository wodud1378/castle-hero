using CastleHero.Data.Model;

namespace CastleHero.Data.DB
{
    [DB("items")]
    public class ItemDB : DB<ItemEntity> {}
    
    [DB("equip_status")]
    public class EquipItemStatDB : DB<EquipItemStatEntity> {}
}