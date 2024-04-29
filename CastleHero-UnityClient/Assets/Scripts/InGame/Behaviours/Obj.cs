using RGLabs.Common.Pattern;
using RGLabs.InGame.System;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public abstract class Obj : MonoBehaviour, IObjectPoolItem
    {
        public PoolContainer Container { get; set; }
        
        public string ResourcePath { get; set; }
        
        protected virtual void OnActivate()
        {
            gameObject.SetActive(true);
        }

        protected virtual void OnInactivate()
        {
            gameObject.SetActive(false);
        }

        public void Activate() => OnActivate();

        public void Inactivate() => OnInactivate();

        public void DestroySelf()
        {
            if (Container == null)
                return;
            
            Container.Release(this);
        }
    }
}