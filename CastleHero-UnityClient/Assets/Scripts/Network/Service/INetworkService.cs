using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service
{
    public interface INetworkService
    {
        // 로그인
        public UniTask<UserInfo> Login();

        public UniTask LoadTables();
        
        // 캐릭터
        public UniTask<CharacterLevelUp> LevelUp(int id, IEnumerable<ConsumeItem> items);
        public UniTask<CharacterUpgrade> Upgrade(int id, IEnumerable<ConsumeItem> items);
        
        // 아이템
        public UniTask Consume(IEnumerable<ConsumeItem> items);
        public UniTask Consume(ConsumeItem items);
        
        // 스테이지
        public UniTask<StageClear> StageClear(int stage);
    }
}