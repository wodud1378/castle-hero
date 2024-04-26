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
        
        public async UniTask InitAsync(Character data, UnitDB db)
        {
            Data = data;

            if (!db.TryFind(data.id, out var entity))
                return;

            Entity = entity;

            string type = entity.Id / 10000 == 1 ? "Character" : "Monster";
            string iconPath = $"Common/Portrait/{type}_{entity.Id}.png";
            
            await base.InitAsync(iconPath);
        }
    }
}