using System;
using Unity.Entities;

namespace Code.CubeMarching.GeometryComponents
{
    public enum CombinerOperation : byte
    {
        Min = 0,
        Max = 1,
        SmoothMin = 2,
        SmoothSubtract = 3,
        Add = 4,
        Replace = 5,
        ReplaceMaterial = 6
    }

    public static class CombinerOperationExtensions
    {
        public static bool HasBlendFactor(this CombinerOperation combinerOperation)
        {
            switch (combinerOperation)
            {
                case CombinerOperation.Min:
                case CombinerOperation.Max:
                case CombinerOperation.Add:
                case CombinerOperation.Replace:
                case CombinerOperation.ReplaceMaterial:
                    return false;
                case CombinerOperation.SmoothMin:
                case CombinerOperation.SmoothSubtract:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(combinerOperation), combinerOperation, null);
            }
        }
    }

    [InternalBufferCapacity(128)]
    public struct CTerrainChunkCombinerChild : IBufferElementData
    {
        #region Public Fields

        public Entity SourceEntity;

        #endregion
    }
}