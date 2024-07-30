using UnityEngine;

namespace RGLabs.Network.Service.Boot
{
    [CreateAssetMenu(fileName = "BootConfig", menuName = "ScriptableObjects/BootConfig")]
    public class BootConfig : ScriptableObject
    {
        public bool useLocalDatabase;
    }
}