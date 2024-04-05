using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RGLabs.Common.ResourceManagement
{
    public class BuiltInResource : IResource
    {
        private readonly Dictionary<string, Object> _cache = new();

        public void PreLoad(string path)
        {
            _cache[path] = Resources.Load(path);
        }

        public void Release(string path) { }
        
        public void Load<T>(string path, Action<T> onComplete) where T : Object => onComplete.Invoke(Resources.Load<T>(path));
    }
}