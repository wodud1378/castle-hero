using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public abstract class SystemBehaviour : MonoBehaviour
    {
        private bool _initialized = false;
        
        public void Init()
        {
            OnInit();
            
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized)
                return;
            
            OnUpdate();
        }

        protected abstract void OnInit();
        
        protected virtual void OnUpdate() { }
    }
}