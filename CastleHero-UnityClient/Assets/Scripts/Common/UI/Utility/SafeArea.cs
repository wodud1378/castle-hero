using System;
using UnityEngine;

namespace RGLabs.Common.UI.Utility
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private UIConfig _config;
        [SerializeField] private RectTransform _target;

        private void OnEnable()
        {
            Adjust();
        }

        private void Adjust()
        {
            var safeArea = GetSafeArea();
            var minAnchor = safeArea.position;
            var maxAnchor = minAnchor + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            _target.anchorMin = minAnchor;
            _target.anchorMax = maxAnchor;
        }

        private Rect GetSafeArea()
        {
#if UNITY_EDITOR
            return SampleSafeArea(_config.uiSampleDevice);
#endif
            return Screen.safeArea;
        }

#if UNITY_EDITOR

        private Rect SampleSafeArea(UIConfig.DeviceType device)
        {
            // Safe Area 샘플 데이터
            switch (device)
            {
                case UIConfig.DeviceType.Pixel5:
                    // Pixel 5의 가상 Safe Area (예시)
                    return new Rect(0, 100, Screen.width, Screen.height - 100);
                case UIConfig.DeviceType.GalaxyS21:
                    // Galaxy S21의 가상 Safe Area (예시)
                    return new Rect(0, 150, Screen.width, Screen.height - 150);
                case UIConfig.DeviceType.GalaxyNote10:
                    // Galaxy Note 10의 가상 Safe Area (예시)
                    return new Rect(0, 120, Screen.width, Screen.height - 120);
                case UIConfig.DeviceType.IPhone12Pro:
                    // iPhone 12 Pro의 가상 Safe Area (예시)
                    return new Rect(0, 200, Screen.width, Screen.height - 200);
                default:
                    // 기본 값 (Safe Area가 없는 경우)
                    return new Rect(0, 0, Screen.width, Screen.height);
            }
        }
        
        private void OnValidate()
        {
            if (_target == null)
                _target = transform as RectTransform;
        }
#endif
    }
}