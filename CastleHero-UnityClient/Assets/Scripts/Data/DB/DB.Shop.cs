using System;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("Shop_Table", "shop")]
    public class ShopDB : DB<ShopItemEntity> {}

    [DB("Shop_Rwd_Table", "shop_items")]
    public class ShopItemGroupDB : DB<ShopItemGroupEntity> { }
}