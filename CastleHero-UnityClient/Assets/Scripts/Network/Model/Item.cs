using System;

namespace RGLabs.Network.Model
{
    [Serializable]
    public struct Consume
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
        public int ItemId { get; }
        public int Quantity { get; }
    }

    [Serializable]
    public struct ConsumableItem : IItem
    {
        public int Id { get;set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        public int consumeOption;
    }

    [Serializable]
    public struct EquipItem : IItem
    {
        public int Id { get;set; }
        public int ItemId { get; set; }
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
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}