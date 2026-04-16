namespace CastleHero.Network
{
    public enum Error
    {
        None,
        FromNetwork,
        Unauthorized,
        Maintenance,
        Unknown,
        FromServer,
        InvalidRequest,

        // Common.
        ChartLoadFailed,
        DBReadFailed,
        DBWriteFailed,
        DataNotFound,
        InvalidData,
        NotEnoughCurrency,
        NotEnoughItem,
        UnitNotFound,

        // InGame.
        NotEnoughAp,
        NotOpened,
        InvalidDayOfWeek,

        // Character.
        AlreadyEquipped,
        NotEquipped,

        // Unit (Castle, Character).
        AlreadyMaxLv,

        // Shop.
        SoldOut,
        SoldOutByType,
        InvalidPaymentType,
        InvalidDate,
        NotOpenedProduct,
    }
}
