using System.Linq;
using CastleHero.Data.Repositories;

namespace CastleHero.Data.UseCases
{
    /// <summary>
    /// 던전 진행도 계산 유틸.
    /// </summary>
    public static class DungeonHelper
    {
        /// <summary>
        /// 해당 layer 의 진입 가능한 최대 레벨을 반환한다.
        /// 클리어 기록이 없으면 1, 있으면 lastClearedLv + 1 과 maxLv 중 작은 값.
        /// </summary>
        public static int GetAvailableLv(IUserRepository userRepo, int layer, int maxLv)
        {
            var record = userRepo.GameRecord.Dungeon
                .FirstOrDefault(x => x.layer == layer);

            return record != null
                ? UnityEngine.Mathf.Min(record.lastClearedLv + 1, maxLv)
                : 1;
        }
    }
}
