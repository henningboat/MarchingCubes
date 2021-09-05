using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryComponents
{
    public struct CGeometryCombiner : IComponentData
    {
        #region Public Fields

        public float BlendFactor;
        public CombinerOperation Operation;

        #endregion

        public uint CalculateHash()
        {
            var hash = math.asuint(BlendFactor);
            hash.AddToHash((uint) Operation);
            return hash;
        }
    }
}