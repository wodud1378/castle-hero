using System;

namespace CastleHero.Utility
{
    /// <summary>
    /// 서버 시간 기준 UTC+3 오프셋을 중앙화합니다.
    /// AddHours(3) 하드코딩을 직접 사용하지 말고 이 클래스를 통해 사용하세요.
    /// </summary>
    public static class ServerTime
    {
        private static readonly TimeSpan ServerOffset = TimeSpan.FromHours(3);

        public static DateTime Now => DateTime.UtcNow + ServerOffset;

        public static DateTime ToServerTime(DateTime utcTime) => utcTime + ServerOffset;
    }
}
