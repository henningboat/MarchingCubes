using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Code.CubeMarching.StateHashing
{
    public struct GeometryInstructionsHasher
    {
        private readonly BufferFromEntity<TerrainInstruction> _getTerrainInstructions;
        private readonly ComponentDataFromEntity<CClusterPosition> _getClusterPosition;

        public GeometryInstructionsHasher(SystemBase systemBase)
        {
            _getTerrainInstructions = systemBase.GetBufferFromEntity<TerrainInstruction>(true);
            _getClusterPosition = systemBase.GetComponentDataFromEntity<CClusterPosition>();
        }

        public void Execute(ref DistanceFieldChunkData chunk, in CTerrainEntityChunkPosition chunkPosition,in ClusterChild clusterChild)
        {
            uint hash = 0;
            var instructions = _getTerrainInstructions[clusterChild.ClusterEntity];
            var clusterPosition = _getClusterPosition[clusterChild.ClusterEntity];

            var indexInCluster = chunkPosition.indexInCluster;
            
            if(clusterPosition.WriteMask[indexInCluster])
            {
                foreach (var terrainInstruction in instructions)
                {
                    if (terrainInstruction.CoverageMask[indexInCluster])
                    {
                        hash.AddToHash(terrainInstruction.Hash);
                    }
                }
            }

            var hashChanged = chunk.CurrentGeometryInstructionsHash != hash;
            chunk.InstructionsChangedSinceLastFrame = hashChanged;
            chunk.CurrentGeometryInstructionsHash = hash;
        }
    }
}