using RGLabs.Common.Behaviours;
using RGLabs.Lobby.Behaviours;
using UniRx;

namespace RGLabs.InGame.Behaviours
{
    public class InGameBehaviour : SceneBehaviour
    {
        private void Awake()
        {
            MessageBroker.Default
                .Receive<StartGame>()
                .Subscribe(Init);
        }

        private void Init(StartGame startGame)
        {
            db = startGame.db;
            userRepo = startGame.userRepo;
            gameRepo = startGame.gameRepo;
            unitFactory = startGame.unitFactory;
            poolContainer = startGame.poolContainer;
        }
    }
}