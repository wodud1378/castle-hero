using System;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;

namespace RGLabs.Utility
{
    public static class ShopHelper
    {
        public static bool IsSoldOut(ShopItemEntity entity, Product product, out bool byDefault, out bool byAd,
            out bool byFree)
        {
            if (entity.totalCount == 0)
            {
                byFree = true;
                byAd = true;
                byDefault = false;
                return false;
            }
            
            var count = product?.current ?? new Product.BuyCount
            {
                byDefault = 0,
                byAd = 0,
                byFree = 0
            };

            if (count.Sum >= entity.totalCount)
            {
                byFree = true;
                byAd = true;
                byDefault = true;
                return true;
            }

            byFree = count.byFree >= entity.countForFree;
            byAd = count.byAd >= entity.countForAd;
            byDefault = count.byDefault >= (entity.totalCount - entity.countForFree - entity.countForAd);

            return byDefault && byAd && byFree;
        }
        
        public static PaymentType GetPaymentType(ShopItemEntity entity, Product product, out int left, out int limit)
        {
            limit = 0;
            left = 0;
            var count = product.GetBuyCount();
            var type = GetPaymentType(entity, product);
            switch (type)
            {
                case PaymentType.Default:
                    limit = entity.totalCount - count.byFree - count.byAd;
                    left = entity.totalCount - count.Sum;
                    break;
                case PaymentType.Ad:
                    limit = entity.countForAd;
                    left = entity.countForAd - count.byAd;
                    break;
                case PaymentType.Free:
                    limit = entity.countForFree;
                    left = entity.countForFree - count.byFree;
                    break;
            }

            return type;
        }

        public static void GetBuyCount(ShopItemEntity entity, Product product, out int left, out int limit)
        {
            limit = 0;
            left = 0;
            var count = product.GetBuyCount();
            var type = GetPaymentType(entity, product);
            switch (type)
            {
                case PaymentType.Default:
                    limit = entity.totalCount - count.byFree - count.byAd;
                    left = entity.totalCount - count.Sum;
                    break;
                case PaymentType.Ad:
                    limit = entity.countForAd;
                    left = entity.countForAd - count.byAd;
                    break;
                case PaymentType.Free:
                    limit = entity.countForFree;
                    left = entity.countForFree - count.byFree;
                    break;
            }
        }
        
        public static PaymentType GetPaymentType(ShopItemEntity entity, Product product)
        {
            var count = product.GetBuyCount();
            if (entity.countForFree > count.byFree)
                return PaymentType.Free;

            if (entity.countForAd > count.byAd)
                return PaymentType.Ad;

            return PaymentType.Default;
        }

        private static Product.BuyCount GetBuyCount(this Product product)
        {
            return product?.current ?? new Product.BuyCount
            {
                byDefault = 0,
                byAd = 0,
                byFree = 0
            };
        }
    }
}