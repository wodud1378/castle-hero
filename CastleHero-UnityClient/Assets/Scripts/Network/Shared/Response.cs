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
        public Currency leftCurrency;
        public IItem leftItem;
    }
    #endregion

    #region Stage.

    public class StageCleared
    {
        public int stage;
        public int exp;
        public bool isFirstClear;
        public Currency currency;
        public List<IItem> items;
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
        public Currency currency;
        public List<IItem> items;
        public IItem leftItem;
    }

    #endregion

    #region Shop.

    public class ItemBought
    {
        public Currency currency;
        public List<IItem> items;
        public List<UnitInfo> units;
    }

    public class ItemsSold
    {
        public Currency currency;
        public List<IItem> items;
    }

    #endregion
}