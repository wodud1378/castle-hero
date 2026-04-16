using UnityEngine;

namespace CastleHero.GamePlay.InGame.Data
{
    [CreateAssetMenu(fileName = "SpawnConfig", menuName = "ScriptableObjects/SpawnConfig")]
    public class SpawnConfig : ScriptableObject
    {
        public CastleHero.GamePlay.InGame.Behaviours.SpawnAreaSetUp[] areaSetUp;
    }
}