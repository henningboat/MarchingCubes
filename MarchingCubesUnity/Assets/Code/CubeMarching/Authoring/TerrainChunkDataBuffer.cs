using System;
using Code.CubeMarching.TerrainChunkSystem;
using Unity.Entities;

namespace Code.CubeMarching.Authoring
{
    [Serializable]
    public struct TerrainChunkDataBuffer : IBufferElementData
    {
        public TerrainChunkData Value;
    }
}