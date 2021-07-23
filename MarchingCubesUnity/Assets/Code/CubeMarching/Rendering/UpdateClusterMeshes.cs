using System.Drawing;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

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

                const int maxTriangleCountPerCluster = 1310720;
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
                            var clusterPosition = getClusterPosition[clusterChild.ClusterEntity];
                            for (int i = 0; i < 512*5; i++)
                            {
                                parallelSubListCollection.Write(clusterPosition.ClusterIndex, chunkPosition.indexInCluster,
                                    new TriangulationPosition {position = chunkPosition.positionGS * 8 + i, triangulationTableIndex = 1});
                            }
                        }
                    })
                    //.WithNativeDisableParallelForRestriction(triangulationDataPerChunk)
                    .WithReadOnly(getClusterPosition)
                    .WithBurst().ScheduleParallel(Dependency);

                Dependency = parallelSubListCollection.ScheduleListCollapse(Dependency);

                const int numChunksInCluster = 512;

                // Dependency.Complete();
                //
                // string log = "";
                // for (int i = 0; i < clusterCount; i++)
                // {
                //     log += parallelSubListCollection.ReadListLength(i) + "  ";
                // }
                //
                // Debug.Log(log);

                clusterEntities.Dispose(Dependency);
            }
        }
    }
}