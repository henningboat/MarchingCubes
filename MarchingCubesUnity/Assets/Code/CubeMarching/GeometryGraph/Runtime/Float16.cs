using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    public struct float16
    {
        private float4 c0;
        private float4 c1;
        private float4 c2;
        private float4 c3;

        public unsafe ref float this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 16)
                {
                    throw new System.ArgumentException("index must be between[0...15]");
                }
#endif
                fixed (float16* array = &this)
                {
                    return ref ((float*) array)[index];
                }
            }
        }
    }
}