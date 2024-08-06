using RGLabs.Common.Flow;
using RGLabs.Common.Localize;
using RGLabs.Data.Repositories;
using RGLabs.Network.DB;

namespace RGLabs.Data
{
    public struct Entrance
    {
        public State state;
        public int stage;
    }
    
    public static class Storage
    {
        public static readonly SettingRepository settingRepository = new();
        public static readonly InGameRepository inGameRepository = new();
        
        public static UserRepository userRepository;
        public static DBCollections db;
        public static LocalizeText localize;
        
        public static Entrance entranceData = new() { state = State.Lobby, };
    }
}