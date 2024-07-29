using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct EquipItemStatEntity : IEntity
    {
        [DataField("Equip_Status")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        [DataField("Equip_Status_Main_Min")]
        public float[] mainMin;
        
        [DataField("Equip_Status_Main_Max")]
        public float[] mainMax;
        
        [DataField("Equip_Status_Sub_Min")]
        public float[] subMin;
        
        [DataField("Equip_Status_Sub_Max")]
        public float[] subMax;
    }
}