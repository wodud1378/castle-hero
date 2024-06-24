using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service.Summon
{
    public interface ISummonService
    {
        public SummonEntity Entity { get; }
        
        public UniTask<ISummonResult> SummonOnce();

        public UniTask<ISummonResult[]> SummonTenth();
    }
}