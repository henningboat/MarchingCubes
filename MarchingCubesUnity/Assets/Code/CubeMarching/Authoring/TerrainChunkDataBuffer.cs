using System;
using Code.CubeMarching.TerrainChunkSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.Authoring
{
    [Serializable]
    public struct TerrainChunkDataBuffer : IBufferElementData
    {
        public TerrainChunkData Value;
    }

    [Serializable]
    public struct TerrainChunkIndexMap : IBufferElementData
    {
        public int Index;
    }

    public struct TotalClustersCount : IComponentData
    {
        public int3 Value;
    }
}