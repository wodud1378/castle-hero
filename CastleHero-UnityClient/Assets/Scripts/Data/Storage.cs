using RGLabs.Common.Flow;
using RGLabs.Common.Localize;
using RGLabs.Common.Sound;
using RGLabs.Data.Repositories;
using RGLabs.Network.DB;

namespace RGLabs.Data
{
    public struct Entrance
    {
        public enum Link
        {
            None,
            LevelUp,
            Equipment,
            RateUp,
        }
        
        public State state;
        public Link link;
        public GameEntrance gameEntrance;
    }
    
    public static class Storage
    {
        public static readonly SettingRepository settingRepository = new();
        public static readonly InGameRepository inGameRepository = new();
        
        public static UserRepository userRepository;
        public static DBCollections db;
        public static LocalizeText localize;
        public static SoundPath soundPath;
        
        public static Entrance entranceData = new() { state = State.Lobby, };

        public static void ClearRepositories()
        {
            settingRepository.Dispose();
            inGameRepository.Dispose();
            userRepository.Dispose();
        }
    }
}