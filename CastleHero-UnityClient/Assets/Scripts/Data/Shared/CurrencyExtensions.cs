using System.Collections.Generic;

namespace CastleHero.Network.Shared
{
    /// <summary>
    /// CurrencyDto 의 비즈니스 로직 확장. DTO 와 동작을 분리하기 위해 별도 파일.
    /// </summary>
    public static class CurrencyExtensions
    {
        public static List<IItem> ToItems(this CurrencyDto c)
            => new()
            {
                new Item { ItemId = CastleHero.Constants.PaidDiaId, Quantity = c.paidDia },
                new Item { ItemId = CastleHero.Constants.FreeDiaId, Quantity = c.freeDia },
                new Item { ItemId = CastleHero.Constants.GoldId, Quantity = c.gold },
            };

        public static bool IsEmpty(this CurrencyDto c)
            => c.gold == 0 && c.freeDia == 0 && c.paidDia == 0;

        public static bool TryConsumeDia(this CurrencyDto c, int amount)
        {
            if (c.freeDia + c.paidDia < amount)
                return false;

            if (c.freeDia > amount)
            {
                c.freeDia -= amount;
            }
            else
            {
                int remain = amount - c.freeDia;
                c.freeDia = 0;
                c.paidDia -= remain;
            }

            return true;
        }

        public static bool TryConsume(this CurrencyDto c, int id, int amount)
        {
            switch (id)
            {
                case CastleHero.Constants.PaidDiaId:
                case CastleHero.Constants.FreeDiaId:
                    return c.TryConsumeDia(amount);

                case CastleHero.Constants.GoldId:
                    if (amount > c.gold)
                        return false;

                    c.gold -= amount;
                    return true;
                default:
                    return false;
            }
        }
    }
}
