using UnityEngine;

namespace CastleHero.Utility
{
    public static class UIHelper
    {
        // public static void Attach(this RectTransform self, RectTransform target, Vector2 objPivot)
        // {
        //     self.anchorMin = objPivot;
        //     self.anchorMax = objPivot;
        //     self.pivot = objPivot;
        //     self.position = target.position;
        // }

        public static void Attach(this RectTransform self, RectTransform target, Vector2 pivot)
        {
            var normalizedPosition = GetNormalizedPosition(target);
            Vector2 screenPosition = new Vector2(
                normalizedPosition.x * Screen.width,
                normalizedPosition.y * Screen.height
            );

            // World Position으로 변환
            Vector3 worldPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(self, screenPosition, null, out worldPosition);

            // B의 위치 설정
            self.pivot = pivot;
            self.position = worldPosition;
        }

        private static Vector2 GetNormalizedPosition(RectTransform rectTransform)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
            Vector2 normalizedPosition = new Vector2(
                screenPoint.x / Screen.width, // 가로 축 노멀라이즈 (0 ~ 1)
                screenPoint.y / Screen.height // 세로 축 노멀라이즈 (0 ~ 1)
            );
            return normalizedPosition;
        }
    }
}