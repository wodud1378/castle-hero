using UniRx;

namespace RGLabs.InGame.Data.Repositories
{
    public class UserRepository
    {
        public readonly ReactiveProperty<int[]> characters = new();
    }
}