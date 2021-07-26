using System;
using System.Drawing;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TerrainUtils;
using Random = Unity.Mathematics.Random;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.TerrainChunkSystem;

namespace Code.CubeMarching.Rendering
{
    [Serializable]
    struct TriangulationComputeShaderInstruction
    {
        public readonly int3 ChunkPositionGS;
        public readonly int SubChunkIndex;

        public TriangulationComputeShaderInstruction(int3 chunkPositionGs, int subChunkIndex)
        {
            ChunkPositionGS = chunkPositionGs;
            SubChunkIndex = subChunkIndex;
        }
    }

    [ExecuteAlways]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(SPrepareGPUData))]
    public class UpdateClusterMeshes : SystemBase
    {
        private int previousFrameClusterCount = -1;

        private ParallelSubListCollection<TriangulationPosition> _triangulationIndices;
        private const int MaxIndexCountPerSubChunk = 4 * 4 * 4 * 5 * 3; //

        private ComputeBuffer _distanceFieldComputeBuffer;
        private ComputeBuffer _indexMapComputeBuffer;

        private NewTerrainChunkGPUData _gpuData;

        protected override void OnCreate()
        {
            _gpuData = new NewTerrainChunkGPUData();
        }

        protected override void OnDestroy()
        {
            _gpuData.Dispose();
            _triangulationIndices.Dispose();
        }

        protected override void OnUpdate()
        {
            unsafe
            {
                var clusterEntityQuery = GetEntityQuery(typeof(CClusterPosition));
                var clusterEntities = clusterEntityQuery.ToEntityArray(Allocator.TempJob);
                var clusterCount = clusterEntities.Length;

                if (previousFrameClusterCount != clusterCount)
                {

                    if (_triangulationIndices.IsCreated)
                    {
                        _triangulationIndices.Dispose();
                    }

                    _triangulationIndices = new ParallelSubListCollection<TriangulationPosition>(clusterCount, 512, 512 * 5);
                }

                _triangulationIndices.Reset();
                previousFrameClusterCount = clusterCount;
                var getClusterPosition = GetComponentDataFromEntity<CClusterPosition>();
                
                var parallelSubListCollection = _triangulationIndices;

                TerrainBufferAccessor accessor = new TerrainBufferAccessor(this);

                var clustersCount = GetSingleton<TotalClustersCount>();


                NativeList<TriangulationComputeShaderInstruction> triangulationInstructions = new NativeList<TriangulationComputeShaderInstruction>(clusterCount * 512 * 8, Allocator.TempJob);
                var triangulationListWriter = triangulationInstructions.AsParallelWriter();

                Dependency = Entities.ForEach((CTerrainEntityChunkPosition chunkPosition, CTerrainChunkStaticData staticData, CTerrainChunkDynamicData dynamicData, ClusterChild clusterChild) =>
                    {
                        if (dynamicData.DistanceFieldChunkData.HasData)
                        {
                            var clusterIndex = getClusterPosition[clusterChild.ClusterEntity];
                            
                            int3 positionOfChunkWS = chunkPosition.positionGS * 8;

                            for (int subChunkIndex = 0; subChunkIndex < 8; subChunkIndex++)
                            {
                                if (!dynamicData.DistanceFieldChunkData.InnerDataMask.GetBit(subChunkIndex))
                                {
                                    continue;
                                }

                                triangulationListWriter.AddNoResize(new TriangulationComputeShaderInstruction(positionOfChunkWS / 8, subChunkIndex));
                                
                                int3 subChunkPositionInChunk = TerrainChunkEntitySystem.Utils.IndexToPositionWS(subChunkIndex, 2) * 4;
                                const int subChunkCapacity = 64;
                                for (int i = 0; i < subChunkCapacity; i++)
                                {
                                    int3 positionInSubChunk = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 4);
                                    int3 positionWS = positionOfChunkWS + subChunkPositionInChunk + positionInSubChunk;
                                
                                    byte triangulationIndex = 0;
                                    triangulationIndex.SetBit(0, accessor.GetSurfaceDistance(positionWS + new int3(0, 0, 0)) > 0);
                                    triangulationIndex.SetBit(1, accessor.GetSurfaceDistance(positionWS + new int3(1, 0, 0)) > 0);
                                    triangulationIndex.SetBit(2, accessor.GetSurfaceDistance(positionWS + new int3(1, 0, 1)) > 0);
                                    triangulationIndex.SetBit(3, accessor.GetSurfaceDistance(positionWS + new int3(0, 0, 1)) > 0);
                                    triangulationIndex.SetBit(4, accessor.GetSurfaceDistance(positionWS + new int3(0, 1, 0)) > 0);
                                    triangulationIndex.SetBit(5, accessor.GetSurfaceDistance(positionWS + new int3(1, 1, 0)) > 0);
                                    triangulationIndex.SetBit(6, accessor.GetSurfaceDistance(positionWS + new int3(1, 1, 1)) > 0);
                                    triangulationIndex.SetBit(7, accessor.GetSurfaceDistance(positionWS + new int3(0, 1, 1)) > 0);
                                    
                                    if(triangulationIndex==0||triangulationIndex==255)
                                        continue;
                                    
                                    parallelSubListCollection.Write(clusterIndex.ClusterIndex,chunkPosition.indexInCluster,new TriangulationPosition(){position = positionWS,triangulationTableIndex = 0});
                                    parallelSubListCollection.Write(clusterIndex.ClusterIndex,chunkPosition.indexInCluster,new TriangulationPosition(){position = positionWS,triangulationTableIndex = 1});
                                    parallelSubListCollection.Write(clusterIndex.ClusterIndex,chunkPosition.indexInCluster,new TriangulationPosition(){position = positionWS,triangulationTableIndex = 2});
                                }
                            }
                        }
                    })
                    .WithReadOnly(getClusterPosition).WithReadOnly(accessor)
                    .WithBurst().WithName("CalculateTriangulationIndices").
                    ScheduleParallel(Dependency);
 
                Dependency = parallelSubListCollection.ScheduleListCollapse(Dependency);

                Dependency.Complete();

                Debug.Log(triangulationInstructions.Length);

                _distanceFieldComputeBuffer?.Dispose();
                _indexMapComputeBuffer?.Dispose();
    
                //todo rename
                var newSystem = World.GetExistingSystem<SPrepareGPUData>();
    
                _distanceFieldComputeBuffer = new ComputeBuffer(newSystem.TerrainChunkData.Length * TerrainChunkData.UnPackedCapacity, TerrainChunkData.UnpackedElementSize);
                _distanceFieldComputeBuffer.SetData(newSystem.TerrainChunkData.AsArray());
    
                var terrainSize = newSystem.IndexMapSize;
    
                var indexMap = this.GetSingletonBuffer<TerrainChunkIndexMap>().Reinterpret<int>();
    
                _indexMapComputeBuffer = new ComputeBuffer(indexMap.Length, 4);
                _indexMapComputeBuffer.SetData(indexMap.AsNativeArray());
    
                var terrainChunkPositionsToRender = new NativeList<int3>(Allocator.Temp);
    
               


                        var clusterMeshRendererEntities = GetEntityQuery(typeof(CClusterMesh)).ToEntityArray(Allocator.TempJob);

                for (int i = 0; i < clusterCount; i++)
                {
                    var clusterMesh = EntityManager.GetSharedComponentData<CClusterMesh>(clusterMeshRendererEntities[i]);

                    _gpuData.UpdateWithSurfaceData(_distanceFieldComputeBuffer,_indexMapComputeBuffer,triangulationInstructions,clustersCount.Value,0,clusterMesh.mesh);

                    clusterMesh.mesh.SetSubMeshes(new[] {new SubMeshDescriptor(0, clusterMesh.mesh.vertexCount)}, MeshGeneratorBuilder.MeshUpdateFlagsNone);
                }

                clusterMeshRendererEntities.Dispose(Dependency);
                clusterEntities.Dispose(Dependency);
                triangulationInstructions.Dispose();
            }
        }
        
        
    }
    
    //todo convert to static class
       internal class NewTerrainChunkGPUData
    {
        private readonly ComputeShader _computeShader;
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _trianglePositionBuffer;
        private ComputeBuffer _trianglePositionCountBuffer;
        private ComputeBuffer _chunksToTriangulize;

        public NewTerrainChunkGPUData()
        {
            _computeShader = DynamicCubeMarchingSettingsHolder.Instance.Compute;
            _argsBuffer = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(new[] {3, 0, 0, 0});
            _trianglePositionCountBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);
            _trianglePositionCountBuffer.SetData(new[] {1, 1, 1, 1, 1});
            
            //todo resize to proper size
            _chunksToTriangulize = new ComputeBuffer(10000, 4 * 4, ComputeBufferType.Default);
        }

        public void UpdateWithSurfaceData(ComputeBuffer globalTerrainBuffer, ComputeBuffer globalTerrainIndexMap, NativeList<TriangulationComputeShaderInstruction> triangulationInstructions, int3 terrainMapSize, int materialIDFilter, Mesh mesh)
        {
            var trianbgleByteSize = (3 + 3 + 4) * 4;
            var requiredTriangleCapacity = triangulationInstructions.Length * 4 * 4 * 4 * 5;
            if (_trianglePositionBuffer == null || _trianglePositionBuffer.count < requiredTriangleCapacity)
            {
                if (_trianglePositionBuffer != null)
                {
                    _trianglePositionBuffer.Dispose();
                }

                _trianglePositionBuffer = new ComputeBuffer(requiredTriangleCapacity, 4 * 4, ComputeBufferType.Append);
            }
            _chunksToTriangulize.SetData(triangulationInstructions.AsArray());

            //Fine positions in the grid that contain triangles
            var getPositionKernel = _computeShader.FindKernel("GetTrianglePositions");
            _computeShader.SetInt("numPointsPerAxis", ChunkLength);
            _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
            _computeShader.SetInts("_TerrainMapSize", terrainMapSize.x, terrainMapSize.y, terrainMapSize.z);
            _computeShader.SetBuffer(getPositionKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
            _computeShader.SetBuffer(getPositionKernel, "_ValidTrianglePositions", _trianglePositionBuffer);
            _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
            _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
            _trianglePositionBuffer.SetCounterValue(0);
            _computeShader.Dispatch(getPositionKernel, triangulationInstructions.Length, 1, 1);
            ComputeBuffer.CopyCount(_trianglePositionBuffer, _trianglePositionCountBuffer, 0);

            
            var meshVertexBuffer = mesh.GetVertexBuffer(0);
            
            var clearVertexData = _computeShader.FindKernel("ClearVertexData");
            _computeShader.SetBuffer(clearVertexData,  "triangles", meshVertexBuffer);
            _computeShader.Dispatch(clearVertexData, mesh.vertexCount / 512, 1, 1);
            
            var calculateTriangulationThreadGroupSizeKernel = _computeShader.FindKernel("CalculateTriangulationThreadGroupSizeKernel");
            _computeShader.SetBuffer(calculateTriangulationThreadGroupSizeKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
            _computeShader.Dispatch(calculateTriangulationThreadGroupSizeKernel, 1, 1, 1);

            var triangulationKernel = _computeShader.FindKernel("Triangulation");
            _computeShader.SetInt("numPointsPerAxis", ChunkLength);
            _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
            _computeShader.SetInts("_TerrainMapSize", terrainMapSize.x, terrainMapSize.y, terrainMapSize.z);


            
            _computeShader.SetBuffer(triangulationKernel, "triangles", meshVertexBuffer);
            _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
            _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
            _computeShader.SetBuffer(triangulationKernel, "_ValidTrianglePositionResults", _trianglePositionBuffer);
            _computeShader.SetBuffer(triangulationKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
            _computeShader.DispatchIndirect(triangulationKernel, _trianglePositionCountBuffer, 4);
            
            meshVertexBuffer.Dispose();
        }

        public const int ChunkLength = 8;
        

        public void Dispose()
        {
            _argsBuffer.Dispose();
            _trianglePositionBuffer.Dispose();
            _chunksToTriangulize.Dispose();
            _trianglePositionCountBuffer.Dispose();
        }
    }
}