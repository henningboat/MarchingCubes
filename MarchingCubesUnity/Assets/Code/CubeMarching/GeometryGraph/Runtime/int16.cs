using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    public struct int16
    {
        private int4 c0;
        private int4 c1;
        private int4 c2;
        private int4 c3;

        public unsafe ref int this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 16)
                {
                    throw new System.ArgumentException("index must be between[0...15]");
                }
#endif
                fixed (int16* array = &this)
                {
                    return ref ((int*) array)[index];
                }
            }
        }

        public void AddOffset(int valueBufferOffset)
        {
            c0 += valueBufferOffset;
            c1 += valueBufferOffset;
            c2 += valueBufferOffset;
            c3 += valueBufferOffset;
        }
    }
}