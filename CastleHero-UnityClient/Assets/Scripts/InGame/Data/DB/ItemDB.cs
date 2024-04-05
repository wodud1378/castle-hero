using RGLabs.InGame.Data.Model;

namespace RGLabs.InGame.Data.DB
{
    public class ItemDB : DB<ItemEntity>
    {
        protected override ItemEntity FallBackEntity() =>
            new()
            {

            };
    }
}