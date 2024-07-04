using System;

namespace RGLabs.Network.Model
{
    [Serializable]
    public class Consume
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
        public int ItemId { get; }
        public int Quantity { get; }
    }

    [Serializable]
    public class ConsumeResult
    {
        public int id;
        public int consumed;
        public int left;
    }

    [Serializable]
    public class ConsumableItem : IItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        public int consumeOption;
    }

    [Serializable]
    public class EquipItem : IItem
    {
        public string Guid { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        public int character;
        public int slot;
        public int[] stats;
        public float[] values;
    }

    [Serializable]
    public class Item : IItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}