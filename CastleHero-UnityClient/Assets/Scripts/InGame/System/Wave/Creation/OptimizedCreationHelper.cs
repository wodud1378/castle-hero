// using System;
// using System.Collections.Generic;
// using RGLabs.Data.DB;
// using RGLabs.Data.Model;
// using UniRx;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace RGLabs.InGame.System.Wave.Creation
// {
//     public class OptimizedCreationHelper : ICreationHelper
//     {
//         private struct SetSpaceBuffer : IJob
//         {
//             public NativeArray<UnitEntity> entities;
//             public NativeArray<EntitySpace> spaces;
//
//             public Vector2 cornerA;
//             public Vector2 cornerB;
//
//             private int _entityCount;
//             private int _blankCount;
//             private int _total;
//             private Vector2 _leftSpace;
//
//             public void Execute()
//             {
//                 _entityCount = entities.Length;
//                 _blankCount = Random.Range(2, 4);
//                 _total = _entityCount + _blankCount;
//                 _leftSpace = cornerB - cornerA;
//
//                 ApplyEntities();
//                 if (_leftSpace is not { x: > 0, y: > 0 })
//                     return;
//                 
//                 ApplyBlanks();
//             }
//
//             private void ApplyEntities()
//             {
//                 var direction = _leftSpace.normalized;
//                 spaces = new NativeArray<EntitySpace>(_total, Allocator.Temp);
//                 for (int i = 0; i < _entityCount; ++i)
//                 {
//                     var entity = entities[i];
//                     var size = new Vector2(entity.size, entity.size) * direction;
//                     _leftSpace -= size;
//
//                     spaces[i] = new EntitySpace
//                     {
//                         valid = true,
//                         index = i,
//                         size = size
//                     };
//                 }
//             }
//
//             private void ApplyBlanks()
//             {
//                 var blank = _leftSpace / _blankCount;
//                 for (int i = _entityCount; i < _total; ++i)
//                 {
//                     spaces[i] = new EntitySpace
//                     {
//                         valid = false,
//                         index = -1,
//                         size = blank
//                     };
//                 }
//             }
//         }
//
//         private struct SetCreationBuffer : IJob
//         {
//             public NativeArray<UnitEntity> entities;
//             public NativeArray<EntitySpace> spaces;
//             public Vector2 startPosition;
//
//             public NativeArray<UnitCreation> creations;
//
//             public void Execute()
//             {
//                 var list = new NativeList<int>(Allocator.Temp);
//                 int count = spaces.Length;
//                 for (int i = 0; i < count; ++i)
//                 {
//                     list.Add(i);
//                 }
//
//                 creations = new NativeArray<UnitCreation>(count, Allocator.Temp);
//
//                 int creationIndex = 0;
//                 Vector2 lastPosition = startPosition;
//                 Vector2 lastHalfSize = default;
//                 while (list.Length > 0)
//                 {
//                     int spaceIndex = Random.Range(0, list.Length);
//                     var space = spaces[spaceIndex];
//                     var halfSize = space.size * 0.5f;
//                     var position = lastPosition + lastHalfSize + halfSize;
//                     if (space.index == -1)
//                         continue;
//                     
//                     var creation = new UnitCreation
//                     {
//                         position = position,
//                         entity = entities[space.index]
//                     };
//                     
//                     creations[creationIndex++] = creation;
//                     
//                     lastPosition = position;
//                     lastHalfSize = halfSize;
//                 }
//             }
//         }
//
//         private struct EntitySpace
//         {
//             public bool valid;
//             public int index;
//             public Vector2 size;
//         }
//
//         private readonly int _areaId;
//         private readonly float _size;
//         private readonly Vector2 _offset;
//
//         private readonly UnitDB _db;
//         private readonly Queue<SpawnEvent> _queue;
//
//         private SetSpaceBuffer _setSpaceJob;
//         private SetCreationBuffer _setCreationJob;
//
//         public OptimizedCreationHelper(int areaId, UnitDB db, Vector2 cornerA, Vector2 cornerB)
//         {
//             _setSpaceJob = new()
//             {
//                 cornerA = cornerA,
//                 cornerB = cornerB,
//             };
//
//             _setCreationJob = new()
//             {
//                 startPosition = cornerA
//             };
//
//             _areaId = areaId;
//             _db = db;
//
//             _queue = new();
//
//             MessageBroker.Default.Receive<SpawnEvent[]>().Subscribe(OnReceiveSpawnEvents);
//         }
//
//         public void SetUpCreations(Action<UnitCreation> onCreation)
//         {
//             if (_queue.Count == 0)
//                 return;
//
//             var entities =GetEntityArray();
//             _setSpaceJob.entities = entities;
//             
//             var setSpaceHandle = _setSpaceJob.Schedule();
//             _setCreationJob.entities = entities;
//             _setCreationJob.spaces = _setSpaceJob.spaces;
//
//             var setCreationHandle = _setCreationJob.Schedule(setSpaceHandle);
//             setCreationHandle.Complete();
//
//             var creations = _setCreationJob.creations;
//             int count = creations.Length;
//             for (int i = 0; i < count; ++i)
//             {
//                 onCreation.Invoke(creations[i]);
//             }
//         }
//
//         private void OnReceiveSpawnEvents(SpawnEvent[] data)
//         {
//             foreach (var ev in data)
//             {
//                 if (ev.area == _areaId)
//                     _queue.Enqueue(ev);
//             }
//         }
//
//         private NativeArray<UnitEntity> GetEntityArray()
//         {
//             int count = _queue.Count;
//             int left = count;
//             int i = 0;
//             var array = new NativeArray<UnitEntity>(count, Allocator.Temp);
//             while (left > 0)
//             {
//                 var data = _queue.Dequeue();
//                 if (_db.TryFind(data.id, out var entity))
//                     array[i] = entity;
//
//                 --left;
//             }
//
//             return array;
//         }
//     }
// }