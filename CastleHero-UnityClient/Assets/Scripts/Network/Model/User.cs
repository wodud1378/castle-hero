using System;

namespace RGLabs.Network.Model
{
    [Serializable]
    public class UserInfo
    {
        public int stage;
        public int castleLv;
        
        public UnitInfo[] characters;
        public FieldCharacter[] fieldCharacters;

        public IItem[] items;
    }
    
    [Serializable]
    public class FieldCharacter
    {
        public int index;
        public float x;
        public float y;
    }
    
    [Serializable]
    public class UnitInfo
    {
        public int id;
        public int lv;
        public int rate;
        public int exp;

        public EquipItem[] equipments;
    }

    public interface IModifyCharacter
    {
        public enum FailedCauses
        {
            None,
            AlreadyMaxValue,
            NotEnoughItem,
            Unknown,
        }

        public FailedCauses FailedCause { get; }
        public UnitInfo Info { get; }
        public ConsumableItem ItemResult { get; }
        public int GoldResult { get; }
    }

    [Serializable]
    public class LevelUpResult : IModifyCharacter
    {
        public IModifyCharacter.FailedCauses FailedCause { get; set; }
        public UnitInfo Info { get; set; }
        public ConsumableItem ItemResult { get; set; }
        public int GoldResult { get; set; }
    }
    
    [Serializable]
    public class UpgradeResult : IModifyCharacter
    {
        public IModifyCharacter.FailedCauses FailedCause { get; set; }
        public UnitInfo Info { get; set; }
        public ConsumableItem ItemResult { get; set; }
        public int GoldResult { get; set; }
    }
}