using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Data.DB
{
    [CreateAssetMenu(fileName = "Shared", menuName = "Scriptable Object/Shared")]
    public class DBReference : ScriptableObject
    {
        public UnitDB characters;
        public UnitDB monsters;
        public Wave[] waves;
    }
}