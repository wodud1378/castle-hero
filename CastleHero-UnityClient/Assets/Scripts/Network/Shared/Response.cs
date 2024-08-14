using System.Collections.Generic;

namespace RGLabs.Network.Shared
{
    #region Character
    /// <summary>
    /// 각 int[] 필드의 인덱스 0 : 이전 값, 인덱스 1: 갱신된 값
    /// </summary>
    public class UnitTransition
    {
        public int[] rateTransition;
        public int[] lvTransition;
        public int[] expTransition;
        public UnitInfo unit;
    }

    public class GrowthResult
    {
        public UnitTransition transition;
        public CurrencyDto leftCurrency;
        public IItem leftItem;
    }
    #endregion
    
    #region Castle
    public class CastleGrowth
    {
        public int lv;
        public CurrencyDto leftCurrency;
    }
    #endregion

    #region InGame.
    public class Cleared
    {
        public int id;
        public bool isFirstClear;
        public CurrencyDto currency;
        public List<IItem> items;
    }

    public class DungeonCleared : Cleared { }

    public class StageCleared : Cleared
    {
        public int exp;
        public List<UnitTransition> transitions;
    }
    #endregion

    #region Summon.

    public interface ISummoned
    {
        public int Id { get; }
    }

    public class SummonedUnit : ISummoned
    {
        public int Id { get; set; }
        public int lv;
        public int rate;
    }

    public class SummonedSoul : ISummoned
    {
        public int Id { get; set; }
        public int quantity;
    }

    public class SummonResult
    {
        public List<ISummoned> summoneds;
    }

    #endregion

    #region Item.

    public class OpenBoxResult
    {
        public CurrencyDto currency;
        public List<IItem> items;
        public IItem leftItem;
    }

    #endregion

    #region Shop.

    public class ItemBought
    {
        public int shopId;
        public CurrencyDto currency;
        public List<IItem> items;
        public List<int> unitIds;
    }

    public class ItemsSold
    {
        public CurrencyDto currency;
        public List<IItem> items;
    }

    #endregion
}