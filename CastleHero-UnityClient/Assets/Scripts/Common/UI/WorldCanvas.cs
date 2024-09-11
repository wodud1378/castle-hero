using UnityEngine;

namespace RGLabs.Common.UI
{
    [RequireComponent(typeof(Canvas))]
    public class WorldCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        private void Awake() => Adjust();
        
        private void Adjust()
        {
            var rect = (_canvas.transform as RectTransform)!;
            var cam = _canvas.worldCamera;
            // 해상도 가져오기
            float screenHeight = Screen.height;
            float screenWidth = Screen.width;
            var canvasSize = new Vector2(screenWidth, screenHeight);
            rect.sizeDelta = canvasSize;

            // 카메라의 Orthographic 크기나 Field of View에 따른 화면 크기 계산
            float worldScreenHeight = 2f * cam.orthographicSize;
            float worldScreenWidth = worldScreenHeight * cam.aspect;
            
            // 캔버스의 스케일 적용
            rect.localScale = new Vector3(worldScreenWidth / canvasSize.x, worldScreenHeight / canvasSize.y, 1f);;
        }
        
        
#if UNITY_EDITOR
        public bool UPDATE = false;
        
        private void OnValidate()
        {
            if (_canvas == null)
                _canvas = GetComponent<Canvas>();

            if (!UPDATE)
                return;

            Adjust();
            UPDATE = false;
        }
#endif
    }
}