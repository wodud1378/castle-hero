using System;
using Object = UnityEngine.Object;

namespace RGLabs.Common.ResourceManagement
{
    public interface IResource
    {
        public void PreLoad(string path);
        public void Release(string path);
        public void Load<T>(string path, Action<T> onComplete) where T : Object;
    }
}