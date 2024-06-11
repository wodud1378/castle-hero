using System;

namespace RGLabs.Network.Model
{
    [Serializable]
    public struct ConsumeItem
    {
        public enum Method
        {
            Sell,
            Use,
            Equip,
        }

        public Method method;
        public int id;
        public int quantity;
    }
    
    public interface IItem
    {
        public int Id { get; }
        public int Quantity { get; }
    }

    [Serializable]
    public struct EquipItem : IItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        public int character;
        public int slot;
        public int[] stats;
        public float[] values;
    }

    [Serializable]
    public struct Item : IItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }
}