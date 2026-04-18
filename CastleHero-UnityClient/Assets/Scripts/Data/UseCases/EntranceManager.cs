using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;

namespace CastleHero.Data.UseCases
{
    /// <summary>
    /// Entrance(스테이지/던전 입장 정보) 변경 및 스테이지 진행도 검증 유틸.
    /// </summary>
    public class EntranceManager
    {
        private readonly IUserRepository _userRepo;
        private readonly IDBProvider _db;

        public EntranceManager(IServiceLocator sl)
        {
            _userRepo = sl.Get<IUserRepository>();
            _db = sl.Get<IDBProvider>();
        }

        /// <summary>
        /// 던전 입장 상태를 스테이지로 복귀시킨다.
        /// </summary>
        public void RevertToStageIfDungeon()
        {
            var prop = _userRepo.Entrance;
            if (prop.Value.type == GameType.Dungeon)
            {
                prop.Value = new GameEntrance
                {
                    type = GameType.Stage,
                    id = _userRepo.StageFocus
                };
            }
        }

        /// <summary>
        /// 로비 진입 시 던전 입장 상태라면 스테이지로 복귀시킨다.
        /// </summary>
        public void RevertToStageOnLobbyInit(EntranceHolder entranceHolder)
        {
            var exist = _userRepo.Entrance.Value;
            if (exist.type == GameType.Dungeon && entranceHolder.Current.state == CastleHero.Common.Flow.State.Lobby)
            {
                _userRepo.Entrance.Value = new GameEntrance
                {
                    type = GameType.Stage,
                    id = _userRepo.StageFocus
                };
            }
        }

        /// <summary>
        /// 현재 스테이지에서 인덱스를 조정하여 이동한다.
        /// </summary>
        public void AdjustStageIndex(int currentStageId, int adjust)
        {
            var stages = _db.Stages;
            if (!stages.TryFindIndex(currentStageId, out int index))
                return;

            index += adjust;
            if (!stages.IsValidIndex(index))
                return;

            if (!stages.TryIndexOf(index, out var entity))
                return;

            _userRepo.Entrance.Value = new GameEntrance
            {
                type = GameType.Stage,
                id = entity.Id
            };
        }
    }
}
