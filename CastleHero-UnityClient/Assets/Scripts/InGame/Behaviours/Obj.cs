using RGLabs.Common.Pattern;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public class Obj : MonoBehaviour, IObjectPoolItem
    {
        public void Activate() => gameObject.SetActive(true);

        public void Inactivate() => gameObject.SetActive(false);
    }
}