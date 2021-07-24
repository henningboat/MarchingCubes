using System;
using System.Drawing;
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

namespace Code.CubeMarching.Rendering
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(SPrepareGPUData))]
    public class UpdateClusterMeshes : SystemBase
    {
        private int previousFrameClusterCount = -1;

        private ParallelSubListCollection<TriangulationPosition> _triangulationIndices;
        
        protected override void OnDestroy()
        {
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

                var clusterMeshRendererEntities = GetEntityQuery(typeof(CClusterMesh)).ToEntityArray(Allocator.TempJob);

                for (int i = 0; i < clusterCount; i++)
                {
                    var clusterMesh = EntityManager.GetSharedComponentData<CClusterMesh>(clusterMeshRendererEntities[i]);
                    int triangleCount = _triangulationIndices.ReadListLength(i);
                    
                    NativeArray<int> dummyIndexBuffer = new NativeArray<int>(triangleCount,Allocator.TempJob);
                    for (int j = 0; j < dummyIndexBuffer.Length; j++)
                    {
                        var triangulationPosition = _triangulationIndices.ReadListValue(i, j);
                        dummyIndexBuffer[j] = Code.CubeMarching.TerrainChunkEntitySystem.Utils.PositionToIndex(triangulationPosition.position, 64)*3+triangulationPosition.triangulationTableIndex;
                    }

                     var gpuIndexBuffer = clusterMesh.mesh.GetIndexBuffer();
                     gpuIndexBuffer.SetData(dummyIndexBuffer);
                     clusterMesh.mesh.SetSubMeshes(new []{new SubMeshDescriptor(0,dummyIndexBuffer.Length)}, MeshGeneratorBuilder.MeshUpdateFlagsNone);
                     gpuIndexBuffer.Dispose();
                     dummyIndexBuffer.Dispose();
                }
                
                clusterMeshRendererEntities.Dispose(Dependency);
                clusterEntities.Dispose(Dependency);
            }
        }
    }
}