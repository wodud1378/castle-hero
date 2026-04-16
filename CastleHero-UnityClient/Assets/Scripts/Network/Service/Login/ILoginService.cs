using BackEnd;
using Cysharp.Threading.Tasks;

namespace CastleHero.Network.Service.Login
{
    public enum Platform
    {
        None = 0,
        PlayStore,
        AppStore,
        Guest,
    }
    
    public interface ILoginService
    {
        public UniTask<Result> Login();
    }
}