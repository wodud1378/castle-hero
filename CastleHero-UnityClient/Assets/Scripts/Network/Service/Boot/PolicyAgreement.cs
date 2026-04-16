using Newtonsoft.Json;

namespace CastleHero.Network.Service.Boot
{
    public struct PolicyAgreement
    {
        public bool terms;
        public bool privacy;
        public bool share;
        public bool push;
        public bool nightPush;

        [JsonIgnore] public bool updated;
    }
}
