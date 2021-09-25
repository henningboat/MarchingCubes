using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Transforms;

namespace Code.CubeMarching.GeometryComponents
{
    public interface ITerrainModifierShape
    {
        ShapeType Type { get; }

        [UsedImplicitly]
        PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer);

        [UsedImplicitly]
        TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer);

        uint CalculateHash();
    }
}