using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Common.Pattern
{
    public class AddressablePool<T> where T : MonoBehaviour, IObjectPoolItem
    {
        private readonly List<T> _activated;
        private readonly Queue<T> _spares;

        private readonly string _path;

        private bool HasSpare => _spares.Count > 0;

        public AddressablePool(string path)
        {
            _path = path;
            _activated = new List<T>();
            _spares = new Queue<T>();
        }

        public async UniTask<T> Get()
        {
            T obj;
            if (HasSpare)
                obj = _spares.Dequeue();
            else
            {
                var go = await Addressables.InstantiateAsync(_path);
                obj = go.GetComponent<T>();
            }

            obj.Activate();
            _activated.Add(obj);

            return obj;
        }

        public void Release(T obj)
        {
            obj.Inactivate();
            Addressables.ReleaseInstance(obj.gameObject);
        }
    }
}