using RGLabs.Common.Pattern;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public abstract class Obj : MonoBehaviour, IObjectPoolItem
    {
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
    }
}