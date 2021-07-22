using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    //todo re-implement this functionality
    public struct CTerrainShapeCoverageMask : IBufferElementData
    {
        #region Public Fields

        public byte CoverageMask;

        #endregion
    }
}