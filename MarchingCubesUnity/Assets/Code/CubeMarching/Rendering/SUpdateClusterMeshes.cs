using System.Collections.Generic;
using System.Drawing;
using Code.CubeMarching.AsyncReadbackSystem;
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
    public struct CTriangulationInstruction : IBufferElementData
    {
        public readonly int3 ChunkPositionGS;
        public readonly int SubChunkIndex;

        public CTriangulationInstruction(int3 chunkPositionGs, int subChunkIndex)
        {
            ChunkPositionGS = chunkPositionGs;
            SubChunkIndex = subChunkIndex;
        }
    }


    public struct CSubChunkWithTrianglesIndex : IBufferElementData
    {
        public readonly int3 ChunkPositionGS;
        public readonly int SubChunkIndex;

        public CSubChunkWithTrianglesIndex(int3 chunkPositionGs, int subChunkIndex)
        {
            ChunkPositionGS = chunkPositionGs;
            SubChunkIndex = subChunkIndex;
        }
    }

    public struct CVertexCountPerSubCluster : IBufferElementData
    {
        public int vertexCount;
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

            var gpuReadbackDatas = AsyncReadbackUtility.GetDataReadbacks();

            var gpuReadbackDataClusterIndex = new NativeArray<int>(gpuReadbackDatas.Count, Allocator.TempJob);
            var gpuReadbackDataFrameTimestamp = new NativeArray<int>(gpuReadbackDatas.Count, Allocator.TempJob);
            var gpuReadbackDataVertexCount = new NativeArray<int>(gpuReadbackDatas.Count * Constants.SubChunksInCluster, Allocator.TempJob);

            for (var i = 0; i < gpuReadbackDatas.Count; i++)
            {
                gpuReadbackDataClusterIndex[i] = gpuReadbackDatas[i].clusterIndex;
                gpuReadbackDataFrameTimestamp[i] = gpuReadbackDatas[i].frameTimestamp;
                new NativeSlice<int>(gpuReadbackDataVertexCount, Constants.SubChunksInCluster * i, Constants.SubChunksInCluster).CopyFrom(new NativeSlice<int>(gpuReadbackDatas[i].vertexCounts));
            }

            var frameCount = GetSingleton<CFrameCount>().Value;

            var getCVertexCountPerSubCluster = GetBufferFromEntity<CVertexCountPerSubCluster>();

            Dependency = Entities.ForEach((Entity entity, DynamicBuffer<CTriangulationInstruction> triangulationInstructions,
                    DynamicBuffer<CSubChunkWithTrianglesIndex> subChunkWithTriangles,
                    ref CClusterParameters clusterParameters, in CClusterPosition clusterPosition,
                    in DynamicBuffer<CClusterChildListElement> chunkEntities) =>
                {
                    clusterParameters.needsIndexBufferUpdate = false;
                    
                    triangulationInstructions.Clear();
                    subChunkWithTriangles.Clear();

                    NativeSlice<int> vertexCountData = default;
                    var hasVertexCountReadback = false;
                    var vertexCountReadbackTimesStamp = 0;

                    for (var i = 0; i < gpuReadbackDataClusterIndex.Length; i++)
                    {
                        if (gpuReadbackDataClusterIndex[i] == clusterPosition.ClusterIndex)
                        {
                            vertexCountData = new NativeSlice<int>(gpuReadbackDataVertexCount, Constants.SubChunksInCluster * i, Constants.SubChunksInCluster);
                            vertexCountReadbackTimesStamp = gpuReadbackDataFrameTimestamp[i];
                            hasVertexCountReadback = true;
                            break;
                        }
                    }

                    var totalVertexCount = 0;

                    var vertexCountPerSubChunk = getCVertexCountPerSubCluster[entity];

                    for (var chunkIndex = 0; chunkIndex < chunkEntities.Length; chunkIndex++)
                    {
                        var positionOfChunkWS = getChunkPosition[chunkEntities[chunkIndex].Entity].positionGS * 8;
                        var dynamicData = getDynamicData[chunkEntities[chunkIndex].Entity];

                        var currentHash = dynamicData.DistanceFieldChunkData.CurrentGeometryInstructionsHash;

                        for (var i = 0; i < 8; i++)
                        {
                            var subChunkIndex = chunkIndex * 8 + i;

                            if (hasVertexCountReadback)
                            {
                                if (dynamicData.DistanceFieldChunkData.InstructionChangeFrameCount <= vertexCountReadbackTimesStamp)
                                {
                                    vertexCountPerSubChunk[subChunkIndex] = new CVertexCountPerSubCluster() {vertexCount = vertexCountData[subChunkIndex]};
                                    clusterParameters.needsIndexBufferUpdate = true;
                                }
                            }

                            if (dynamicData.DistanceFieldChunkData.InnerDataMask.GetBit(i))
                            {
                                var subChunkOffset = TerrainChunkEntitySystem.Utils.IndexToPositionWS(i, 2) * 4;
                                subChunkWithTriangles.Add(new CSubChunkWithTrianglesIndex(positionOfChunkWS + subChunkOffset, 0));

                                if (dynamicData.DistanceFieldChunkData.InstructionsChangedSinceLastFrame)
                                {
                                    triangulationInstructions.Add(new CTriangulationInstruction(positionOfChunkWS + subChunkOffset, 0));
                                    vertexCountPerSubChunk[subChunkIndex] = new CVertexCountPerSubCluster() {vertexCount = Constants.maxVertsPerCluster};
                                    
                                    clusterParameters.needsIndexBufferUpdate = true;
                                }
 
                                totalVertexCount += vertexCountPerSubChunk[subChunkIndex].vertexCount;
                            }
                        }
                    }

                    clusterParameters.vertexCount = totalVertexCount;
                })
                .WithBurst().WithReadOnly(getChunkPosition).WithReadOnly(getDynamicData).WithReadOnly(gpuReadbackDataClusterIndex).WithReadOnly(gpuReadbackDataFrameTimestamp)
                .WithReadOnly(gpuReadbackDataVertexCount).WithNativeDisableParallelForRestriction(getCVertexCountPerSubCluster).WithName("CalculateTriangulationIndices").ScheduleParallel(Dependency);

            gpuReadbackDataClusterIndex.Dispose(Dependency);
            gpuReadbackDataFrameTimestamp.Dispose(Dependency);
            gpuReadbackDataVertexCount.Dispose(Dependency);

            foreach (var gpuReadbackData in gpuReadbackDatas)
            {
                gpuReadbackData.Dispose(Dependency);
            }

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
                DynamicBuffer<CSubChunkWithTrianglesIndex> subChunkWithTriangles, CClusterPosition clusterPosition, CClusterParameters clusterParameters) =>
            {
                gpuBuffers.UpdateWithSurfaceData(_distanceFieldComputeBuffer, _indexMapComputeBuffer, triangulationInstructions, subChunkWithTriangles, clusterCounts.Value, 0, clusterMesh.mesh,
                    clusterPosition, frameCount, clusterParameters);
            }).WithoutBurst().Run();

            // Entities.ForEach((DynamicBuffer<TerrainInstruction> instructions) =>
            // {
            //     string s = "";
            //     foreach (TerrainInstruction instruction in instructions)
            //     {
            //         s += instruction.Hash + "\n";
            //     }
            //     Debug.Log(s);
            // }).Run();
            
            clusterMeshRendererEntities.Dispose(Dependency);
            clusterEntities.Dispose(Dependency);
        }
    }

    public struct CClusterParameters : IComponentData
    {
        public bool needsIndexBufferUpdate;
        public BitArray512 WriteMask;
        public int vertexCount;
        public int lastVertexBufferChangeTimestamp;
    }
}