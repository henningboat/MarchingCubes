using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [GenerateAuthoringComponent]
    public struct CGeometryGraphPropertyValueProvider : IComponentData
    {
        public Float16 Value;

        public unsafe void CopyFromFloat4x4(ref float4x4 value)
        {
            CopyFrom(UnsafeUtility.AddressOf(ref value), 16);
        }
        
        public unsafe void CopyFrom(void* data, int floatComponentCount)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (floatComponentCount < 1 || floatComponentCount > 16)
            {
                throw new ArgumentOutOfRangeException();
            }
#endif
            var selfPointer = UnsafeUtility.AddressOf(ref this);
            for (int i = 0; i < floatComponentCount; i++)
            {
                UnsafeUtility.WriteArrayElement(selfPointer, i, ((float*) data)[i]);
            }
        }
    }
}