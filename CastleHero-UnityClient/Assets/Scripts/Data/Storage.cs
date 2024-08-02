using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Flow;
using RGLabs.Common.Pattern;
using RGLabs.Data.Repositories;
using RGLabs.Network.DB;
using RGLabs.Network.Shared;
using RGLabs.Unit.Factory;
using UnityEngine.AddressableAssets;

namespace RGLabs.Data
{
    public struct Entrance
    {
        public State state;
        public int stage;
    }
    
    public static class Storage
    {
        public static UserRepository userRepository;
        public static InGameRepository inGameRepository = new();
        public static DBCollections db;
        
        public static Entrance entranceData = new() { state = State.Lobby, };

        public static void Init(string nickname, UserData userData, DBCollections database)
        {
            userRepository = new UserRepository(nickname, userData);
            db = database;
        }
    }
}