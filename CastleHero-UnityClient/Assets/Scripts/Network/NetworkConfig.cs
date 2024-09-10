using UnityEngine;

namespace RGLabs.Network
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "ScriptableObjects/NetworkConfig")]
    public class NetworkConfig : ScriptableObject
    {
#if UNITY_EDITOR
        public bool openAllDungeons;
#endif
    }
}