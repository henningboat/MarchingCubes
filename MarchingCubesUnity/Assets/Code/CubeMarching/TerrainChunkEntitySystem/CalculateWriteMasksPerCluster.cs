using Code.CubeMarching.GeometryGraph;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.Utils;
using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public struct CalculateWriteMasksPerCluster
    {
        public void Execute(ref CClusterParameters clusterParameters)
        {
            clusterParameters.WriteMask = BitArray512.AllBitsTrue;
        }
    }
}