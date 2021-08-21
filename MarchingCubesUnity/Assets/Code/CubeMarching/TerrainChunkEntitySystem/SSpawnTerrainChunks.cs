using System;
using System.Runtime.CompilerServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.TerrainChunkSystem;
using Code.CubeMarching.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using CGenericTerrainModifier = Code.CubeMarching.Authoring.CGenericTerrainModifier;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public class SSpawnTerrainChunks : SystemBase
    {
        #region Static Stuff

        public const int TerrainChunkLength = 8;

        #endregion

        #region Unity methods

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EntityManager.DestroyEntity(GetSingletonEntity<TerrainChunkDataBuffer>());
        }

        #endregion

        #region Public methods

        public void SpawnCluster(int3 clusterPositionGS, int clusterIndex)
        {
            //spawn cluster
            var clusterEntity = EntityManager.CreateEntity(_clusterArchetype);
            EntityManager.SetComponentData(clusterEntity, new CClusterPosition {PositionGS = clusterPositionGS,ClusterIndex = clusterIndex});
            var clusterMesh = MeshGeneratorBuilder.GenerateClusterMesh();
            EntityManager.AddSharedComponentData(clusterEntity, clusterMesh);
            EntityManager.SetName(clusterEntity, "Cluster " + clusterPositionGS);
            EntityManager.AddSharedComponentData(clusterEntity, ClusterMeshGPUBuffers.CreateGPUData());
            
            //spawn terrain renderer
            var renderMeshDescriptor = new RenderMeshDescription(clusterMesh.mesh, Resources.Load<Material>("DefaultMaterial"), ShadowCastingMode.On, true);

            var rendererEntity = EntityManager.CreateEntity(typeof(ClusterChild),typeof(Translation));
            EntityManager.SetName(rendererEntity, "Cluster " + clusterPositionGS + " RenderMesh");
            RenderMeshUtility.AddComponents(rendererEntity, EntityManager, renderMeshDescriptor);
            EntityManager.SetComponentData(rendererEntity, new Translation() {Value = clusterPositionGS * 8});

            NativeArray<CClusterChildListElement> chunkList = new NativeArray<CClusterChildListElement>(512, Allocator.Temp);
            
            //spawn chunks for the cluster
            var createdChunks = EntityManager.CreateEntity(_chunkArchtype, 512, Allocator.Temp);
            for (var i = 0; i < createdChunks.Length; i++)
            {
                var chunkEntity = createdChunks[i];
                CTerrainChunkStaticData staticDataData = default;
                CTerrainChunkDynamicData dynamicData = default;
                CTerrainEntityChunkPosition chunkData = default;

                var clusterSize = new int3(8, 8, 8);
                chunkData.positionGS = new int3(i % clusterSize.x, i / clusterSize.y % clusterSize.y, i / (clusterSize.x * clusterSize.y)) + clusterPositionGS;
                chunkData.indexInCluster = i;

                EntityManager.SetComponentData(chunkEntity, chunkData);
                EntityManager.SetComponentData(chunkEntity, staticDataData);
                EntityManager.SetComponentData(chunkEntity, dynamicData);
                EntityManager.SetComponentData(chunkEntity, new ClusterChild {ClusterEntity = clusterEntity});
                chunkList[i] = new CClusterChildListElement() {Entity = chunkEntity};
            }

            EntityManager.GetBuffer<CClusterChildListElement>(clusterEntity).CopyFrom(chunkList);
            chunkList.Dispose();
        }

        #endregion

        #region Private Fields

        private EntityArchetype _chunkArchtype;
        private EntityArchetype _clusterArchetype;

        #endregion

        #region Protected methods

        protected override void OnCreate()
        {  
            base.OnCreate();
            _chunkArchtype = EntityManager.CreateArchetype(
                typeof(CTerrainEntityChunkPosition),
                typeof(CTerrainShapeCoverageMask),
                typeof(CTerrainChunkStaticData),
                typeof(CTerrainChunkDynamicData),
                typeof(ClusterChild));

            _clusterArchetype = EntityManager.CreateArchetype(
                typeof(CClusterPosition),
                typeof(TerrainInstruction),
                typeof(CTriangulationInstruction),
                typeof(CSubChunkWithTrianglesIndex),
                typeof(CClusterChildListElement));

            //spawn data holder            
            var entity = EntityManager.CreateEntity(typeof(TerrainChunkDataBuffer), typeof(TotalClusterCounts), typeof(TerrainChunkIndexMap));
            var totalClustersCount = new TotalClusterCounts() {Value = new int3(1, 1, 1)};
            EntityManager.SetComponentData(entity, totalClustersCount);
            var terrainChunkIndexMaps = this.GetSingletonBuffer<TerrainChunkIndexMap>();
            terrainChunkIndexMaps.ResizeUninitialized(totalClustersCount.Value.Volume() * 512);
            for (int i = 0; i < terrainChunkIndexMaps.Length; i++)
            {
                terrainChunkIndexMaps[i] = default;
            }
            
            int clusterIndex = 0;

            for (var x = 0; x < totalClustersCount.Value.x; x++)
            for (var y = 0; y < totalClustersCount.Value.y; y++)
            for (var z = 0; z < totalClustersCount.Value.z; z++)
            {
                SpawnCluster(new int3(x * 8, y * 8, z * 8), clusterIndex);
                clusterIndex++;
            } 
            
            var buffer = EntityManager.GetBuffer<TerrainChunkDataBuffer>(entity);
            buffer.Add(new TerrainChunkDataBuffer {Value = TerrainChunkData.Outside});
            buffer.Add(new TerrainChunkDataBuffer {Value = TerrainChunkData.Inside});

            
            Entities.ForEach((ref CTerrainChunkDynamicData distanceField) =>
            {
                distanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer = buffer.Length;
                //todo, this probably costs a lot of performance, since it constantly has to resize the array. 
                //better just count how much we need and resize ones
                buffer.Add(default);
            }).Run();
            
            Entities.ForEach((ref CTerrainChunkStaticData distanceField) =>
            {
                distanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer = buffer.Length;
                //todo, this probably costs a lot of performance, since it constantly has to resize the array. 
                //better just count how much we need and resize ones
                buffer.Add(default);
            }).Run();
        }

        protected override void OnUpdate()
        {
        }

        #endregion
    }

    public struct CClusterChildListElement : IBufferElementData
    {
        public Entity Entity;
    }

    public struct CClusterPosition : IComponentData
    {
        #region Public Fields

        public int ClusterIndex;
        public int3 PositionGS;
        public BitArray512 WriteMask;

        #endregion
    }

    [UpdateAfter(typeof(SSpawnTerrainChunks))]
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public class SCalculateSphereBounds : SystemBase
    {
        #region Protected methods

        protected override void OnUpdate()
        {
            Entities.ForEach((ref CTerrainModifierBounds bounds, in CGenericTerrainModifier terrainModifier, in Translation translation) =>
            {
                bounds.Bounds = terrainModifier.CalculateBounds(translation);
            }).WithBurst().ScheduleParallel();
        }

        #endregion
    }

    public struct GeometryShapeTranslationTuple
    {
        #region Public Fields

        public CTerrainMaterial TerrainMaterial;
        public CGenericTerrainModifier TerrainModifier;
        public CTerrainModifierTransformation Translation;

        #endregion

        public uint CalculateHash()
        {
            //todo add TerrainMaterial to hash ones there is a proper implementation
            uint hash = TerrainModifier.CalculateHash();
            hash.AddToHash(Translation.CalculateHash());
            return hash;
        }
    }

    public struct GizmosVisualization
    {
        #region Public Fields

        public float4 color;
        public bool IsSubChunk;
        public float4x4 transformation;

        #endregion
    }

    public static class Utils
    {
        #region Static Stuff

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 IndexToPositionWS(int i, int3 size)
        {
            var index = i;

            var x = index % size.x;
            var y = index / size.x % size.y;
            var z = index / (size.x * size.y);

            return new int3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PositionToIndex(int3 position, int3 size)
        {
            return position.x + position.y * size.x + position.z * size.x * size.y;
        }

        #endregion
    }

    public struct CTerrainEntityChunkPosition : IComponentData
    {
        #region Public Fields

        public int indexInCluster;
        public int3 positionGS;

        #endregion
    }

    public struct DistanceFieldChunkData
    {
        public byte ChunkInsideTerrain;
        public byte InnerDataMask;
        public int IndexInDistanceFieldBuffer;
        public bool HasData => InnerDataMask != 0;
        public uint CurrentGeometryInstructionsHash;
        public bool InstructionsChangedSinceLastFrame;
    }

    public struct ClusterChild : IComponentData
    {
        public Entity ClusterEntity;
    }

    public struct CTerrainChunkStaticData : IComponentData
    {
        #region Public Fields

        public DistanceFieldChunkData DistanceFieldChunkData;

        #endregion
    }

    public struct CTerrainChunkDynamicData : IComponentData
    {
        #region Public Fields

        public DistanceFieldChunkData DistanceFieldChunkData;

        #endregion
    }

    public struct TerrainInstruction : IBufferElementData
    {
        #region Public Fields

        public int CombinerDepth;
        public CGeometryCombiner Combiner;
        public BitArray512 CoverageMask;
        public int DependencyIndex;
        public TerrainInstructionType TerrainInstructionType;
        public GeometryShapeTranslationTuple TerrainShape;
        public CGenericTerrainTransformation TerrainTransformation;
        public WorldToLocal WorldToLocal;
        public uint Hash;

        #endregion

        public uint CalculateHash()
        {
            uint hash = math.hash(new uint3((uint) TerrainInstructionType, (uint) CombinerDepth, (uint) DependencyIndex));
            
            switch (TerrainInstructionType)
            {
                case TerrainInstructionType.Shape:
                    hash.AddToHash(TerrainShape.CalculateHash());
                    break;
                case TerrainInstructionType.Combiner:
                    hash.AddToHash(Combiner.CalculateHash());
                    break;
                case TerrainInstructionType.Transformation:
                    throw new NotImplementedException();
                    break;
                case TerrainInstructionType.None:
                case TerrainInstructionType.CopyOriginal:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return hash;
        }
    }

    public enum TerrainInstructionType : byte
    {
        None = 0,
        Shape = 1,
        Combiner = 2,
        Transformation = 3,
        CopyOriginal
    }

    public struct CTerrainModifierBounds : IComponentData
    {
        #region Public Fields

        public TerrainBounds Bounds;
        public int IndexInShapeMap;

        #endregion
    }
    public static class Int3Extensions
    {
        public static int Volume(this int3 vector)
        {
            return vector.x * vector.y * vector.z;
        }
    }

    public static class UintExtensions
    {
        public static void AddToHash(ref this uint a, uint b)
        {
            a = math.hash(new uint2(a, b));
        }
    }
}