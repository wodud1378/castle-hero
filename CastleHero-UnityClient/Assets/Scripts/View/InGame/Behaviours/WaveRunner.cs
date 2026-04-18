using System;
using System.Linq;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.InGame;
using CastleHero.GamePlay.InGame.Behaviours;
using CastleHero.GamePlay.InGame.Data;
using CastleHero.GamePlay.InGame.System;
using CastleHero.GamePlay.InGame.System.Wave;
using CastleHero.Data.DB;
using CastleHero.Utility;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Factory;

namespace CastleHero.View.InGame.Behaviours
{
    public class WaveRunner : MonoBehaviour, IDisposable, IWaveController
    {
        [FormerlySerializedAs("_config")]
        [SerializeField] private SpawnConfig config;

        public bool IsRunning { get; set; }

        public ReactiveProperty<bool> Completed { get; } = new(false);

        private int _totalSpawn;
        private int _totalDead;

        private WaveUpdate _main;
        private SpawnArea[] _areas;

        private IUpdate[] _updates;

        private bool _disposed = false;

        private UnitFactory _unitFactory;

        private void Awake()
        {
            _unitFactory = ServiceLocator.Instance.Get<UnitFactory>();

            this.SubscribeMessage<ReleaseEvent>(OnReceiveReleaseEvent);
            this.SubscribeMessage<GameFinished>(OnGameFinished);
        }

        private void OnGameFinished(GameFinished _) => IsRunning = false;

        private void OnReceiveReleaseEvent(ReleaseEvent data)
        {
            ++_totalDead;

            data.unit.DestroySelf();
        }

        public void Init(int groupId, IDBProvider db, IInGameSession inGameSession)
        {
            _disposed = false;
            IsRunning = false;
            Completed.Value = false;

            var waves = db.Waves.Map(groupId);
            var setUp = config.areaSetUp;
            int length = setUp.Length;

            _main = new WaveUpdate(waves);
            _areas = new SpawnArea[length];
            _updates = new IUpdate[length + 1];

            _updates[0] = _main;

            var castle = inGameSession.Castle.Value as UnitActor;
            for (int i = 0; i < length; ++i)
            {
                var data = setUp[i];
                var area = new SpawnArea(data.id, data.position, data.size, data.angle, db.Units, _unitFactory, castle);
                _areas[i] = area;
                _updates[i + 1] = area;
            }

            _totalSpawn = 0;
            length = waves.Length;
            for (int i = 0; i < length; ++i)
            {
                _totalSpawn += TotalCount(ref waves[i]);
            }
        }

        private int TotalCount(ref WaveEntity entity) => entity.counts.Sum();

        private void Update()
        {
            if (_disposed)
                return;

            if (!IsRunning)
                return;

            var delta = Time.deltaTime;
            foreach (var update in _updates)
            {
                update.ProcessUpdate(delta);
            }

            Completed.Value = _totalDead >= _totalSpawn;
        }

        public void Dispose()
        {
            if (_areas != null)
            {
                foreach (var area in _areas)
                {
                    area.Dispose();
                }
            }

            _main = null;
            _areas = null;

            _disposed = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (config == null)
                return;

            foreach (var data in config.areaSetUp)
            {
                DrawArea(data);
            }
        }

        private void DrawArea(SpawnAreaSetUp data)
        {
            var pos = data.position;
            var points = new Vector2[4];

            var halfSize = data.size * 0.5f;
            var angle = data.angle;
            points[0] = new Vector2(pos.x - halfSize, pos.y + 1f).Rotate(pos, angle);
            points[1] = new Vector2(pos.x + halfSize, pos.y + 1f).Rotate(pos, angle);
            points[2] = new Vector2(pos.x + halfSize, pos.y).Rotate(pos, angle);
            points[3] = new Vector2(pos.x - halfSize, pos.y).Rotate(pos, angle);

            Debug.DrawLine(points[0], points[1], Color.cyan);
            Debug.DrawLine(points[1], points[2], Color.cyan);
            Debug.DrawLine(points[2], points[3], Color.cyan);
            Debug.DrawLine(points[3], points[0], Color.cyan);

            Vector2 center = default;
            foreach (var point in points)
            {
                center += point;
            }

            center /= 4;

            var style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.cyan }
            };

            Handles.Label(center, $"{data.id}", style);
        }
#endif
    }
}
