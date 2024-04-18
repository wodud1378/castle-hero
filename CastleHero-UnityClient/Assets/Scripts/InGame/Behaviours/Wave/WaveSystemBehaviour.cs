using System;
using RGLabs.InGame.System;
using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Wave
{
    public class WaveSystemBehaviour : SystemBehaviour
    {
        [SerializeField] private SpawnSystemBehaviour[] _spawnBehaviours;

        [NonSerialized] public bool isRunning;
        
        private IUpdate _waveUpdate;
        
        protected override void OnInit()
        {
            foreach (var behaviour in _spawnBehaviours)
                behaviour.Init();
            
            _waveUpdate = new WaveUpdate();

            isRunning = false;
        }

        protected override void OnUpdate()
        {
            if (!isRunning)
                return;
            
            _waveUpdate.ProcessUpdate(Time.deltaTime);
        }

        private void OnValidate()
        {
            var areas = transform.GetComponentsInChildren<SpawnSystemBehaviour>();
            if (areas.Length == 0)
                return;
            
            Array.Sort(areas, (x, y)=> x.Id.CompareTo(y.Id));
            _spawnBehaviours = areas;
        }
    }
}