using System;
using System.Linq;
using CastleHero.Data.Model;

namespace CastleHero.Data.DB
{
    [DB("shop")]
    public class ShopDB : DB<ShopItemEntity>
    {
        public string[] GetInAppProducts() =>
            entities.Where(x => x.inApp != string.Empty)
                .Select(x => x.inApp)
                .ToArray();
    }

    [DB("shop_group")]
    public class ShopItemGroupDB : DB<ShopItemGroupEntity> { }
}