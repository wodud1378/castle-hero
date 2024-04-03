using System;
using RGLabs.InGame.System;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.System
{
    public abstract class SystemHandler<T> : MonoBehaviour where T: ISystem
    {
        [SerializeField] private T _system;

        public void Awake()
        {
            _system.Init();
        }

        private void Update()
        {
            _system.ProcessUpdate(Time.deltaTime);
        }
    }
}