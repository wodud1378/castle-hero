using System;

namespace RGLabs.Network.Model
{
    [Serializable]
    public class SummonInfo
    {
        public enum EventType
        {
            Common,
            Group,
            SpecificId,
        }

        public ulong id;
        public EventType eventType;
        public int eventParam;

        public int diaPerOnce;
        public int diaPerTenth;
        public float diaDiscountRate;
        
        public int ticketPerOnce;
        public int ticketPerTenth;

        public bool hasDailyTicket;
    }

    public interface ISummonResult
    {
        public int Id { get; }
    }

    public class SummonedUnit : ISummonResult
    {
        public int Id { get; set; }
    }

    public class SummonedSoul : ISummonResult
    {
        public int Id { get; set; }
        public int quantity;
    }
    
    [Serializable]
    public class SummonResult
    {
        public int[] ids;

        public SummonResult()
        {
        }

        public SummonResult(int[] ids)
        {
            this.ids = ids;
        }
    }
}