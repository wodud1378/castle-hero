using Cysharp.Threading.Tasks;

namespace RGLabs.Network.DB.Service
{
    public interface IDBLoadService
    {
        public UniTask<DBCollections> Load();
    }
}