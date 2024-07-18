using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Item_Table", "items")]
    public class ItemDB : DB<ItemEntity> {}
}