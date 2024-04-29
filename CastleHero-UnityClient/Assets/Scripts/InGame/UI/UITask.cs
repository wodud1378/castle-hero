using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public abstract class UITask<T> : MonoBehaviour
    {
        public abstract UniTask<T> RunTask();
    }
}