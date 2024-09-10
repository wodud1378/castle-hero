using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public enum Pattern
    {
        ForEach,
        AtOnce,
    }
    
    public struct SpawnInfo
    {
        public int id;
        public int count;
        public int lv;
        public int area;
        public float timeStep;
    }
    
    public struct WaveEntity : IEntity
    {
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        [DataField("Wave_Grp_ID")]
        public int groupId;

        [DataField("Wave_Grp_Index")]
        public int index;
        
        [DataField("Wave_Grp_Delay")]
        public float startTime;
        
        [DataField("Wave_Grp_Pattern")]
        public int pattern;

        [DataField("Wave_Grp_Spawn")]
        public int[] areas;
        
        [DataField("Wave_Grp_Mob_ID")]
        public int[] ids;
        
        [DataField("Wave_Grp_Mob_Lv")]
        public int[] lvs;
        
        [DataField("Wave_Grp_Mob_Value")]
        public int[] counts;
        
        [DataField("Wave_Grp_Mob_Delay")]
        public float[] timeSteps;
    }
}