using System;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.System.Wave
{
    public class TempHelper : MonoBehaviour
    {
        [SerializeField] private Shared _shared;
        
        private void Update()
        {
            _shared.spawnDataStream.ProcessUpdate(0f);
        }
    }
}