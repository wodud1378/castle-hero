using System;
using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    [DB("shop")]
    public class ShopDB : DB<ShopItemEntity> {}

    [DB("shop_items")]
    public class ShopItemGroupDB : DB<ShopItemGroupEntity> { }
}