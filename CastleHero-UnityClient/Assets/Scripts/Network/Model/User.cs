using System;
using System.Collections.Generic;

namespace RGLabs.Network.Model
{
    [Serializable]
    public class UserInfo
    {
        public int stage;
        public int focusedStage;
        public int castleLv;
        
        public int gold;
        public int freeDia;
        public int paidDia;
        
        public List<UnitInfo> characters;
        public List<FieldCharacter> fieldCharacters;
        public List<IItem> items;
    }
    
    [Serializable]
    public class FieldCharacter
    {
        public int id;
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