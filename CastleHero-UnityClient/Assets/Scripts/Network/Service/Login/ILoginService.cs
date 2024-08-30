using BackEnd;
using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Login
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