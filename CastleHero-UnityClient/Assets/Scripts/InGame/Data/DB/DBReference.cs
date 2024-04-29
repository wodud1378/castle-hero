using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Data.DB
{
    [CreateAssetMenu(fileName = "Shared", menuName = "Scriptable Object/Shared")]
    public class DBReference : ScriptableObject
    {
        [Header("Unit")]
        public UnitDB characters;
        public UnitDB monsters;

        [Header("Item")] 
        public ItemDB items;
        
        [Header("Reward")] 
        public RewardDB rewards;

        [Header("Stage")] 
        public StageDB stages;
        public WaveDB[] waveDbs;
    }
}