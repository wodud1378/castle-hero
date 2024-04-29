using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Data.User;

namespace RGLabs.InGame.UI
{
    public class UICharacterSlot : UIItemSlot
    {
        public Character Data { get; private set; }
        public UnitEntity Entity { get; private set; }

        private bool _selected;
        
        public async UniTask InitAsync(Character data, UnitDB db)
        {
            _selected = false;
            Data = data;

            if (!db.TryFind(data.id, out var entity))
                return;

            Entity = entity;

            await base.InitAsync(entity.icon);
        }
    }
}