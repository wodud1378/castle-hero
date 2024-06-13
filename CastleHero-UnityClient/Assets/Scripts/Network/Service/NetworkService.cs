using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service
{
    public class NetworkService : INetworkService
    {
        private readonly BackendWrapper _wrapper;
        
        public UniTask Init()
        {
            throw new System.NotImplementedException();
        }

        public UniTask<UserInfo> Login()
        {
            throw new System.NotImplementedException();
        }

        public UniTask LoadTables()
        {
            throw new System.NotImplementedException();
        }

        public UniTask<CharacterLevelUp> LevelUp(int id, IEnumerable<Consume> items)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<CharacterUpgrade> Upgrade(int id, IEnumerable<Consume> items)
        {
            throw new System.NotImplementedException();
        }

        public UniTask Consume(IEnumerable<Consume> items)
        {
            throw new System.NotImplementedException();
        }

        public UniTask Consume(Consume items)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<StageClear> StageClear(int stage)
        {
            throw new System.NotImplementedException();
        }

        public UniTask SetStage(int stage)
        {
            throw new System.NotImplementedException();
        }
    }
}