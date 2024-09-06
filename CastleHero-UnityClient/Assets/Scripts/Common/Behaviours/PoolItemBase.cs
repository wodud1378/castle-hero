using RGLabs.Common.Pattern;
using UnityEngine;

namespace RGLabs.Common.Behaviours
{
    public abstract class PoolItemBase : MonoBehaviour, IObjectPoolItem
    {
        public PoolContainer Container { get; set; }
        
        public AddressablePool<PoolItemBase> Pool { get; set; }

        public bool Activated => gameObject.activeSelf;
        
        public string ResourcePath { get; set; }
        
        protected virtual void OnActivate()
        {
            gameObject.SetActive(true);
        }

        protected virtual void OnInactivate()
        {
            gameObject.SetActive(false);
        }

        public void ForceActivate() => Pool.ForceActivate(this);
        
        public void Activate() => OnActivate();

        public void Inactivate() => OnInactivate();

        public void DestroySelf() => Container?.Release(this);
    }
}