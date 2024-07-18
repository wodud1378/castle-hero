using System.Collections.Generic;

namespace RGLabs.Network.Shared
{
    #region Character

    public class UnitResult
    {
        public UnitInfo unit;
    }

    public class GrowthResult : UnitResult
    {
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
        public List<UnitInfo> updated;
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
    }

    #endregion
}