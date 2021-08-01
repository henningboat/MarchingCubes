using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using JetBrains.Annotations;
using Unity.Transforms;

namespace Code.CubeMarching
{
    public interface ITerrainModifierShape
    {
        TerrainModifierType Type { get; }

        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS);

        [UsedImplicitly]
        TerrainBounds CalculateBounds(Translation translation);

        uint CalculateHash();
    }
}