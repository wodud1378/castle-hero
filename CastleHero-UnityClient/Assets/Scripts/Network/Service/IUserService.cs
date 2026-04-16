using Cysharp.Threading.Tasks;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    public interface IUserService
    {
        UniTask SaveFormation(FormationDto formation);
        UniTask<Result> UpdateStamina();
        UniTask<Result> UpdateNickname(string nickname);
        UniTask<Result<UserDataDto>> GetUserData();
        UniTask<Result<UserDataDto>> CreateUserData();
    }
}
