using UnityEngine;

namespace CastleHero.Network
{
    public enum NetworkType
    {
        Backend,
        Local,
    }

    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "ScriptableObjects/NetworkConfig")]
    public class NetworkConfig : ScriptableObject
    {
        [SerializeField] private NetworkType type;

        public NetworkType Type => type;

#if UNITY_EDITOR
        public bool openAllDungeons;
#endif

        public static NetworkConfig Current { get; set; }

        public static NetworkConfig Load()
        {
            var config = Resources.Load<NetworkConfig>("NetworkConfig");
            if (config != null)
                Current = config;
            return config;
        }
    }
}
