using System.Drawing;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TerrainUtils;
using Random = Unity.Mathematics.Random;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.TerrainChunkSystem;

namespace Code.CubeMarching.Rendering
{
    

    public struct CTriangulationInstruction:IBufferElementData
    {
        public readonly int3 ChunkPositionGS;
        public readonly int SubChunkIndex;

        public CTriangulationInstruction(int3 chunkPositionGs, int subChunkIndex)
        {
            ChunkPositionGS = chunkPositionGs;
            SubChunkIndex = subChunkIndex;
        }
    }
    
    

    public struct CSubChunkWithTrianglesIndex:IBufferElementData
    {  public readonly int3 ChunkPositionGS;
        public readonly int SubChunkIndex;

        public CSubChunkWithTrianglesIndex(int3 chunkPositionGs, int subChunkIndex)
        {
            ChunkPositionGS = chunkPositionGs;
            SubChunkIndex = subChunkIndex;
        }
    }

    [ExecuteAlways]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(SUpdateIndexMap))]
    public class SUpdateClusterMeshes : SystemBase
    {
        private int previousFrameClusterCount = -1;
        
        private ComputeBuffer _distanceFieldComputeBuffer;
        private ComputeBuffer _indexMapComputeBuffer;

        protected override void OnUpdate()
        {
            var clusterEntityQuery = GetEntityQuery(typeof(CClusterPosition));
            var clusterEntities = clusterEntityQuery.ToEntityArray(Allocator.TempJob);
            var clusterCount = clusterEntities.Length;

          
            previousFrameClusterCount = clusterCount;

            var getChunkPosition = GetComponentDataFromEntity<CTerrainEntityChunkPosition>(true);
            var getDynamicData = GetComponentDataFromEntity<CTerrainChunkDynamicData>(true);

            Dependency = Entities.ForEach(( DynamicBuffer<CTriangulationInstruction> triangulationInstructions, DynamicBuffer<CSubChunkWithTrianglesIndex> subChunkWithTriangles, in DynamicBuffer<CClusterChildListElement> chunkEntities) =>
                {
                    triangulationInstructions.Clear();
                    subChunkWithTriangles.Clear();

                    for (int chunkIndex = 0; chunkIndex < chunkEntities.Length; chunkIndex++)
                    {
                        int3 positionOfChunkWS = getChunkPosition[chunkEntities[chunkIndex].Entity].positionGS * 8;
                        var dynamicData = getDynamicData[chunkEntities[chunkIndex].Entity];

                        for (int i = 0; i < 8; i++)
                        {
                            if (dynamicData.DistanceFieldChunkData.InnerDataMask.GetBit(i))
                            {
                                int3 subChunkOffset = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 2) * 4;
                                subChunkWithTriangles.Add(new CSubChunkWithTrianglesIndex(positionOfChunkWS + subChunkOffset, 0));

                                if (dynamicData.DistanceFieldChunkData.InstructionsChangedSinceLastFrame)
                                {
                                    triangulationInstructions.Add(new CTriangulationInstruction(positionOfChunkWS + subChunkOffset, 0));
                                }
                            }
                        }
                    }
                })
                .WithBurst().WithReadOnly(getChunkPosition).WithReadOnly(getDynamicData).WithName("CalculateTriangulationIndices").
                ScheduleParallel(Dependency);
            
            Dependency.Complete();

            _distanceFieldComputeBuffer?.Dispose();
            _indexMapComputeBuffer?.Dispose();
 

            var terrainChunkDataBuffer = this.GetSingletonBuffer<TerrainChunkDataBuffer>().AsNativeArray().Reinterpret<TerrainChunkData>();
            _distanceFieldComputeBuffer = new ComputeBuffer(terrainChunkDataBuffer.Length * TerrainChunkData.PackedCapacity, 4 * 4 * 2);
            _distanceFieldComputeBuffer.SetData(terrainChunkDataBuffer);
    
    
            var indexMap = this.GetSingletonBuffer<TerrainChunkIndexMap>().Reinterpret<int>();
    
            _indexMapComputeBuffer = new ComputeBuffer(indexMap.Length, 4);
            _indexMapComputeBuffer.SetData(indexMap.AsNativeArray());
    
            var clusterMeshRendererEntities = GetEntityQuery(typeof(CClusterMesh)).ToEntityArray(Allocator.TempJob);


            var clusterCounts = GetSingleton<TotalClusterCounts>();

            Entities.ForEach((CClusterMesh clusterMesh, ClusterMeshGPUBuffers gpuBuffers, DynamicBuffer<CTriangulationInstruction> triangulationInstructions,
                DynamicBuffer<CSubChunkWithTrianglesIndex> subChunkWithTriangles, CClusterPosition clusterPosition) =>
            {
                gpuBuffers.UpdateWithSurfaceData(_distanceFieldComputeBuffer, _indexMapComputeBuffer, triangulationInstructions, subChunkWithTriangles, clusterCounts.Value, 0, clusterMesh.mesh,
                    clusterPosition.PositionGS * 8);
            }).WithoutBurst().Run();
            
            
            clusterMeshRendererEntities.Dispose(Dependency);
            clusterEntities.Dispose(Dependency);
        }
    }
}