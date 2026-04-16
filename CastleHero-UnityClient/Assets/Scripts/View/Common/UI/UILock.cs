using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.Common.UI
{
    public class UILock : MonoBehaviour
    {
        [FormerlySerializedAs("_image")]
        [SerializeField] private Image image;

        private readonly Dictionary<string, int> _requests = new();

        public void Set(string key)
        {
            _requests.TryAdd(key, 0);

            ++_requests[key];

            UpdateLock();
        }

        public void Release(string key)
        {
            --_requests[key];

            if (_requests[key] > 0)
                return;

            _requests.Remove(key);

            UpdateLock();
        }

        private void UpdateLock()
        {
            int left = _requests.Sum((x) => x.Value);

            image.raycastTarget = left > 0;
        }
    }
}
