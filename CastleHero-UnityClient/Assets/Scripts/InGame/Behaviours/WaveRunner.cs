using System;
using System.Linq;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.Data;
using RGLabs.InGame.System;
using RGLabs.InGame.System.Wave;
using RGLabs.Utility;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public class WaveRunner : MonoBehaviour, IDisposable
    {
        [Serializable]
        public struct SpawnAreaSetUp
        {
            public int id;
            public Vector2 position;
            public float size;
            public float angle;
        }

        [SerializeField] private SpawnConfig _config;

        [NonSerialized] public bool isRunning;
        
        public readonly ReactiveProperty<bool> completed = new(false);

        private int _totalSpawn;
        private int _totalDead;
        
        private WaveUpdate _main;
        private SpawnArea[] _areas;

        private IUpdate[] _updates;

        private bool _disposed = false;

        private void Awake()
        {
            this.SubscribeMessage<ReleaseEvent>(OnReceiveReleaseEvent);
            this.SubscribeMessage<GameFinished>(OnGameFinished);
        }

        private void OnGameFinished(GameFinished _)=> isRunning = false;
        
        private void OnReceiveReleaseEvent(ReleaseEvent data)
        {
            ++_totalDead;
            
            data.unit.DestroySelf();
        }

        public void Init(int groupId)
        {
            _disposed = false;
            isRunning = false;
            completed.Value = false;
            
            var db = Storage.db;
            var waves = db.waves.Map(groupId);
            var setUp = _config.areaSetUp;
            int length = setUp.Length;
            
            _main = new WaveUpdate(waves);
            _areas = new SpawnArea[length];
            _updates = new IUpdate[length + 1];

            _updates[0] = _main;

            var factory = Context.unitFactory;
            var castle = Storage.inGameRepository.castle.Value;
            for (int i = 0; i < length; ++i)
            {
                var data = setUp[i];
                var area = new SpawnArea(data.id, data.position, data.size, data.angle, db.units, factory, castle);
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
            
            if (!isRunning)
                return;
            
            var delta = Time.deltaTime;
            foreach (var update in _updates)
            {
                update.ProcessUpdate(delta);
            }

            completed.Value = _totalDead >= _totalSpawn;
        }

        public void Dispose()
        {
            _main = null;
            _areas = null;
            
            _disposed = true;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_config == null)
                return;
            
            foreach (var data in _config.areaSetUp)
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