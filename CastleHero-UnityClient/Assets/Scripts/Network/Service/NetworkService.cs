using RGLabs.Network.Service.Castle;
using RGLabs.Network.Service.Character;
using RGLabs.Network.Service.Item;
using RGLabs.Network.Service.Stage;
using RGLabs.Network.Service.Summon;
using RGLabs.Network.Service.Test;
using RGLabs.Network.Service.User;

namespace RGLabs.Network.Service
{
    public static class NetworkService
    {
        public static readonly UserService User = new();
        public static readonly StageService Stage = new();
        public static readonly CharacterService Character = new();
        public static readonly ItemService Item = new();
        public static readonly SummonService Summon = new();
        public static readonly CastleService Castle = new();

        public static readonly TestService Test = new();
    }
}