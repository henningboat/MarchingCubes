using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using static Code.SIMDMath.SimdMath;

namespace Code.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericDistanceModification: IDistanceModification
    {
        public int16 Data;
        public DistanceModificationType Type;
        
        public PackedFloat GetSurfaceDistance(PackedFloat surfaceDistance, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (Type)
                {
                    case DistanceModificationType.Onion:
                        return ((OnionDistanceModification*) ptr)->GetSurfaceDistance(surfaceDistance, valueBuffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public struct OnionDistanceModification: IDistanceModification
    {
        public FloatValue Thickness;

        public PackedFloat GetSurfaceDistance(PackedFloat currentDistance, NativeArray<float> valueBuffer)
        {
            return abs(currentDistance) - Thickness.Resolve(valueBuffer);
        }
    }

    public interface IDistanceModification
    {
        PackedFloat GetSurfaceDistance(PackedFloat currentDistance, NativeArray<float> valueBuffer);
    }
}