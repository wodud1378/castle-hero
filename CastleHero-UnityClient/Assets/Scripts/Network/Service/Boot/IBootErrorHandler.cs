using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Boot
{
    public interface IBootErrorHandler
    {
        public UniTask OnInitFailed(Response response);
    }
}