using Cysharp.Threading.Tasks;
using PolyNav;
using CastleHero.Common;
using CastleHero.Common.Localize;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Factory;
using CastleHero.Data.Factory;
using CastleHero.Network.Service;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Behaviours
{
    /// <summary>
    /// 배치(Formation) 영역의 Scene 의존 파사드.
    /// 실제 배치 로직은 <see cref="FormationComposer"/>, 리포지토리 동기화는 <see cref="FormationSyncer"/> 가 담당한다.
    /// </summary>
    public class FormationField : MonoBehaviour, IFormationFieldArea
    {
        [field: SerializeField] public float Radius { get; private set; }

        [FormerlySerializedAs("_map")]
        [SerializeField] private PolyNavMap map;
        [FormerlySerializedAs("_autoPlacementRadius")]
        [SerializeField] private float autoPlacementRadius;

        private readonly Collider2D[] _buffer = new Collider2D[Constants.BufferSize];

        private FormationComposer _composer;
        private FormationSyncer _syncer;

        public FormationDraft Draft => _composer.Draft;
        public IReadOnlyReactiveProperty<int> capacity => _composer.Capacity;
        public IReadOnlyReactiveProperty<int> placed => _composer.Placed;

        float IFormationFieldArea.AutoPlacementRadius => autoPlacementRadius;

        public async UniTask Init()
        {
            var userRepo = ServiceLocator.Get<IUserRepository>();
            var db = ServiceLocator.Get<IDBProvider>();

            _composer = new FormationComposer(
                area: this,
                unitFactory: ServiceLocator.Get<IUnitFactory>(),
                castleFactory: ServiceLocator.Get<ICastleFactory>(),
                userRepo: userRepo,
                db: db,
                popups: ServiceLocator.Get<IPopupManager>(),
                sounds: ServiceLocator.Get<ISoundManager>(),
                localize: ServiceLocator.Get<LocalizeText>(),
                soundPath: ServiceLocator.Get<SoundPath>(),
                network: ServiceLocator.Get<INetworkServiceProvider>());

            _syncer = new FormationSyncer(_composer, userRepo, db);
            _syncer.Start();

            if (db.Castles.TryFind(userRepo.GameRecord.CastleLv.Value, out var entity))
                _composer.ApplyCastleCapacity(entity.maxCharacter, entity.barricadeCount);

            await _composer.LoadSavedUnits();
            _composer.ApplyBarricadesToMap();
        }

        public UniTask AutoPlacement() => _composer.AutoPlacement();

        public void Clear() => _composer.Clear();

        public void Remove(UnitBehaviour unit) => _composer.Remove(unit);

        public bool TryRegister(UnitBehaviour unit, int layer, bool isExist) =>
            _composer.TryRegister(unit, layer, isExist);

        public bool IsValid(Collider2D col, int layer)
        {
            var layerMask = new LayerMask { value = 1 << layer };
            var distance = Vector2.Distance(col.transform.position, transform.position);
            if (distance > Radius)
                return false;

            int overlapped = Physics2D.OverlapCollider(col, new ContactFilter2D { layerMask = layerMask }, _buffer);
            return overlapped <= 0;
        }

        public bool InArea(Vector2 position) => Vector2.Distance(transform.position, position) <= Radius;

        void IFormationFieldArea.AddObstacle(UnitBehaviour unit, bool regenerateMap) =>
            map.AddObstacle(unit, regenerateMap);

        void IFormationFieldArea.RemoveObstacle(UnitBehaviour unit) =>
            map.RemoveObstacle(unit);

        void IFormationFieldArea.GenerateMap() => map.GenerateMap();

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        private void OnDestroy()
        {
            _syncer?.Dispose();
            _composer?.Dispose();
        }
    }
}
