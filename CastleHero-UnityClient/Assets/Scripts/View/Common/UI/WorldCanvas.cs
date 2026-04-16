using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Common.UI
{
    [RequireComponent(typeof(Canvas))]
    public class WorldCanvas : MonoBehaviour
    {
        [FormerlySerializedAs("_canvas")]
        [SerializeField] private Canvas canvas;

        private void Awake() => Adjust();

        private void Adjust()
        {
            var rect = (canvas.transform as RectTransform)!;
            var cam = canvas.worldCamera;
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
    }
}
