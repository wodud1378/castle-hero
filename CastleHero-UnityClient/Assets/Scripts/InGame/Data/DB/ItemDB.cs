using RGLabs.InGame.Data.Model;
using NotImplementedException = System.NotImplementedException;

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