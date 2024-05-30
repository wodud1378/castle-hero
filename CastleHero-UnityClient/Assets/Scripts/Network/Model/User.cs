using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLabs.Network.Model
{
    [Serializable]
    public class UserInfo
    {
        public UnitInfo[] characters;
        public FieldCharacter[] fieldCharacters;

        public IItem[] items;
    }
    
    [Serializable]
    public class FieldCharacter
    {
        public int index;
        public Vector2 position;
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
        public enum ResultCode
        {
            Success,
            Failed,
        }

        public ResultCode Result { get; }
        public UnitInfo Info { get; }
    }

    [Serializable]
    public class CharacterLevelUp : IModifyCharacter
    {
        public IModifyCharacter.ResultCode Result { get; }
        public UnitInfo Info { get; }
    }
    
    [Serializable]
    public class CharacterUpgrade : IModifyCharacter
    {
        public IModifyCharacter.ResultCode Result { get; }
        public UnitInfo Info { get; }
    }
}