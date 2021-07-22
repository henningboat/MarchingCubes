//todo reimplement physics system from the old repo

// using Code.CubeMarching;
// using Code.CubeMarching.Authoring;
// using Code.CubeMarching.TerrainChunkEntitySystem;
// using Code.CubeMarching.TerrainChunkSystem;
// using Code.SIMDMath;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Physics;
// using Unity.Physics.Authoring;
// using Unity.Rendering;
// using Unity.Transforms;
// using UnityEngine;
// using UnityEngine.Profiling;
// using UnityEngine.Rendering;
// using TerrainCollider = Unity.Physics.TerrainCollider;
//
// namespace Code
// {
//     [AlwaysUpdateSystem]
//     [UpdateInGroup(typeof(UpdateTerrainSystemGroup))]
//     [UpdateAfter(typeof(SBuildStaticGeometry))]
//     public class PhysicsTerrainSpawningSystem : SystemBase
//     {
//         private NativeArray<int> _detailBufferMapping;
//
//         protected override void OnCreate()
//         {
//             _detailBufferMapping=new NativeArray<int>(512, Allocator.Persistent);
//             base.OnCreate();
//         }
//
//         private PhysicsCollider CreateTerrainCollider()
//         {
//             var detailBufferList =  this.GetSingletonBuffer<TerrainChunkDataBuffer>();
//
//             var detailBufferMapping = _detailBufferMapping;
//             Dependency = Entities.ForEach((CTerrainChunkStaticData staticData, CTerrainEntityChunkPosition position) =>
//             {
//                 detailBufferMapping[position.indexInCluster] = staticData.DistanceFieldChunkData.IndexInDistanceFieldBuffer;
//             }).WithNativeDisableParallelForRestriction(detailBufferMapping).WithBurst().ScheduleParallel(Dependency);
//             
//             Dependency.Complete();
//
//             var physicsCollider = new PhysicsCollider();
//
//             Profiler.BeginSample("SetupTerrainCollider");
//             {
//
//                 NativeArray<TerrainChunkData> terrainChunkDatas = detailBufferList.AsNativeArray().Reinterpret<TerrainChunkData>();
//                 physicsCollider.Value = TerrainCollider.Create(CollisionFilter.Default, terrainChunkDatas, detailBufferMapping);
//             }
//
//             
//             Profiler.EndSample();
//
//             return physicsCollider;
//         }
//
//         private bool _initialized;
//         private Entity _entity;
//
//         protected override void OnUpdate()
//         {
//             Dependency.Complete();
//             if (!_initialized)
//             {
//                 EntityManager.DestroyEntity(GetEntityQuery(typeof(PhysicsCollider), typeof(TerrainColliderTag)));
//
//                 _entity = EntityManager.CreateEntity();
//                 EntityManager.SetName(_entity, "TerrainEntity");
//                 EntityManager.AddComponentData(_entity, new Translation() {Value = 0});
//                 EntityManager.AddComponentData(_entity, new LocalToWorld() {Value = float4x4.identity});
//                 EntityManager.AddComponentData(_entity, new Rotation() {Value = quaternion.identity});
//                 EntityManager.AddComponentData(_entity, new TerrainColliderTag());
//                 EntityManager.AddComponentData(_entity, CreateTerrainCollider());
//
//                 _initialized = true;
//             }
//             else
//             {
//
//                 //EntityManager.SetComponentData(_entity, CreateTerrainCollider());
//             }
//         }
//
//         protected override void OnDestroy()
//         {
//             base.OnDestroy();
//             _detailBufferMapping.Dispose(Dependency);
//         }
//     }
//
//     public struct TerrainColliderTag:IComponentData
//     {
//         
//     }
// }