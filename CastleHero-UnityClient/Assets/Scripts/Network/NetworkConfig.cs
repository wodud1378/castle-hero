using UnityEngine;

namespace CastleHero.Network
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "ScriptableObjects/NetworkConfig")]
    public class NetworkConfig : ScriptableObject
    {
#if UNITY_EDITOR
        public bool openAllDungeons;

        public static NetworkConfig Current { get; set; }
#endif
    }
}
