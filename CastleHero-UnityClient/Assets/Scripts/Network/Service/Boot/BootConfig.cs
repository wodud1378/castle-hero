using UnityEngine;

namespace RGLabs.Network.Service.Boot
{
    [CreateAssetMenu(fileName = "BootConfig", menuName = "ScriptableObjects/BootConfig")]
    public class BootConfig : ScriptableObject
    {
        public bool useLocalDatabase;
        
#if UNITY_EDITOR
        public bool deleteGuestId;
#endif
    }
}