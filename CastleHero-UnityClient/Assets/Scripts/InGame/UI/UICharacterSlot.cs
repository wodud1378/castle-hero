using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.DB;
using RGLabs.Data.Model;
using RGLabs.Data.User;

namespace RGLabs.InGame.UI
{
    public class UICharacterSlot : UIItemSlot
    {
        public Character Data { get; private set; }
        public UnitEntity Entity { get; private set; }
 
        public async UniTask InitAsync(Character data, UnitDB db)
        {
            Data = data;

            if (!db.TryFind(data.id, out var entity))
                return;

            Entity = entity;

            await base.InitAsync(entity.icon);
        }
    }
}