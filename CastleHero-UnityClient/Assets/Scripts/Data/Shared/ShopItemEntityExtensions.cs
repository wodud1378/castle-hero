using CastleHero.Common.Localize;
using CastleHero.Data.Model;

namespace CastleHero.Data.Shared
{
    public static class ShopItemEntityExtensions
    {
        public static string CategoryText(this in ShopItemEntity entity, LocalizeText localize)
        {
            int id = entity.category switch
            {
                ShopCategory.NoAds      => 112,
                ShopCategory.Contract   => 118,
                ShopCategory.BattlePass => 122,
                ShopCategory.Package    => 125,
                ShopCategory.UnitPackage=> 132,
                ShopCategory.Currency01 => 134,
                ShopCategory.Currency02 => 134,
                ShopCategory.Currency03 => 134,
                ShopCategory.Supply     => 138,
                _                       => 0,
            };

            return localize.Get(id);
        }
    }
}
