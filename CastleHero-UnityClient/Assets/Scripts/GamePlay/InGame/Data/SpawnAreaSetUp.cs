using System;
using UnityEngine;

namespace CastleHero.GamePlay.InGame.Behaviours
{
    [Serializable]
    public struct SpawnAreaSetUp
    {
        public int id;
        public Vector2 position;
        public float size;
        public float angle;
    }
}
