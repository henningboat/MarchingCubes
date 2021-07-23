using System.Drawing;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

namespace Code.CubeMarching.Rendering
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
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

                Dependency = Entities.ForEach((CTerrainEntityChunkPosition chunkPosition, CTerrainChunkStaticData staticData, CTerrainChunkDynamicData dynamicData, ClusterChild clusterChild) =>
                    {
                        if (staticData.DistanceFieldChunkData.HasData || dynamicData.DistanceFieldChunkData.HasData)
                        {
                            Random random = Random.CreateFromIndex((uint)chunkPosition.indexInCluster);
                            
                            var clusterPosition = getClusterPosition[clusterChild.ClusterEntity];
                            for (int i = 0; i < 512*3; i++)
                            {
                                parallelSubListCollection.Write(clusterPosition.ClusterIndex, chunkPosition.indexInCluster,
                                    new TriangulationPosition {position = chunkPosition.positionGS * 8 + new int3(random.NextFloat3(0, 8)), triangulationTableIndex = 1});
                            }
                        }
                    })
                    //.WithNativeDisableParallelForRestriction(triangulationDataPerChunk)
                    .WithReadOnly(getClusterPosition)
                    .WithBurst().ScheduleParallel(Dependency);

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
                        dummyIndexBuffer[j] = j;
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