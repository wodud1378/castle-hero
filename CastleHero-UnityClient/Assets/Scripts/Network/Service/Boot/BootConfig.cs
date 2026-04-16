using UnityEngine;

namespace CastleHero.Network.Service.Boot
{
    [CreateAssetMenu(fileName = "BootConfig", menuName = "ScriptableObjects/BootConfig")]
    public class BootConfig : ScriptableObject
    {
#if UNITY_EDITOR
        public bool deleteGuestId;
#endif
    }
}