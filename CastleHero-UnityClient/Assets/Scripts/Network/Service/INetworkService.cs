using RGLabs.Network.Service.Query;

namespace RGLabs.Network.Service
{
    public interface INetworkService
    {
        public QueryBase Query { get; }
    }
}