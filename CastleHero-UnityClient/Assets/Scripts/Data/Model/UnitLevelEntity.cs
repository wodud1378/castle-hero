using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct UnitLevelEntity : IEntity
    {
        [DataField("Character_ID")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        [DataField("Character_Hp_Up")]
        public float hp;
        
        [DataField("Character_Atk_Up")]
        public float atk;
        
        [DataField("Character_Cri_Up")]
        public float critical;
        
        [DataField("Character_Cri_Damage_Up")]
        public float criticalAtk;
        
        [DataField("Character_Speed_Atk_Up")]
        public float atkSpeed;
        
        [DataField("Character_Speed_Move_Up")]
        public float speed;
        
        [DataField("Character_Range_Atk_Up")]
        public float atkRange;
        
        [DataField("Character_Range_Move_Up")]
        public float moveRange;

        [DataField("Rate_1_option")]
        public int[] rate1Options;

        [DataField("Rate_1_option_Value")]
        public float[] rate1OptionValues;
        
        [DataField("Rate_2_option")]
        public int[] rate2Options;

        [DataField("Rate_2_option_Value")]
        public float[] rate2OptionValues;
        
        [DataField("Rate_3_option")]
        public int[] rate3Options;

        [DataField("Rate_3_option_Value")]
        public float[] rate3OptionValues;
        
        [DataField("Rate_4_option")]
        public int[] rate4Options;

        [DataField("Rate_4_option_Value")]
        public float[] rate4OptionValues;
        
        [DataField("Rate_5_option")]
        public int[] rate5Options;

        [DataField("Rate_5_option_Value")]
        public float[] rate5OptionValues;

        public int[][] rateOptions;
        public float[][] rateValues;
    }
}