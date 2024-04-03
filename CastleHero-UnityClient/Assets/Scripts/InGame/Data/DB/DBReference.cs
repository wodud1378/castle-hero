using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Data.DB
{
    [CreateAssetMenu(fileName = "Shared", menuName = "Scriptable Object/Shared")]
    public class DBReference : ScriptableObject
    {
        public MonsterDB monsters;
        public System.Wave.Wave[] waves;
    }
}