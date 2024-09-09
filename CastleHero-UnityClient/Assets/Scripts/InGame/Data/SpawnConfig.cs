using RGLabs.InGame.Behaviours;
using UnityEngine;

namespace RGLabs.InGame.Data
{
    [CreateAssetMenu(fileName = "SpawnConfig", menuName = "ScriptableObjects/SpawnConfig")]

    public class SpawnConfig : ScriptableObject
    {
        public WaveRunner.SpawnAreaSetUp[] areaSetUp;
    }
}