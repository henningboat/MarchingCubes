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

            //spawn terrain renderer
            var renderMeshDescriptor = new RenderMeshDescription(clusterMesh.mesh, Resources.Load<Material>("DefaultMaterial"), ShadowCastingMode.On, true);

            var rendererEntity = EntityManager.CreateEntity(typeof(ClusterChild),typeof(Translation));
            EntityManager.SetName(rendererEntity, "Cluster " + clusterPositionGS + " RenderMesh");
            RenderMeshUtility.AddComponents(rendererEntity, EntityManager, renderMeshDescriptor);
            EntityManager.SetComponentData(rendererEntity, new Translation() {Value = clusterPositionGS * 8});
            
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
            }
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
                typeof(TriangulationPosition));

            //spawn data holder            
            var entity = EntityManager.CreateEntity(typeof(TerrainChunkDataBuffer),typeof(TotalClustersCount));
            var totalClustersCount = new TotalClustersCount() {Value = new int3(2, 1, 2)};
            EntityManager.SetComponentData(entity, totalClustersCount);
            
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

            Entities.ForEach((ref CTerrainChunkStaticData distanceField) =>
            {
                distanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer = buffer.Length;
                //todo, this probably costs a lot of performance, since it constantly has to resize the array. 
                //better just count how much we need and resize ones
                buffer.Add(default);
            }).Run();
            Entities.ForEach((ref CTerrainChunkDynamicData distanceField) =>
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

    public struct TriangulationPosition : IBufferElementData
    {
        public int3 position;
        public byte triangulationTableIndex;
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

    [UpdateAfter(typeof(SBuildStaticGeometry))]
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public class SPrepareGPUData : SystemBase
    {
        #region Properties

        public int3 IndexMapSize { get; private set; }

        #endregion

        #region Static Stuff

        public static TerrainChunkData ConvertDataForGPUFriendlyFormat(TerrainChunkData distanceFieldValue)
        {
            var result = new TerrainChunkData();
            for (var subChunkIndex = 0; subChunkIndex < 8; subChunkIndex++)
            {
                var positionOfSubChunk = new int3(subChunkIndex % 2, subChunkIndex / 2 % 2, subChunkIndex / 4) * 4;
                for (var indexInSubChunk = 0; indexInSubChunk < 16; indexInSubChunk++)
                {
                    var positionInsideSubChunk = new int3(0, indexInSubChunk % 4, indexInSubChunk / 4);

                    var totalPosition = positionInsideSubChunk + positionOfSubChunk;
                    var index = totalPosition.x + totalPosition.y * 8 + totalPosition.z * 8 * 8;

                    result[index / 4] = distanceFieldValue[subChunkIndex * 16 + indexInSubChunk];
                }
            }

            return result;
        }

        #endregion

        #region Unity methods

        protected override void OnDestroy()
        {
            TerrainChunkData.Dispose();
        }

        #endregion

        #region Public Fields

        public NativeArray<int> IndexMap;
        public NativeList<TerrainChunkData> TerrainChunkData;

        #endregion

        #region Protected methods

        protected override void OnStopRunning()
        {
            if (IndexMap.IsCreated)
            {
                IndexMap.Dispose();
            }
        }

        protected override void OnCreate()
        {
            TerrainChunkData = new NativeList<TerrainChunkData>(Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            var terrainChunkData = TerrainChunkData;

            if (IndexMap.IsCreated)
            {
                IndexMap.Dispose();
            }

            terrainChunkData.Clear();
            terrainChunkData.Add(TerrainChunkSystem.TerrainChunkData.Outside);
            terrainChunkData.Add(TerrainChunkSystem.TerrainChunkData.Inside);

            var i = new NativeValue<int>(Allocator.TempJob);
            i.Value = 2;

            IndexMapSize = 0;
            Entities.ForEach((in CClusterPosition clusterPosition) => { IndexMapSize = math.max(IndexMapSize, clusterPosition.PositionGS + 8); }).WithoutBurst().Run();

            var indexMapCapacity = IndexMapSize.x * IndexMapSize.y * IndexMapSize.z;
            IndexMap = new NativeArray<int>(indexMapCapacity, Allocator.TempJob);
            var indexMap = IndexMap;

            var terrainChunkBuffer = EntityManager.GetBuffer<TerrainChunkDataBuffer>(GetSingletonEntity<TerrainChunkDataBuffer>());

            var indexMapSize = IndexMapSize;
            Dependency = Entities.ForEach((CTerrainChunkStaticData staticDistanceField, CTerrainChunkDynamicData dynamicDistanceField, CTerrainEntityChunkPosition chunkPosition) =>
            {
                var indexInChunkMap = Utils.PositionToIndex(chunkPosition.positionGS, indexMapSize);


                var indexInDistanceFieldBuffer = 0;
                var hasData = false;
                if (dynamicDistanceField.DistanceFieldChunkData.HasData)
                {
                    indexInDistanceFieldBuffer = dynamicDistanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer;
                    hasData = true;
                }
                else
                {
                    indexInDistanceFieldBuffer = staticDistanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer;
                    hasData = true;
                }

                if (hasData)
                {
                    var convertedData = ConvertDataForGPUFriendlyFormat(terrainChunkBuffer[indexInDistanceFieldBuffer].Value);
                    terrainChunkData.Add(convertedData);
                    indexMap[indexInChunkMap] = i.Value;
                    i.Value++;
                }
                else
                {
                    if (staticDistanceField.DistanceFieldChunkData.ChunkInsideTerrain == 0)
                    {
                        indexMap[indexInChunkMap] = 0;
                    }
                    else
                    {
                        indexMap[indexInChunkMap] = 1;
                    }
                }
            }).Schedule(Dependency);

            i.Dispose(Dependency);
            //todo remove this
            Dependency.Complete();
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
            const int chunkLength = RenderCubeMarchingSystem.ChunkLength;

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

        #endregion
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
}