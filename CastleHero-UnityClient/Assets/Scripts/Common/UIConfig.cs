using UnityEngine;

namespace RGLabs.Common
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "ScriptableObjects/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        public enum DeviceType
        {
            None,
            Pixel5,
            GalaxyS21,
            GalaxyNote10,
            IPhone12Pro
        }
        
#if UNITY_EDITOR
        public DeviceType uiSampleDevice;
#endif
    }
}