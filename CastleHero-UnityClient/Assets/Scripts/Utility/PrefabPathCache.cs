using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RGLabs.Utility
{
    public class PrefabPathAttribute : Attribute
    {
        public string Path { get; }

        public PrefabPathAttribute(string path) => Path = path;
    }
    
    public static class PrefabPathCache
    {
        private static Dictionary<Type, string> _cache;

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            var assembly = Assembly.Load("Assembly-CSharp");
            _cache = assembly.GetTypes()
                .Where(x => x.IsClass && x.GetCustomAttribute<PrefabPathAttribute>() != null)
                .ToDictionary(x => x, y => y.GetCustomAttribute<PrefabPathAttribute>().Path);
        }

        public static string Load(Type type) => _cache.GetValueOrDefault(type);
    }
}