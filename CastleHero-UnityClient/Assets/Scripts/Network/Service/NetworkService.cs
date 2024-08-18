using System;
using RGLabs.Network.Service.Test;

namespace RGLabs.Network.Service
{
    public static class NetworkService
    {
        public static readonly UserService User = new();
        public static readonly GameService Game = new();
        public static readonly CharacterService Character = new();
        public static readonly ItemService Item = new();
        public static readonly ShopService Shop = new();
        public static readonly SummonService Summon = new();
        public static readonly CastleService Castle = new();
        
        public static readonly TestService Test = new();
        
        public static DateTime CurrentTime() => DateTime.UtcNow.AddHours(3);
    }
}