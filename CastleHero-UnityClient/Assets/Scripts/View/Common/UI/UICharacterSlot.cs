using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Common.UI
{
    public class UICharacterSlot : UISlot
    {
        public UnitInfo Info { get; private set; }

        private IDBProvider _db;

        protected override void OnAwake()
        {
            base.OnAwake();
            _db = ServiceLocator.Instance.Get<IDBProvider>();
        }

        public void Init(UnitInfo info, UnitEntity entity)
        {
            Info = info;

            Init(entity.icon, $"Lv.{info.lv}");
        }

        public void Init(UnitInfo info)
        {
            if (!_db.Units.TryFind(info.id, out var entity))
                return;

            Init(info, entity);
        }
    }
}