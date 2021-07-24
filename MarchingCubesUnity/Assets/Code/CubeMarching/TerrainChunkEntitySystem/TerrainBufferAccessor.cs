﻿using Code.CubeMarching.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public struct TerrainBufferAccessor
    {
        public readonly DynamicBuffer<TerrainChunkDataBuffer> DataBuffer;
        //public readonly DynamicBuffer<TerrainChunkIndexMap> IndexBuffer;
        public readonly TotalClustersCount ClustersCount;

        private const int clusterLength = 64;
        private const int chunkLength = 8;
        private const int chunksInCluster = 512;

        public TerrainBufferAccessor(SystemBase systemBase)
        {
            DataBuffer = systemBase.GetSingletonBuffer<TerrainChunkDataBuffer>();
            //IndexBuffer = systemBase.GetSingletonBuffer<TerrainChunkIndexMap>();
            ClustersCount = systemBase.GetSingleton<TotalClustersCount>();
        }

        public float GetSurfaceDistance(int3 positionWS)
        {
            positionWS = clamp(positionWS, 0, ClustersCount.Value);
            
            int indexInDataBuffer = 0;
            int clusterIndex = Utils.PositionToIndex(positionWS / 64, ClustersCount.Value);
            int indexInCluster = Utils.PositionToIndex(positionWS % clusterLength, 8);

            TerrainChunkDataBuffer terrainChunkDataBuffer = DataBuffer[clusterIndex * chunksInCluster + indexInCluster];

            return GetPointPosition(positionWS % chunkLength, terrainChunkDataBuffer);
        }

        private static float GetPointPosition(int3 positionWithinTerrainChunk, TerrainChunkDataBuffer chunk)
        {
            int subChunkIndex = Utils.PositionToIndex(positionWithinTerrainChunk / 4, 2);
            int indexWithinSubChunk = Utils.PositionToIndex(positionWithinTerrainChunk % 4, 4);
            var indexWithinTerrainChunk = subChunkIndex * 64 + indexWithinSubChunk;

            var indexInTerrainBuffer = indexWithinTerrainChunk;
            return chunk.Value[indexInTerrainBuffer / 4][indexInTerrainBuffer % 4].SurfaceDistance;
        }
    }
}