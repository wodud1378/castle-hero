using Cysharp.Threading.Tasks;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service.Stage
{
    public interface IStageService
    {
        public UniTask<StageClear> StageClear(string inDate, int stage);
        public UniTask SetStageFocus(string inDate, int stage);

        public UniTask<(int focused, int latest)> LoadStageInfo(string inDate);
    }
}